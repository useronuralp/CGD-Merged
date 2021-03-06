using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using System.Collections.Generic;
/// <summary>
/// This script manages the third (build index 2) scene of the game.
/// </summary>
namespace Game
{
    public class HubManager : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private GameObject                        m_PlayerPrefab;
        [SerializeField]
        private GameObject                        m_UI;
        private TextMeshProUGUI                   m_PlayerCountText;
        private GameObject                        m_StartGameButton;
        [SerializeField]
        private int                               m_MinimumPlayerCount; //Set it in inspector
        private float                             m_LevelSyncInterval = 2.0f;
        private bool                              m_LeavingRoom = false;
        private GameObject                        m_ScorePanelContent;
        public GameObject                         PlayerScorePrefab;
        private List<KeyValuePair<float, string>> m_Players;
        // TPS camera we use to track the player.
        private Cinemachine.CinemachineFreeLook   m_FreeLookCamera;
        private void Start()
        {
            m_Players = new List<KeyValuePair<float, string>>();
            m_ScorePanelContent = GameObject.Find("UI").transform.Find("Canvas").Find("ScorePanel").Find("Content").gameObject;
            m_FreeLookCamera = GameObject.Find("PlayerCamera").GetComponent<Cinemachine.CinemachineFreeLook>();
            m_FreeLookCamera.m_RecenterToTargetHeading.m_enabled = false;
            PhotonNetwork.CurrentRoom.IsOpen = true;
            EventManager.Get().OnToggleCursor += OnToggleCursor;
            EventManager.Get().OnUpdateScores += OnUpdateScores;
            LockCursor();
            if (m_PlayerPrefab == null)
            {

                //Debug.LogError("<Color=Red><b>Missing</b></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            }
            else
            {
                if (PlayerManager.s_LocalPlayerInstance == null) 
                {
                    Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                    PhotonNetwork.Instantiate(m_PlayerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
                    UpdateScores();
                }
                else
                {
                    //Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
                }
            }
            m_StartGameButton = m_UI.transform.Find("Canvas").Find("StartGameButton").gameObject;
            m_PlayerCountText = m_UI.transform.Find("Canvas").Find("PlayerCount").Find("PlayerCountText").GetComponent<TextMeshProUGUI>();
        }
        private void Update()
        {
            if(PhotonNetwork.IsMasterClient)
            {
                m_LevelSyncInterval -= Time.deltaTime;
                if(m_LevelSyncInterval < 0)
                {
                    m_LevelSyncInterval = 2.0f;
                    EventManager.Get().SyncObstacles();
                }
            }
            if (!m_LeavingRoom && PhotonNetwork.CurrentRoom.PlayerCount >= m_MinimumPlayerCount)
            {
                if(PhotonNetwork.IsMasterClient)
                {
                    m_StartGameButton.SetActive(true);
                }
            }
            else
            {
                m_StartGameButton.SetActive(false);
            }
            if(!m_LeavingRoom)
                m_PlayerCountText.text = PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + (" (at least 3 players to start the game)"); //TODO: Null reference when the player leaves the reoom using "Leave Room" button.
        }
        public void OnStartGameButtonPressed()
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            photonView.RPC("FadeOut", RpcTarget.All);
        }
        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting
            UpdateScores();
        }
        public override void OnPlayerLeftRoom(Player other)
        {
            //Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects
        }
        public override void OnLeftRoom()
        {
            PhotonNetwork.LoadLevel(1);
        }
        public void OnLeaveRoomButtonPressed()
        {
            m_LeavingRoom = true;
            PhotonNetwork.LeaveRoom();
        }
        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat("OnDisconnected() was called by PUN with reason {0}", cause);
            PhotonNetwork.LoadLevel(0);
        }
        [PunRPC]
        public void FadeOut()
        {
            m_UI.transform.Find("Canvas").Find("BlackScreen").GetComponent<Animator>().SetTrigger("FadeOut");
        }
        public void LockCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        public void OnToggleCursor(bool forceUnlock)
        {
            if(forceUnlock)
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
                m_FreeLookCamera.m_XAxis.m_InputAxisName = "";
                m_FreeLookCamera.m_YAxis.m_InputAxisName = "";
                m_FreeLookCamera.m_XAxis.m_InputAxisValue = 0;
                m_FreeLookCamera.m_YAxis.m_InputAxisValue = 0;
                EventManager.Get().DisableInput(SenderType.Standard);
                return;
            }
            Cursor.visible = !Cursor.visible;
            Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None : CursorLockMode.Locked;
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                m_FreeLookCamera.m_XAxis.m_InputAxisName = "Mouse X";
                m_FreeLookCamera.m_YAxis.m_InputAxisName = "Mouse Y";
                EventManager.Get().EnableInput();
                EventManager.Get().DropChatFocus();
            }
            else
            {
                m_FreeLookCamera.m_XAxis.m_InputAxisName = "";
                m_FreeLookCamera.m_YAxis.m_InputAxisName = "";
                m_FreeLookCamera.m_XAxis.m_InputAxisValue = 0;
                m_FreeLookCamera.m_YAxis.m_InputAxisValue = 0;
                EventManager.Get().DisableInput(SenderType.Standard);
            }
        }
        void UpdateScores()
        {
            foreach (Transform score in m_ScorePanelContent.transform)
                Destroy(score.gameObject);

            m_Players.Clear();
            GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
            foreach (GameObject player in players)
            {
                m_Players.Add(new KeyValuePair<float, string>(player.GetComponent<PlayerManager>().GetScore(), player.GetComponent<PlayerManager>().GetName()));
            }

            foreach (var info in m_Players)
            {
                GameObject newPlayerScore = Instantiate(PlayerScorePrefab, m_ScorePanelContent.transform);
                newPlayerScore.transform.Find("PlayerNameText").GetComponent<TextMeshProUGUI>().text = info.Value;
                newPlayerScore.transform.Find("PlayerScoreText").GetComponent<TextMeshProUGUI>().text = "0";
            }
        }
        void OnUpdateScores()
        {
            UpdateScores();
        }
    }
}