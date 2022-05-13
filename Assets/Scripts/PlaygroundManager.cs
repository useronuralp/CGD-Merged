using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Specialized;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
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
        StartedSpectating,
    }
    public enum SenderType : int
    {
        Standard,
        HitByObstacle,
    }
    public class PlaygroundManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        [SerializeField]
        private GameObject       m_PlayerPrefab;
        public static bool       m_AssignFirstBulldog;
        public static int        m_FirstBulldogID = -1;
        private GameObject       m_LocalPlayer;
        [SerializeField]         
        private float            m_TimerDuration = 180;
        private float            m_Timer;
        private TextMeshProUGUI  m_TimerText;
                                 
        public GameObject        PlayerButtonPrefab;
        public GameObject        PlayerScorePrefab;
        private TextMeshProUGUI  m_CountdownText;
        private GameObject       m_SpectatorText;
        private GameObject       m_PlayerList;
        static public bool       s_HasRoundStarted;
        private GameObject       m_ScorePanelContent;
        private TextMeshProUGUI  m_GameEndText;

        static public int        m_NumberOfInstantitatedPlayers = 0;
        static public int        m_NumberOfInitiallySetupPlayers = 0;
        private bool             m_IsSpectating = false;

        private int              m_RoundNumber = 1;

        private GameObject       m_TreetopKingdom;
        private GameObject       m_TheLongRoad;
        private int              m_ActiveLevel = 1; //1 Road - 2 Jungle.


        private List<KeyValuePair<float, string>> m_Players;
        private List<GameObject>     m_PlayerButtons;

        private bool DoOnce = true;
        private bool DoOnce2 = true;
        private bool DoOnce3 = true;
        private bool DoOnce4 = true;

        private bool m_StartCountdown = false; //Meant to be only used by the master client.
        private float m_CountdownTimer = 3;

        private float m_LevelSyncInterval = 2.0f;

        private bool m_HasGameEnded = false;

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
            m_TheLongRoad = GameObject.Find("The_Long_Road");
            m_TreetopKingdom = GameObject.Find("Treetop Kingdom");
            m_TreetopKingdom.SetActive(false);
            m_GameEndText = GameObject.Find("UI").transform.Find("Canvas").Find("EndOfGameTimer").GetComponent<TextMeshProUGUI>();
            m_ScorePanelContent = GameObject.Find("UI").transform.Find("Canvas").Find("ScorePanel").Find("Content").gameObject;
            m_Players = new List<KeyValuePair<float, string>>();
            m_PlayerButtons = new List<GameObject>();
            m_FreeLookCamera = GameObject.Find("PlayerCamera").GetComponent<Cinemachine.CinemachineFreeLook>();
            m_FreeLookCamera.m_RecenterToTargetHeading.m_enabled = true;
            EventManager.Get().OnToggleCursor += OnToggleCursor;
            EventManager.Get().OnStartingSpectating += OnStartingSpectating;
            EventManager.Get().OnStoppingSpectating += OnStoppingSpectating;
            EventManager.Get().OnUpdateScores += OnUpdateScores;
            EventManager.Get().OnTriggerEndGame += OnTriggerEndGame;
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
            m_TimerText.text = "3:00";
            if (m_PlayerPrefab == null)
            {
                Debug.LogError("<Color=Red><b>Missing</b></Color> playerPrefab Reference. Please set it up in GameObject 'Game Manager'", this);
            }
            else
            {
                if (PlayerManager.s_LocalPlayerInstance == null)
                {
                    Debug.LogFormat("We are Instantiating LocalPlayer from {0}", SceneManagerHelper.ActiveSceneName);
                    m_LocalPlayer = PhotonNetwork.Instantiate(m_PlayerPrefab.name, GameObject.Find("RunnerSpectatorZone").transform.position, Quaternion.identity, 0); //Store the local player here.
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
        public override void OnLeftRoom()
        {
            SceneManager.LoadScene(1);
        }
        private void Update()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                m_LevelSyncInterval -= Time.deltaTime;
                if (m_LevelSyncInterval < 0)
                {
                    m_LevelSyncInterval = 2.0f;
                    EventManager.Get().SyncObstacles();
                }
            }
            if (m_HasGameEnded)
            {
                if(m_Timer <= 0)
                {
                    m_Timer = 0;
                    if(DoOnce4)
                    {
                        DoOnce4 = false;
                        PhotonNetwork.LeaveRoom();
                    }
                }
                m_Timer -= Time.deltaTime;
                m_GameEndText.text = "Lobby will shut down in: " + (int)m_Timer;
            }
            if (m_IsSpectating)
            {
                if(DoOnce3)
                {
                    var content = m_PlayerList.transform.Find("Content");
                    if(m_PlayerButtons.Count > 0)
                    {
                        foreach(var button in m_PlayerButtons)
                            Destroy(button);
                    }
                    DoOnce3 = false;
                    foreach (var player in PhotonNetwork.PlayerList)
                    {
                        if (player.IsLocal || GameObject.Find(player.NickName).GetComponent<PlayerManager>().IsSpectating)
                            continue;

                        GameObject newPlayerNameButton = Instantiate(PlayerButtonPrefab, content);

                        newPlayerNameButton.transform.Find("PlayerName").GetComponent<TextMeshProUGUI>().text = player.NickName;
                        newPlayerNameButton.GetComponent<Button>().onClick.AddListener(delegate { SpectatePlayer(player.NickName); });
                        m_PlayerButtons.Add(newPlayerNameButton);
                    }
                }
                if(Input.GetKeyDown(KeyCode.M) && !m_HasGameEnded)
                {
                    var spectateCamera = GameObject.Find("CutsceneCam2");
                    var playerCamera = GameObject.Find("PlayerCamera");
                    playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().Priority = 0;
                    spectateCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>().Priority = 1;
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
                if(s_HasRoundStarted && !m_HasGameEnded)
                {
                    if (PlayerManager.s_CrossedFinishLineCount > 0 && PlayerManager.s_CrossedFinishLineCount == PlayerManager.s_RunnerCount) // Test this might not work.
                    {
                        RestartRound();
                        return;
                    }
                }
                if (m_NumberOfInstantitatedPlayers == PhotonNetwork.CurrentRoom.PlayerCount) // When all the players are instantiated and ready to be setup / placed in their respective places
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
                        photonView.RPC("UpdateScoresRPC", RpcTarget.All);
                        DoOnce2 = false;
                        Utility.RaiseEvent(true, EventType.PlacePlayers, ReceiverGroup.All, EventCaching.DoNotCache, true);
                    }
                }
            }
            if(s_HasRoundStarted && !m_HasGameEnded)
            {
                if(m_Timer > 0.0f)
                {
                    m_Timer -= Time.deltaTime;
                }
                else
                {
                    m_Timer = 0.0f;
                    s_HasRoundStarted = false;
                    if (PhotonNetwork.IsMasterClient)
                    {
                        if(!m_HasGameEnded)
                            RestartRound();
                    }
                }
                m_TimerText.text = TimeSpan.FromSeconds(m_Timer).ToString(@"mm\:ss");
            }
        }
        [PunRPC]
        void UpdateScoresRPC()
        {
            UpdateScores();
        }

        public void RestartRound() // Meant to be only called by the master client!
        {
            if (m_HasGameEnded)
                return;
            if(PhotonNetwork.IsMasterClient)
            {
                if(PlayerManager.s_CrossedFinishLineCount == 1)
                {
                    Utility.RaiseEvent(PlayerManager.s_LatestPlayerWhoCrossedTheline, EventType.RunnersWin, ReceiverGroup.All, EventCaching.DoNotCache, true);
                    PlayerManager.s_CrossedFinishLineCount = 0;
                    return;
                }
                else if(PlayerManager.s_CrossedFinishLineCount == 0)
                {

                    Utility.RaiseEvent(false, EventType.BulldogsWin, ReceiverGroup.All, EventCaching.DoNotCache, true);
                    return;
                }

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
                photonView.RPC("LockChat", RpcTarget.All);
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
            if(GameObject.Find("IndicatorCanvas").activeInHierarchy)
                GameObject.Find("IndicatorCanvas").SetActive(false);
            m_FreeLookCamera.m_RecenterToTargetHeading.m_enabled = false;
            m_CountdownText.gameObject.SetActive(true);
            m_StartCountdown = true;
        }
        public void OnEvent(ExitGames.Client.Photon.EventData photonEvent)
        {
            if (photonEvent.Code == (byte)EventType.RoundStart)
            {
                m_FreeLookCamera.m_RecenterToTargetHeading.m_enabled = false;
                s_HasRoundStarted = true;
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
                photonView.RPC("EndGame", RpcTarget.All);
            }
            else if (photonEvent.Code == (byte)EventType.RunnersWin && PhotonNetwork.IsMasterClient)
            {
                photonView.RPC("EndGame", RpcTarget.All);
            }
            else if(photonEvent.Code == (byte)EventType.ReleaseCamera)
            {
                m_FreeLookCamera.m_RecenterToTargetHeading.m_enabled = false;
            }
            else if(photonEvent.Code == (byte)EventType.StartedSpectating)
            {

            }
        }
        [PunRPC]
        void ShowRanking()
        {
            GameObject.Find("GameEndCamera").transform.GetComponent<Cinemachine.CinemachineVirtualCamera>().Priority = 5;
        }
        [PunRPC]
        void StopPlayableDirector()
        {
            GameObject.Find("Main Camera").GetComponent<PlayableDirector>().Stop();
        }
        public override void OnPlayerLeftRoom(Player other)
        {
            if(PhotonNetwork.IsMasterClient && !m_HasGameEnded)
            { 
                if(PlayerManager.s_BulldogCount == PhotonNetwork.CurrentRoom.PlayerCount)
                {
                    Utility.RaiseEvent(false, EventType.BulldogsWin, ReceiverGroup.All, EventCaching.DoNotCache, true);
                    photonView.RPC("StopPlayableDirector", RpcTarget.All);
                }
                else if(PlayerManager.s_RunnerCount == 1 && PlayerManager.s_CrossedFinishLineCount == 1)
                {
                    Utility.RaiseEvent(false, EventType.RunnersWin, ReceiverGroup.All, EventCaching.DoNotCache, true);
                    photonView.RPC("StopPlayableDirector", RpcTarget.All);
                }
                else if(PlayerManager.s_RunnerCount == PhotonNetwork.CurrentRoom.PlayerCount)
                {
                    Utility.RaiseEvent(false, EventType.RunnersWin, ReceiverGroup.All, EventCaching.DoNotCache, true);
                    photonView.RPC("StopPlayableDirector", RpcTarget.All);
                }
            }
        }
        // ---------------------------------------------------RPCs-----------------------------------------------------------------
        [PunRPC]
        public void Respawn()
        {
            m_LocalPlayer.GetComponent<PlayerManager>().Respawn();
        }
        void SwapLevels()
        {
            if(m_ActiveLevel == 1)
            {
                EventManager.Get().MoveDown_LongRoad();
                EventManager.Get().ChangeTrack(2);
                m_TreetopKingdom.SetActive(true);
                m_ActiveLevel = 2;
            }
            else
            {
                EventManager.Get().MoveDown_TreetopKingdom();
                EventManager.Get().ChangeTrack(1);
                m_TheLongRoad.SetActive(true);
                m_ActiveLevel = 1;
            }
        }
        IEnumerator TreeKingdomMoveup()
        {
            yield return new WaitForEndOfFrame();
            m_TreetopKingdom.GetComponent<TreetopKingdom>().MoveUp();
        }
        IEnumerator LongRoadMoveup()
        {
            yield return new WaitForEndOfFrame();
            m_TheLongRoad.GetComponent<TheLongRoad>().MoveUp();
        }
        [PunRPC]
        public void ResetRound()
        {
            if(m_RoundNumber > 1)
            {
                SwapLevels();
                m_RoundNumber = 1;
            }
            m_RoundNumber++;
            m_CountdownText.gameObject.SetActive(true);
            m_FreeLookCamera.m_XAxis.m_InputAxisName = "Mouse X";
            m_FreeLookCamera.m_YAxis.m_InputAxisName = "Mouse Y";
            m_Timer = m_TimerDuration;
            m_TimerText.text = "3:00";
            m_FreeLookCamera.m_RecenterToTargetHeading.m_enabled = true;
            s_HasRoundStarted = false;
            PlayerManager.s_CrossedFinishLineCount = 0;
            EventManager.Get().DropChatFocus();
        }
        [PunRPC]
        public void LockChat()
        {
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
        public void OnToggleCursor(bool forceUnlock)
        {
            if(s_HasRoundStarted)
            {
                if (forceUnlock)
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
        }
        void OnStartingSpectating()
        {
            Utility.RaiseEvent(m_LocalPlayer.GetComponent<PlayerManager>().GetName(), EventType.StartedSpectating, ReceiverGroup.All, EventCaching.DoNotCache, true);
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
        [PunRPC]
        void EndGame()
        {
            StopAllCoroutines();
            m_TimerText.gameObject.SetActive(false);
            m_GameEndText.gameObject.SetActive(true);
            m_Timer = 20;
            m_HasGameEnded = true;
            EventManager.Get().EndGame();
            m_IsSpectating = false;
            m_SpectatorText.SetActive(false);
            m_PlayerList.SetActive(false);

            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            m_FreeLookCamera.m_XAxis.m_InputAxisName = "";
            m_FreeLookCamera.m_YAxis.m_InputAxisName = "";
            m_FreeLookCamera.m_XAxis.m_InputAxisValue = 0;
            m_FreeLookCamera.m_YAxis.m_InputAxisValue = 0;
            EventManager.Get().DisableInput(SenderType.Standard);

            s_HasRoundStarted = false;

            if(PhotonNetwork.IsMasterClient)
            {
                m_Players.Clear();
                GameObject[] runners = GameObject.FindGameObjectsWithTag("Runner");
                GameObject[] bulldogs = GameObject.FindGameObjectsWithTag("Bulldog");
                foreach(GameObject runn in runners)
                {
                    m_Players.Add(new KeyValuePair<float, string>(runn.GetComponent<PlayerManager>().GetScore(), runn.GetComponent<PlayerManager>().GetName()));
                }
                foreach (GameObject bull in bulldogs)
                {
                    m_Players.Add(new KeyValuePair<float, string>(bull.GetComponent<PlayerManager>().GetScore(), bull.GetComponent<PlayerManager>().GetName()));
                }

                KeyValuePair<float, string> first = new KeyValuePair<float, string>(-1, String.Empty);
                KeyValuePair<float, string> second = new KeyValuePair<float, string>(-1, String.Empty);
                KeyValuePair<float, string> third = new KeyValuePair<float, string>(-1, String.Empty);
                GameObject stand;
                GameObject plyr;
                if(m_Players.Count > 0)
                {
                    foreach (KeyValuePair<float, string> player in m_Players)
                    {
                        if (player.Key > first.Key)
                            first = player;
                    }
                    stand = GameObject.Find("Stand1");
                    plyr = GameObject.Find(first.Value);
                    plyr.GetComponent<PlayerManager>().photonView.RPC("Stand", RpcTarget.All, "FirstPlace", new Vector3(stand.transform.position.x, stand.transform.position.y + 5, stand.transform.position.z), new Vector3(0, 180, 0));
                    m_Players.Remove(first);
                }
                if(m_Players.Count > 0)
                {
                    foreach (KeyValuePair<float, string> player in m_Players)
                    {
                        if (player.Key > second.Key)
                            second = player;
                    }
                    stand = GameObject.Find("Stand2");
                    plyr = GameObject.Find(second.Value);
                    plyr.GetComponent<PlayerManager>().photonView.RPC("Stand", RpcTarget.All, "SecondPlace", new Vector3(stand.transform.position.x, stand.transform.position.y + 5, stand.transform.position.z), new Vector3(0, 180, 0));
                    m_Players.Remove(second);
                }
                if(m_Players.Count > 0)
                {
                    foreach (KeyValuePair<float, string> player in m_Players)
                    {
                        if (player.Key > third.Key)
                            third = player;
                    }
                    stand = GameObject.Find("Stand3");
                    plyr = GameObject.Find(third.Value);
                    plyr.GetComponent<PlayerManager>().photonView.RPC("Stand", RpcTarget.All, "ThirdPlace", new Vector3(stand.transform.position.x, stand.transform.position.y + 5, stand.transform.position.z), new Vector3(0, 180, 0));
                    m_Players.Remove(third);
                }
                photonView.RPC("ShowRanking", RpcTarget.All);
            }
        }
        void UpdateScores()
        {
            foreach (Transform score in m_ScorePanelContent.transform)
                Destroy(score.gameObject);

            m_Players.Clear();
            GameObject[] runners = GameObject.FindGameObjectsWithTag("Runner");
            GameObject[] bulldogs = GameObject.FindGameObjectsWithTag("Bulldog");
            foreach (GameObject runn in runners)
            {
                m_Players.Add(new KeyValuePair<float, string>(runn.GetComponent<PlayerManager>().GetScore(), runn.GetComponent<PlayerManager>().GetName()));
            }
            foreach (GameObject bull in bulldogs)
            {
                m_Players.Add(new KeyValuePair<float, string>(bull.GetComponent<PlayerManager>().GetScore(), bull.GetComponent<PlayerManager>().GetName()));
            }

            foreach (var info in m_Players)
            {
                GameObject newPlayerScore = Instantiate(PlayerScorePrefab, m_ScorePanelContent.transform);
                newPlayerScore.transform.Find("PlayerNameText").GetComponent<TextMeshProUGUI>().text = info.Value;
                newPlayerScore.transform.Find("PlayerScoreText").GetComponent<TextMeshProUGUI>().text = info.Key.ToString();
            }
        }
        void OnTriggerEndGame()
        {
            if(PhotonNetwork.IsMasterClient)
                photonView.RPC("EndGame", RpcTarget.All);
        }
        void OnUpdateScores()
        {
            UpdateScores();
        }
    }
}
