using Photon.Pun;
using Photon.Realtime;
using System;
using TMPro;
using UnityEngine;

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
        private bool            m_HasRoundStarted;

        static public int       m_NumberOfInstantitatedPlayers = 0;
        static public int       m_NumberOfInitiallySetupPlayers = 0;

        private bool DoOnce = true;
        private bool DoOnce2 = true;

        private bool m_StartCountdown = false; //Meant to be only used by the master client.
        private float m_CountdownTimer = 3; //Meant to be only used by the master client.

        [SerializeField]
        private Vector3 m_BulldogSpawnPoint = new Vector3(0, 5, 21);
        [SerializeField]
        private Vector3 m_RunnerSpawnPoint = new Vector3(0, 5, -21);
        [SerializeField]
        private float m_SpawnSpacing = 3;

        // TPS camera we use to track the player.
        private Cinemachine.CinemachineFreeLook m_FreeLookCamera;
        private void Awake()
        {
            m_FreeLookCamera = GameObject.Find("PlayerCamera").GetComponent<Cinemachine.CinemachineFreeLook>();
            m_FreeLookCamera.m_RecenterToTargetHeading.m_enabled = true;
        }
        void Start()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;

            m_NumberOfInstantitatedPlayers = 0;
            m_NumberOfInitiallySetupPlayers = 0;

            PlayerManager.s_BulldogCount = 0;
            PlayerManager.s_RunnerCount = 0;

            m_Timer = m_TimerDuration;

            
            m_TimerText = GameObject.Find("UI").transform.Find("Canvas").Find("TimerText").GetComponent<TextMeshProUGUI>();
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
            if(Input.GetKeyDown(KeyCode.LeftControl))
            {
                Cursor.visible = !Cursor.visible;
                Cursor.lockState = Cursor.lockState == CursorLockMode.Confined ? CursorLockMode.None : CursorLockMode.Confined;
            }
            //Debug.LogError("Instantiated Players:" + m_NumberOfInstantitatedPlayers);
            // Master - Client sets up the game here.
            if (PhotonNetwork.IsMasterClient)
            {
                if(m_StartCountdown)
                {
                    m_CountdownTimer -= Time.deltaTime;
                    if(m_CountdownTimer <= 0)
                    {
                        m_StartCountdown = false;
                        m_CountdownTimer = 3;
                        StartRound();
                    }
                }
                if(Input.GetKeyDown(KeyCode.R))
                {
                    RestartRound();
                }
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
                m_StartCountdown = true;
            }
        }
        public void StartRound() //Called by the Timeline Component that can be found in the MainCamera game object in the PlaygroundScene.
        {
            if (PhotonNetwork.IsMasterClient)
            {
                Utility.RaiseEvent(true, EventType.RoundStart, ReceiverGroup.All, EventCaching.DoNotCache, true);
                photonView.RPC("ReleaseCamera", RpcTarget.All);
            }
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
            m_Timer = m_TimerDuration;
            m_TimerText.text = "2:00";
            m_FreeLookCamera.m_RecenterToTargetHeading.m_enabled = true;
            m_HasRoundStarted = false;
        }
        [PunRPC]
        public void ReleaseCamera()
        {
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
            PhotonNetwork.LoadLevel(2);
        }
    }
}
