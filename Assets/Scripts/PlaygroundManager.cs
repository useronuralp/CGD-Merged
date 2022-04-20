using Photon.Pun;
using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Game
{
    static class Utility
    {
        static public void RaiseEvent(object data, EventType eventCode, ReceiverGroup recievers, EventCaching caching, bool reliability)
        {
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions()
            {
                Receivers = recievers,
                CachingOption = caching
            };

            ExitGames.Client.Photon.SendOptions sendOptions = new ExitGames.Client.Photon.SendOptions()
            {
                Reliability = reliability
            };
            PhotonNetwork.RaiseEvent((byte)eventCode, data, raiseEventOptions, sendOptions);
        }
    }
    public enum EventType : Byte
    {
        RoundStart = 1,
        RoundEnd,
        PlayerInstantiated,
        PlacePlayers,
        IncreaseInstantiatedPlayerCount,
        InitialSetupComplete,
        BulldogsWin,
        RunnersWin,
        TimeUp,
        InitPlayers,
        ReleaseCamera,
    }
    public enum SenderType : int
    {
        Standard,
        HitByObstacle,
    }
    public class PlaygroundManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        [SerializeField]
        private GameObject      m_PlayerPrefab;
        public static bool      m_AssignFirstBulldog;
        public static int       m_FirstBulldogID = -1;
        private GameObject      m_LocalPlayer;
        [SerializeField]
        private float           m_TimerDuration = 60;
        private float           m_Timer;
        private TextMeshProUGUI m_TimerText;

        public GameObject       PlayerButtonPrefab;
        private TextMeshProUGUI m_CountdownText;
        private GameObject      m_SpectatorText;
        private GameObject      m_PlayerList;
        private bool            m_HasRoundStarted;

        static public int       m_NumberOfInstantitatedPlayers = 0;
        static public int       m_NumberOfInitiallySetupPlayers = 0;
        private bool            m_IsSpectating = false;

        private bool DoOnce = true;
        private bool DoOnce2 = true;
        private bool DoOnce3 = true;

        private bool m_StartCountdown = false; //Meant to be only used by the master client.
        private float m_CountdownTimer = 3;

        private float m_LevelSyncInterval = 2.0f;

        [SerializeField]
        private Vector3 m_BulldogSpawnPoint = new Vector3(0, 5, 21);
        [SerializeField]
        private Vector3 m_RunnerSpawnPoint = new Vector3(0, 5, -21);
        [SerializeField]
        private float m_SpawnSpacing = 3;

        // TPS camera we use to track the player.
        private Cinemachine.CinemachineFreeLook m_FreeLookCamera;
        void Start()
        {
            m_FreeLookCamera = GameObject.Find("PlayerCamera").GetComponent<Cinemachine.CinemachineFreeLook>();
            m_FreeLookCamera.m_RecenterToTargetHeading.m_enabled = true;
            EventManager.Get().OnToggleCursor += OnToggleCursor;
            EventManager.Get().OnStartingSpectating += OnStartingSpectating;
            EventManager.Get().OnStoppingSpectating += OnStoppingSpectating;
            LockCursor();
            m_NumberOfInstantitatedPlayers = 0;
            m_NumberOfInitiallySetupPlayers = 0;

            PlayerManager.s_BulldogCount = 0;
            PlayerManager.s_RunnerCount = 0;

            m_Timer = m_TimerDuration;

            
            m_TimerText = GameObject.Find("UI").transform.Find("Canvas").Find("TimerText").GetComponent<TextMeshProUGUI>();
            m_CountdownText = GameObject.Find("UI").transform.Find("Canvas").Find("Countdown").GetComponent<TextMeshProUGUI>();
            m_SpectatorText = GameObject.Find("UI").transform.Find("Canvas").Find("SpectatorText").gameObject;
            m_PlayerList = GameObject.Find("UI").transform.Find("Canvas").Find("PlayerList").gameObject;
            m_CountdownText.text = "3";
            m_CountdownText.gameObject.SetActive(false);
            m_TimerText.text = "2:00";
            if (m_PlayerPrefab == null)
            {
                Debug.LogError("<Color=Red><b>Missing</b></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            }
            else
            {
                if (PlayerManager.s_LocalPlayerInstance == null)
                {
                    Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                    m_LocalPlayer = PhotonNetwork.Instantiate(m_PlayerPrefab.name, new Vector3(0f, 5f, 0f), Quaternion.identity, 0); //Store the local player here.
                }
                else
                {
                    Debug.LogFormat("Ignoring scene load for {0}", SceneManagerHelper.ActiveSceneName);
                }
            }
            if(!PhotonNetwork.IsMasterClient)
            {
                GameObject.Find("UI").transform.Find("Canvas").transform.Find("ReturnToHub").gameObject.SetActive(false);
            }
        }
        private void Update()
        {
            if(m_IsSpectating)
            {
                
                if(DoOnce3)
                {
                    var content = m_PlayerList.transform.Find("Content");
                    DoOnce3 = false;
                    foreach (var player in PhotonNetwork.PlayerList)
                    {
                        if (player.IsLocal)
                            continue;
                        GameObject newRoomButton = Instantiate(PlayerButtonPrefab, content);

                        newRoomButton.transform.Find("PlayerName").GetComponent<TextMeshProUGUI>().text = player.NickName;
                        newRoomButton.GetComponent<Button>().onClick.AddListener(delegate { SpectatePlayer(player.NickName); });
                    }
                }
                if(Input.GetKeyDown(KeyCode.M))
                {
                    var spectateCamera = GameObject.Find("CutsceneCam2");
                    var playerCamera = GameObject.Find("PlayerCamera");
                    playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().Priority = 0;
                    spectateCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>().Priority = 1;
                }
            }
            if (PhotonNetwork.IsMasterClient)
            {
                m_LevelSyncInterval -= Time.deltaTime;
                if (m_LevelSyncInterval < 0)
                {
                    m_LevelSyncInterval = 2.0f;
                    EventManager.Get().SyncObstacles();
                }
            }
            // Master - Client sets up the game here.
            if (m_StartCountdown)
            {
                m_CountdownTimer -= Time.deltaTime;
                m_CountdownText.text = ((int)m_CountdownTimer + 1).ToString();
                if (m_CountdownTimer <= 0)
                {
                    m_StartCountdown = false;
                    m_CountdownTimer = 3;
                    if (PhotonNetwork.IsMasterClient)
                        StartRound();
                }
            }
            if (PhotonNetwork.IsMasterClient)
            {
                //if(Input.GetKeyDown(KeyCode.R))
                //{
                //    RestartRound();
                //}
                if(m_NumberOfInstantitatedPlayers == PhotonNetwork.CurrentRoom.PlayerCount) // When all the players are instantiated and ready to be setup / placed in their respective places
                {
                    if(DoOnce)
                    {
                        DoOnce = false;
                        // Pick the first bulldog here and make the rest of the players runners
                        int firstBulldogID = UnityEngine.Random.Range(1, PhotonNetwork.CurrentRoom.PlayerCount + 1);
                        Utility.RaiseEvent(new object[] { m_BulldogSpawnPoint, m_RunnerSpawnPoint, m_SpawnSpacing }, EventType.InitPlayers, ReceiverGroup.All, EventCaching.DoNotCache, true);
                        photonView.RPC("InitialSetup", RpcTarget.All, firstBulldogID);
                    }
                }
                if(m_NumberOfInitiallySetupPlayers == PhotonNetwork.CurrentRoom.PlayerCount) // When all the players are setup and ready to start the game (first round)
                {
                    if(DoOnce2)
                    {
                        DoOnce2 = false;
                        Utility.RaiseEvent(true, EventType.PlacePlayers, ReceiverGroup.All, EventCaching.DoNotCache, true);
                    }
                }
            }
            if(m_HasRoundStarted)
            {
                if(m_Timer > 0.0f)
                {
                    m_Timer -= Time.deltaTime;
                }
                else
                {
                    m_Timer = 0.0f;
                    m_HasRoundStarted = false;
                    if(PhotonNetwork.IsMasterClient)
                    {
                        RestartRound();
                    }
                }
                m_TimerText.text = TimeSpan.FromSeconds(m_Timer).ToString(@"mm\:ss");
            }
        }
        public void RestartRound() // Meant to be only called by the master client!
        {
            if(PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("ResetRound", RpcTarget.All);
                Utility.RaiseEvent(true, EventType.RoundEnd, ReceiverGroup.All, EventCaching.DoNotCache, true);
                m_LocalPlayer.GetComponent<PlayerManager>().ResetSpawnNumbering();
                Utility.RaiseEvent(new object[] {m_BulldogSpawnPoint, m_RunnerSpawnPoint, m_SpawnSpacing}, EventType.InitPlayers, ReceiverGroup.All, EventCaching.DoNotCache, true);
                Utility.RaiseEvent(false, EventType.PlacePlayers, ReceiverGroup.All, EventCaching.DoNotCache, true);
                photonView.RPC("StartCountdown", RpcTarget.All);
            }
        }
        public void StartRound() // Called whenever a rounds ends and the next one is intented to be started.
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Utility.RaiseEvent(true, EventType.RoundStart, ReceiverGroup.All, EventCaching.DoNotCache, true);
                photonView.RPC("ReleaseCamera", RpcTarget.All);
            }
        }
        public void StartRoundFirst() //Called by the Timeline Component that can be found in the MainCamera game object in the PlaygroundScene.
        {
            if (PhotonNetwork.IsMasterClient)
                photonView.RPC("StartCountdown", RpcTarget.All);
        }
        [PunRPC]
        public void StartCountdown()
        {
            m_FreeLookCamera.m_RecenterToTargetHeading.m_enabled = false;
            m_CountdownText.gameObject.SetActive(true);
            m_StartCountdown = true;
        }
        public void OnEvent(ExitGames.Client.Photon.EventData photonEvent)
        {
            if (photonEvent.Code == (byte)EventType.RoundStart)
            {
                m_FreeLookCamera.m_RecenterToTargetHeading.m_enabled = false;
                m_HasRoundStarted = true;
            }
            else if (photonEvent.Code == (byte)EventType.IncreaseInstantiatedPlayerCount && PhotonNetwork.IsMasterClient)
            {
                m_NumberOfInstantitatedPlayers++;
                photonView.RPC("IncreaseInstantiatedPlayerCount", RpcTarget.All, m_NumberOfInstantitatedPlayers);
            }
            else if(photonEvent.Code == (byte)EventType.InitialSetupComplete && PhotonNetwork.IsMasterClient)
            {
                m_NumberOfInitiallySetupPlayers++;
                photonView.RPC("IncreaseInitiallySetupPlayerCount", RpcTarget.All, m_NumberOfInitiallySetupPlayers);
            }
            else if(photonEvent.Code == (byte)EventType.BulldogsWin && PhotonNetwork.IsMasterClient)
            {
                if((bool)photonEvent.CustomData)
                    Debug.LogError("BULLDOGS WIN by END OF ROUND");
                else
                    Debug.LogError("BULLDOGS WIN by COLLISION");
            }
            else if(photonEvent.Code == (byte)EventType.ReleaseCamera)
            {
                m_FreeLookCamera.m_RecenterToTargetHeading.m_enabled = false;
            }
        }
        // ---------------------------------------------------RPCs-----------------------------------------------------------------
        [PunRPC]
        public void Respawn()
        {
            m_LocalPlayer.GetComponent<PlayerManager>().Respawn();
        }
        [PunRPC]
        public void ResetRound()
        {
            m_CountdownText.gameObject.SetActive(true);
            m_FreeLookCamera.m_XAxis.m_InputAxisName = "Mouse X";
            m_FreeLookCamera.m_YAxis.m_InputAxisName = "Mouse Y";
            m_Timer = m_TimerDuration;
            m_TimerText.text = "2:00";
            m_FreeLookCamera.m_RecenterToTargetHeading.m_enabled = true;
            m_HasRoundStarted = false;
            EventManager.Get().DropChatFocus();
        }
        [PunRPC]
        public void ReleaseCamera()
        {
            m_CountdownText.gameObject.SetActive(false);
            m_FreeLookCamera.m_RecenterToTargetHeading.m_enabled = false;
        }
        [PunRPC]
        public void InitialSetup(int bulldogID) // Every PlaygroundManager should have a localPlayer instance when they're initialized. 
        {
            m_LocalPlayer.GetComponent<PlayerManager>().InitialSetup(bulldogID);
        }
        [PunRPC]
        public void IncreaseInitiallySetupPlayerCount(int newCount)
        {
            m_NumberOfInitiallySetupPlayers = newCount;
        }
        [PunRPC]
        public void IncreaseInstantiatedPlayerCount(int newCount)
        {
            m_NumberOfInstantitatedPlayers = newCount;
        }
        public void OnPressedReturnToHubButton()
        {
            if(PhotonNetwork.IsMasterClient)
                PhotonNetwork.LoadLevel(2);
        }
        public void LockCursor()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        public void OnToggleCursor()
        {
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
        void OnStartingSpectating()
        {
            m_IsSpectating = true;
            m_SpectatorText.SetActive(true);
            m_PlayerList.SetActive(true);
            DoOnce3 = true;
        }
        void OnStoppingSpectating()
        {
            m_IsSpectating = false;
            m_SpectatorText.SetActive(false);
            m_PlayerList.SetActive(false);
            m_FreeLookCamera.LookAt = m_LocalPlayer.transform;
            m_FreeLookCamera.Follow = m_LocalPlayer.transform;
        }
        void SpectatePlayer(string playerToSpectate)
        {
            GameObject.Find("Main Camera").GetComponent<Cinemachine.CinemachineBrain>().m_DefaultBlend.m_Time = 0;
            var spectateCamera = GameObject.Find("CutsceneCam2");
            var playerCamera = GameObject.Find("PlayerCamera");
            if (playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().Priority < 1)
            {
                playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().Priority = 1;
                spectateCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>().Priority = 0;
            }
            var player = GameObject.Find(playerToSpectate);
            m_FreeLookCamera.LookAt = player.transform;
            m_FreeLookCamera.Follow = player.transform;
        }
    }
}
