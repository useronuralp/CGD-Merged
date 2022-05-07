using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;
namespace Game
{
    public class PlayerManager : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        public static GameObject s_LocalPlayerInstance; //The static variables are initialized to defualt values whenever a new player joins the game. Therefore, each time a new player joins, they will set the s_localPlayerInstance to "null".
        private Rigidbody m_Rigidbody;

        public bool m_IsBulldog { get; set; } = false;

        private Vector3              m_BulldogSpawnPoint;
        private Vector3              m_RunnerSpawnPoint;

        private Dictionary<int, int> m_EvenSpawnPoints;
        private Dictionary<int, int> m_OddSpawnPoints;

        static public int           s_BulldogCount = 0;
        static public int           s_RunnerCount = 0;
        static public int           s_CrossedFinishLineCount = 0;

        private float               m_SpawnSpacing;

        private bool                m_HasRoundStarted = false;

        private bool                m_HasCrossedTheFinishLine = false;

        public bool                 IsSpectating = false;

        private Material            m_JammoEyesMaterial;

        private TextMeshProUGUI     m_PowerupName;

        static public string        s_LatestPlayerWhoCrossedTheline = "None";
        private float               m_Score = 0;

        private string              m_PlayerName;

        private Animator            m_Animator;

        private bool                m_IsStunned = false;

        private float               m_StunTimer;
        private float               m_StunCD = 5;


        private bool                m_HasForcefield = false;

        //Powerups----------------------
        private bool                m_HasDoubleJump = false;

        private Dictionary<string, Vector2> m_EyeTypes;

        public enum PowerupType
        {
            DoubleJump,
            WaterBaloon,
            Forcefield,
        }
        public void Respawn()
        {
            PlacePlayer(false);
        }
        private void Start()
        {
            m_StunTimer = m_StunCD;
            m_PowerupName = transform.Find("PowerupCanvas").Find("PowerupSlot").Find("PowerupName").GetComponent<TextMeshProUGUI>();
            if (photonView.IsMine)
                transform.Find("PowerupCanvas").Find("PowerupSlot").gameObject.SetActive(true);
            m_EyeTypes = new Dictionary<string, Vector2>();
            m_EyeTypes.Add("Default", new Vector2(0.0f, 0.0f));
            m_EyeTypes.Add("Happy", new Vector2(0.33f, 0.0f));
            m_EyeTypes.Add("Angry", new Vector2(0.66f, 0.0f));
            m_EyeTypes.Add("Dead", new Vector2(0.0f, 0.66f));
            m_EyeTypes.Add("Sad", new Vector2(0.33f, 0.66f));
            m_JammoEyesMaterial = transform.Find("head_eyes_low").GetComponent<Renderer>().material;
            m_Animator = GetComponent<Animator>();
            if (SceneManagerHelper.ActiveSceneBuildIndex == 2)
                photonView.RPC("SetOutlineColor", RpcTarget.All, new object[] {0.0f, 0.0f, 0.0f, 0.0f});
            if (photonView.IsMine)
                photonView.RPC("SetNickname", RpcTarget.AllBuffered, PhotonNetwork.NickName);
            EventManager.Get().UpdateScores();
            EventManager.Get().OnChangeEyes += OnChangeEyes;
            m_EvenSpawnPoints = new Dictionary<int, int>() { {0,1}, {2,2}, {4,3}, {6,4}, {8,5}, {10,6}, {12,7}, {14,8}, {16,9}, {18,10} };
            m_OddSpawnPoints  = new Dictionary<int, int>() { {1,1}, {3,2}, {5,3}, {7,4}, {9,5}, {11,6}, {13,7}, {15,8}, {17,9}, {19,10} };
            m_Rigidbody = GetComponent<Rigidbody>();
            if (photonView.IsMine)
            {
                s_LocalPlayerInstance = gameObject;
                photonView.RPC("ChangePrefabName_RPC", RpcTarget.AllBuffered, PhotonNetwork.NickName);
                GetComponent<Camera>().StartFollowing();
            }
            if(PhotonNetwork.IsMasterClient)
                Utility.RaiseEvent(false, EventType.IncreaseInstantiatedPlayerCount, ReceiverGroup.All, EventCaching.DoNotCache, true); // PlaygroundManager catches this.
            
        }
        private void OnDestroy()
        {
            EventManager.Get().OnChangeEyes -= OnChangeEyes;
            if(m_HasCrossedTheFinishLine)
            {
                s_CrossedFinishLineCount--;
            }
        }
        private void Update()
        {
            //if(Input.GetKeyDown(KeyCode.J) && photonView.IsMine)
            //{
            //    GetStunned();
            //}
            if(m_IsStunned)
            {
                if(m_StunTimer <= 0.0f)
                {
                    m_IsStunned = false;
                    EventManager.Get().StunWearsOff();
                    photonView.RPC("DisableStunVFX", RpcTarget.All);
                    m_StunTimer = m_StunCD;
                    return;
                }
                m_StunTimer -= Time.deltaTime;
            }
        }
        [PunRPC]
        void EnableStunVFX()
        {
            transform.Find("StunVFX").gameObject.SetActive(true);
        }
        [PunRPC]
        void DisableStunVFX()
        {
            transform.Find("StunVFX").gameObject.SetActive(false);
        }
        [PunRPC]
        private void SetOutlineColor(object[] data)
        {
            GetComponent<Outline>().OutlineColor = new Color((float)data[0], (float)data[1], (float)data[2], (float)data[3]);
        }
        [PunRPC]
        private void SetNickname(string name)
        {
            m_PlayerName = name;
            transform.Find("NameCanvas").Find("PlayerName").GetComponent<TextMeshProUGUI>().text = name + " (0)";
        }
        public void InitialSetup(int bulldogID)
        {
            if (PhotonNetwork.LocalPlayer.ActorNumber == bulldogID)
                photonView.RPC("BecomeFirstBulldog", RpcTarget.All);
            else
                photonView.RPC("BecomeFirstRunner", RpcTarget.All);
        }
        private void SetBulldogParams()
        {
            m_IsBulldog = true;
            transform.tag = "Bulldog";
            transform.Find("NameCanvas").Find("PlayerName").GetComponent<TextMeshProUGUI>().color = Color.red;
            GetComponent<Outline>().OutlineColor = Color.red;
            m_JammoEyesMaterial.color = Color.red;
            m_JammoEyesMaterial.SetColor("_EmissionColor", (Color.red) * 800.0f);
            m_JammoEyesMaterial.SetTextureOffset("_MainTex", m_EyeTypes["Angry"]);
        }
        private void SetRunnerParams()
        {
            Color clr = new Color32(54, 147, 169, 255);
            m_IsBulldog = false;
            transform.tag = "Runner";
            transform.Find("NameCanvas").Find("PlayerName").GetComponent<TextMeshProUGUI>().color = clr;
            GetComponent<Outline>().OutlineColor = clr;
            m_JammoEyesMaterial.color = clr;
            m_JammoEyesMaterial.SetColor("_EmissionColor", (clr) * 1200.0f);
            m_JammoEyesMaterial.SetTextureOffset("_MainTex", m_EyeTypes["Default"]);

        }
        [PunRPC]
        public void BecomeFirstBulldog()
        {
            
            SetBulldogParams();
            if (PhotonNetwork.IsMasterClient)
                Utility.RaiseEvent(false, EventType.InitialSetupComplete, ReceiverGroup.All, EventCaching.DoNotCache, true); // PlaygroundManager catches this.
        }
        [PunRPC]
        public void BecomeFirstRunner() 
        {
            SetRunnerParams();
            if (PhotonNetwork.IsMasterClient)
                Utility.RaiseEvent(false, EventType.InitialSetupComplete, ReceiverGroup.All, EventCaching.DoNotCache, true); //Should only be handled by master client
        }
        [PunRPC]
        public void BecomeBulldogByEndOfRound()
        {
            SetBulldogParams();
        }
        [PunRPC]
        public void BecomeBulldogByCollision()
        {
            if(PhotonNetwork.IsMasterClient)
            {
                s_BulldogCount++;
                s_RunnerCount--;
                if(s_RunnerCount == 0) // Bulldogs win the game if they catch all the players while the round is still being played
                    Utility.RaiseEvent(false, EventType.BulldogsWin, ReceiverGroup.All, EventCaching.DoNotCache, true); // PlaygroundManager catches this.
                photonView.RPC("SyncBulldogAndRunnerCounts", RpcTarget.All, new int[] { s_BulldogCount, s_RunnerCount });
            }
            SetBulldogParams();
        }
        [PunRPC]
        public void Stand(string ranking, Vector3 position, Vector3 rotation)
        {
            m_Rigidbody.useGravity = true;
            m_Rigidbody.velocity = new Vector3(0, 0, 0);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            transform.SetPositionAndRotation(position, Quaternion.Euler(rotation));
            m_Rigidbody.constraints = RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
            m_Animator.SetTrigger(ranking);
        }
        [PunRPC]
        public void PlacePlayer(bool isFirstRound)
        {
            if (m_IsBulldog)
            {
                if(s_BulldogCount % 2 == 0)
                    transform.position = new Vector3(m_BulldogSpawnPoint.x + m_EvenSpawnPoints[s_BulldogCount] * m_SpawnSpacing, m_BulldogSpawnPoint.y, m_BulldogSpawnPoint.z);
                else
                    transform.position = new Vector3(m_BulldogSpawnPoint.x - m_OddSpawnPoints[s_BulldogCount] * m_SpawnSpacing, m_BulldogSpawnPoint.y, m_BulldogSpawnPoint.z);

                s_BulldogCount++;
            }
            else
            {
                if (s_RunnerCount % 2 == 0)
                    transform.position = new Vector3(m_RunnerSpawnPoint.x + m_EvenSpawnPoints[s_RunnerCount] * m_SpawnSpacing, m_RunnerSpawnPoint.y, m_RunnerSpawnPoint.z);
                else
                    transform.position = new Vector3(m_RunnerSpawnPoint.x - m_OddSpawnPoints[s_RunnerCount] * m_SpawnSpacing, m_RunnerSpawnPoint.y, m_RunnerSpawnPoint.z);

                s_RunnerCount++;
            }
            if (m_IsBulldog)
                transform.eulerAngles = new Vector3(0, 180, 0);
            else
                transform.eulerAngles = new Vector3(0, 0, 0);

            m_Rigidbody.velocity = Vector3.zero;
            m_Rigidbody.angularVelocity = Vector3.zero;

            m_HasCrossedTheFinishLine = false;
            if (!isFirstRound)
                StartCoroutine(ReleaseCameraDelayed(0.2f));
        }
        [PunRPC]
        public void SyncBulldogAndRunnerCounts(int[] countArr)
        {
            s_BulldogCount = countArr[0];
            s_RunnerCount = countArr[1];
        }
        [PunRPC]
        public void CrossFinishLine(string nickname) // Use only on runners, Bulldogs can't cross the finish line
        {
            if(photonView.IsMine)
                StartCoroutine(SpawnSmoke());

            m_HasCrossedTheFinishLine = true;
            s_LatestPlayerWhoCrossedTheline = nickname; 
            s_CrossedFinishLineCount++;
            if(photonView.IsMine)
            {
                EventManager.Get().StopAllCoroutines_InControls();
            }
        }
        [PunRPC]
        public void Init(Vector3 bulldogSpawnPoint, Vector3 runnerSpawnPoint, float spawnSpacing)
        {
            m_BulldogSpawnPoint = bulldogSpawnPoint;
            m_RunnerSpawnPoint = runnerSpawnPoint;  
            m_SpawnSpacing = spawnSpacing;
        }
        [PunRPC]
        void SyncSpectatingStatus(bool status)
        {
            IsSpectating = status;
        }
        public void OnEvent(ExitGames.Client.Photon.EventData photonEvent)
        {
            if (photonEvent.Code == (byte)EventType.PlacePlayers)
            {
                if((bool)photonEvent.CustomData) //if first round
                {
                    if (PhotonNetwork.IsMasterClient)
                    {
                        photonView.RPC("PlacePlayer", RpcTarget.All, true);
                    }
                }
                else
                {
                    // Pre-placement setup to ensure necessary settings are set
                    var spectateCamera = GameObject.Find("CutsceneCam2");
                    var playerCamera = GameObject.Find("PlayerCamera");
                    playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().LookAt = transform;
                    playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().Follow = transform;
                    playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().Priority = 1;
                    spectateCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>().Priority = 0;
                    Cursor.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    IsSpectating = false;
                    if(photonView.IsMine)
                        photonView.RPC("SyncSpectatingStatus", RpcTarget.All, IsSpectating);
                    EventManager.Get().StopSpectating();

                    if (PhotonNetwork.IsMasterClient && !m_HasCrossedTheFinishLine && !m_IsBulldog) // If a runner hasn't crossed the line when the round ends, they turn into bulldogs.
                        photonView.RPC("BecomeBulldogByEndOfRound", RpcTarget.All);
                    if (PhotonNetwork.IsMasterClient)
                        photonView.RPC("PlacePlayer", RpcTarget.All, false);
                }
            }
            else if(photonEvent.Code == (byte)EventType.RoundStart)
            {
                m_HasRoundStarted = true;
            }
            else if(photonEvent.Code == (byte)EventType.RoundEnd)
            {
                m_HasRoundStarted = false;
            }
            else if(photonEvent.Code == (byte)EventType.InitPlayers && PhotonNetwork.IsMasterClient)
            {
                object[] data = (object[])photonEvent.CustomData;
                photonView.RPC("Init", RpcTarget.All, (Vector3)data[0], (Vector3)data[1], (float)data[2]);
            }
        }
        public void ResetSpawnNumbering()
        {
            if(PhotonNetwork.IsMasterClient)
            {
                s_BulldogCount = 0;
                s_RunnerCount = 0;
                photonView.RPC("SyncBulldogAndRunnerCounts", RpcTarget.All, new int[] { s_BulldogCount, s_RunnerCount});
            }
        }
        private void OnCollisionEnter(Collision collision)
        {
            if(m_HasRoundStarted && photonView.IsMine)
            {
                if (collision.transform.CompareTag("Bulldog") && !m_IsBulldog)
                {
                
                    Debug.LogError("Collided with bulldog");
                    BecomeBulldogByCollisionHelper();
                    collision.transform.GetComponent<PlayerManager>().photonView.RPC("IncreaseScore", RpcTarget.All, 5.0f);
                }
            }
        }
        [PunRPC]
        public void IncreaseScore(float amount)
        {
            m_Score += amount;
            transform.Find("NameCanvas").Find("PlayerName").GetComponent<TextMeshProUGUI>().text = name + " (" + m_Score.ToString() + ")";
            EventManager.Get().UpdateScores();
        }
        private void OnTriggerEnter(Collider other)
        {
            if(photonView.IsMine)
            {
                if (other.transform.CompareTag("FinishLine") && !m_IsBulldog && !m_HasCrossedTheFinishLine)
                {
                    m_HasCrossedTheFinishLine = true;
                    EventManager.Get().DisableInput(SenderType.Standard);
                    photonView.RPC("CrossFinishLine", RpcTarget.All, PhotonNetwork.NickName);
                    photonView.RPC("IncreaseScore", RpcTarget.All, 10.0f);
                    if(s_CrossedFinishLineCount < s_RunnerCount) // Current crosser not being the last person to cross the line
                        BecomeSpectatorByCrossingFinishLine();
                    else if(s_CrossedFinishLineCount == s_RunnerCount) // Current crosser being the last person to cross the line
                    {
                        var playerCamera = GameObject.Find("PlayerCamera");
                        playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().LookAt = null;
                        playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().Follow = null;
                        if(photonView.IsMine)
                            EventManager.Get().DisableInput(SenderType.Standard);
                        SpectatorSpawn();
                        Cursor.visible = false;
                        Cursor.lockState = CursorLockMode.Locked;
                    }

                }
                else if(other.CompareTag("DeathBox"))
                {
                    if(SceneManagerHelper.ActiveSceneBuildIndex == 2)
                    {
                        // Spawn out of bounds player in the middle of the map. This part is not too important.
                        transform.position = new Vector3(0, 5, 0);
                        m_Rigidbody.velocity = new Vector3(0, 0, 0);
                    }
                    else if(SceneManagerHelper.ActiveSceneBuildIndex == 3)
                    {
                        if(m_IsBulldog)
                        {
                            // TODO: Spawn at the location where this character was 5 seconds ago.
                            transform.position = new Vector3(0, 5, 0);
                            m_Rigidbody.velocity = new Vector3(0, 0, 0);
                        }
                        else
                        {
                            BecomeBulldogByCollisionHelper();
                            m_Rigidbody.useGravity = false;
                        }
                    }
                }
            }
            if (PhotonNetwork.IsMasterClient)
            {
                if(other.CompareTag("Powerup_DoubleJump"))
                {
                    photonView.RPC("Powerup", RpcTarget.All, PowerupType.DoubleJump, m_PlayerName);
                    StartCoroutine(SpawnPowerupParticles(other.transform.position, other.transform.rotation));
                    StartCoroutine(SpawnSoundSource(other.transform.position, other.transform.rotation));
                    PhotonNetwork.Destroy(other.gameObject);
                }
                else if(other.CompareTag("Powerup_WaterBaloon") && (!m_IsBulldog || CompareTag("Player")))
                {
                    photonView.RPC("Powerup", RpcTarget.All, PowerupType.WaterBaloon, m_PlayerName);
                    StartCoroutine(SpawnPowerupParticles(other.transform.position, other.transform.rotation));
                    StartCoroutine(SpawnSoundSource(other.transform.position, other.transform.rotation));
                    PhotonNetwork.Destroy(other.gameObject);
                }
                else if (other.CompareTag("Powerup_Forcefield") && (!m_IsBulldog || CompareTag("Player")))
                {
                    photonView.RPC("Powerup", RpcTarget.All, PowerupType.Forcefield, m_PlayerName);
                    StartCoroutine(SpawnPowerupParticles(other.transform.position, other.transform.rotation));
                    StartCoroutine(SpawnSoundSource(other.transform.position, other.transform.rotation));
                    PhotonNetwork.Destroy(other.gameObject);
                }
            }
        }
        [PunRPC]
        void GetStunned_RPC()
        {
            if(photonView.IsMine)
            {
                EventManager.Get().GetStunned();
                m_IsStunned = true;
                photonView.RPC("EnableStunVFX", RpcTarget.All);
            }
        }
        public void GetStunned()
        {
            photonView.RPC("GetStunned_RPC", RpcTarget.All);
        }
        IEnumerator SpawnPowerupParticles(Vector3 spawnLocation, Quaternion spawnRotation)
        {
            var particles = PhotonNetwork.Instantiate("PowerupParticles", spawnLocation, spawnRotation);
            yield return new WaitForSeconds(particles.GetComponent<ParticleSystem>().main.startLifetimeMultiplier);
            PhotonNetwork.Destroy(particles);
        }
        IEnumerator SpawnSoundSource(Vector3 spawnLocation, Quaternion spawnRotation)
        {
            var soundSource = PhotonNetwork.Instantiate("SoundSource", spawnLocation, spawnRotation);
            AudioClip clip = Resources.Load<AudioClip>("Audio/Powerup/PowerupCollect");
            soundSource.GetComponent<AudioSource>().PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
            PhotonNetwork.Destroy(soundSource);
        }
        [PunRPC]
        public void Powerup(PowerupType type, string name)
        {
            switch(type)
            {
                case PowerupType.DoubleJump:
                    Debug.LogError("Player " + name + " has double jump");
                    if(photonView.IsMine)
                    {
                        DoubleJump();
                    }
                    break;
                case PowerupType.WaterBaloon:
                    Debug.LogError("Player " + name + " has water baloon");
                    if (photonView.IsMine)
                    {
                        WaterBaloon();
                    }
                    break;
                case PowerupType.Forcefield:
                    Debug.LogError("Player " + name + " has forcefield");
                    if (photonView.IsMine)
                    {
                        Forcefield();
                    }
                    break;
            }
        }
        void BecomeBulldogByCollisionHelper()
        {
            StartCoroutine(SpawnSmoke());
            IsSpectating = true;
            if (photonView.IsMine)
                photonView.RPC("SyncSpectatingStatus", RpcTarget.All, IsSpectating);
            photonView.RPC("BecomeBulldogByCollision", RpcTarget.All);
            if (s_CrossedFinishLineCount == s_RunnerCount)
            {
                var playerCamera = GameObject.Find("PlayerCamera");
                playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().LookAt = null;
                playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().Follow = null;
                if (photonView.IsMine)
                    EventManager.Get().DisableInput(SenderType.Standard);
                SpectatorSpawn();
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                // Blend from player camera to the spectator bird-eye view camera.
                var spectateCamera = GameObject.Find("CutsceneCam2"); 
                var playerCamera = GameObject.Find("PlayerCamera");
                GameObject.Find("Main Camera").GetComponent<Cinemachine.CinemachineBrain>().m_DefaultBlend.m_Time = 0.5f;
                playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().LookAt = null;
                playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().Follow = null;
                playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().Priority = 0;
                spectateCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>().Priority = 1;
                // Call events 
                EventManager.Get().StartSpectating();
                EventManager.Get().DisableInput(SenderType.Standard);
                SpectatorSpawn();
            }
        }
        void BecomeSpectatorByCrossingFinishLine()
        {
            IsSpectating = true;
            if (photonView.IsMine)
                photonView.RPC("SyncSpectatingStatus", RpcTarget.All, IsSpectating);
            var spectateCamera = GameObject.Find("CutsceneCam2");
            var playerCamera = GameObject.Find("PlayerCamera");
            GameObject.Find("Main Camera").GetComponent<Cinemachine.CinemachineBrain>().m_DefaultBlend.m_Time = 0.5f;
            playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().LookAt = null;
            playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().Follow = null;
            playerCamera.GetComponent<Cinemachine.CinemachineFreeLook>().Priority = 0;
            spectateCamera.GetComponent<Cinemachine.CinemachineVirtualCamera>().Priority = 1;
            // Call events 
            EventManager.Get().StartSpectating();
            EventManager.Get().DisableInput(SenderType.Standard);
            SpectatorSpawn();
        }

        IEnumerator SpawnSmoke()
        {
            // Destroy the particle system after its lifetime duration passes.
            var sys = PhotonNetwork.Instantiate("DisappearSmoke", new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), transform.rotation);
            yield return new WaitForSeconds(sys.GetComponent<ParticleSystem>().main.startLifetimeMultiplier);
            PhotonNetwork.Destroy(sys);
        }
        void SpectatorSpawn() // Takes the character of the player who was just turned into a bulldog and puts it into a location away from the play area.
        {
            m_Rigidbody.useGravity = true;
            Vector3 spectatorZonePos;
            if (m_IsBulldog)
                spectatorZonePos = GameObject.Find("BulldogSpectatorZone").transform.position;
            else
                spectatorZonePos = GameObject.Find("RunnerSpectatorZone").transform.position;
            transform.position = new Vector3(spectatorZonePos.x, spectatorZonePos.y + 10, spectatorZonePos.z);
            m_Rigidbody.velocity = new Vector3(0, 0, 0);
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        IEnumerator ReleaseCameraDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            Utility.RaiseEvent(true, EventType.ReleaseCamera, ReceiverGroup.All, EventCaching.DoNotCache, true);
        }
        [PunRPC]
        void ChangePrefabName_RPC(string name)
        { 
            gameObject.name = name;
        }
        public float GetScore()
        {
            return m_Score;
        }
        public string GetName()
        {
            return m_PlayerName;
        }
        [PunRPC]
        void ChangeEyes(string type)
        {
            m_JammoEyesMaterial.SetTextureOffset("_MainTex", m_EyeTypes[type]);
        }
        void OnChangeEyes(string type)
        {
            if(photonView.IsMine)
                photonView.RPC("ChangeEyes", RpcTarget.All, type);
        }
        //void OnActivateForcefield()
        //{
        //    if(photonView.IsMine)
        //        photonView.RPC("ActivateForcefield_RPC", RpcTarget.All);
        //}
        [PunRPC]
        void ActivateForcefield_RPC()
        {

            m_HasForcefield = true;
        }
        [PunRPC]
        void DeactivateForcefield_RPC()
        {
            m_HasForcefield = false;
        }
        //Powerups-------------------------------
        void DoubleJump()
        {
            EventManager.Get().ActivateDoubleJump();
            m_HasDoubleJump = true;
            m_PowerupName.text = "Double Jump";
        }
        void WaterBaloon()
        {
            m_PowerupName.text = "Water Baloon";
            EventManager.Get().ActivateWaterBaloon();
        }
        void Forcefield()
        {
            m_PowerupName.text = "Forcefield";
            if (photonView.IsMine)
            {
                PhotonNetwork.Instantiate("Forcefield", new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z), transform.rotation);
            }
            photonView.RPC("ActivateForcefield_RPC", RpcTarget.All);
        }
    }
}
