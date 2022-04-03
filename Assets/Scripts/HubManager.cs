using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

namespace Game
{
    public class HubManager : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private GameObject      m_PlayerPrefab;
        [SerializeField]
        private GameObject      m_UI;
        private TextMeshProUGUI m_PlayerCountText;
        private GameObject      m_StartGameButton;
        [SerializeField]
        private int             m_MinimumPlayerCount; //Set it in inspector

        // TPS camera we use to track the player.
        private Cinemachine.CinemachineFreeLook m_FreeLookCamera;
        private void Awake()
        {
            m_FreeLookCamera = GameObject.Find("PlayerCamera").GetComponent<Cinemachine.CinemachineFreeLook>();
            m_FreeLookCamera.m_RecenterToTargetHeading.m_enabled = false;
        }
        private void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            if (m_PlayerPrefab == null)
            {

                //Debug.LogError("<Color=Red><b>Missing</b></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            }
            else
            {
                if (PlayerManager.s_LocalPlayerInstance == null) 
                {
                    //Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                    PhotonNetwork.Instantiate(m_PlayerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0);
                }
                else
                {
                    //Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
                }
            }
            m_StartGameButton = m_UI.transform.Find("Canvas").Find("StartGameButton").gameObject;
            m_PlayerCountText = m_UI.transform.Find("Canvas").Find("PlayerCount").GetComponent<TextMeshProUGUI>();
        }
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                Cursor.visible = !Cursor.visible;
                Cursor.lockState = Cursor.lockState == CursorLockMode.Locked ? CursorLockMode.None: CursorLockMode.Locked;
            }
            if (PhotonNetwork.CurrentRoom.PlayerCount >= m_MinimumPlayerCount)
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
            m_PlayerCountText.text = PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + (" (at least 3 players to start the game)"); //TODO: Null reference when the player leaves the reoom using "Leave Room" button.
        }
        public void OnStartGameButtonPressed()
        {
            photonView.RPC("FadeOut", RpcTarget.All);
        }
        public override void OnPlayerEnteredRoom(Player other)
        {
            Debug.LogFormat("OnPlayerEnteredRoom() {0}", other.NickName); // not seen if you're the player connecting
        }
        public override void OnPlayerLeftRoom(Player other)
        {
            //Debug.LogFormat("OnPlayerLeftRoom() {0}", other.NickName); // seen when other disconnects
        }
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(1);
        }
        public void OnLeaveRoomButtonPressed()
        {
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
    }
}