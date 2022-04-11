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
        public static GameObject     s_LocalPlayerInstance; //The static variables are initialized to defualt values whenever a new player joins the game. Therefore, each time a new player joins, they will set the s_localPlayerInstance to "null".
        private Rigidbody            m_Rigidbody;

        [SerializeField]
        private bool                 m_IsBulldog;

        private Vector3              m_BulldogSpawnPoint;
        private Vector3              m_RunnerSpawnPoint;

        private Dictionary<int, int> m_EvenSpawnPoints;
        private Dictionary<int, int> m_OddSpawnPoints;

        static public int           s_BulldogCount = 0;
        static public int           s_RunnerCount = 0;

        private float               m_SpawnSpacing;

        private bool                m_HasRoundStarted = false;

        private bool                m_HasCrossedTheFinishLine = false;

        private Animator            m_Animator;
        public void Respawn()
        {
            PlacePlayer(false);
        }
        private void Start()
        {
            if(SceneManagerHelper.ActiveSceneBuildIndex == 2)
                photonView.RPC("SetOutlineColor", RpcTarget.All, new object[] {0.0f, 0.0f, 0.0f, 0.0f});
            if (photonView.IsMine)
                photonView.RPC("SetNickname", RpcTarget.AllBuffered, PhotonNetwork.NickName);
            m_Animator = GetComponent<Animator>();
            m_EvenSpawnPoints = new Dictionary<int, int>() { {0,1}, {2,2}, {4,3}, {6,4}, {8,5}, {10,6}, {12,7}, {14,8}, {16,9}, {18,10} };
            m_OddSpawnPoints  = new Dictionary<int, int>() { {1,1}, {3,2}, {5,3}, {7,4}, {9,5}, {11,6}, {13,7}, {15,8}, {17,9}, {19,10} };
            m_Rigidbody = GetComponent<Rigidbody>();
            if (photonView.IsMine)
            {
                s_LocalPlayerInstance = gameObject;
                GetComponent<Camera>().StartFollowing();
            }
            if(PhotonNetwork.IsMasterClient)
                Utility.RaiseEvent(false, EventType.IncreaseInstantiatedPlayerCount, ReceiverGroup.All, EventCaching.DoNotCache, true); // PlaygroundManager catches this.
            
        }
        private void Update()
        {
            //Debug.LogError("B ->" + s_BulldogCount + " R ->" + s_RunnerCount);
        }
        [PunRPC]
        private void SetOutlineColor(object[] data)
        {
            GetComponent<Outline>().OutlineColor = new Color((float)data[0], (float)data[1], (float)data[2], (float)data[3]);
        }
        [PunRPC]
        private void SetNickname(string name)
        {
            transform.Find("NameCanvas").Find("PlayerName").GetComponent<TextMeshProUGUI>().text = name;
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
        }
        private void SetRunnerParams()
        {
            m_IsBulldog = false;
            transform.tag = "Runner";
            transform.Find("NameCanvas").Find("PlayerName").GetComponent<TextMeshProUGUI>().color = Color.blue;
            GetComponent<Outline>().OutlineColor = Color.blue;
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
        public void PlacePlayer(bool isFirstRound)
        {
            m_HasCrossedTheFinishLine = false;
            if (m_IsBulldog)
            {
                //Debug.LogError("Bulldog Count:" + s_BulldogCount);
                if(s_BulldogCount % 2 == 0)
                    transform.position = new Vector3(m_BulldogSpawnPoint.x + m_EvenSpawnPoints[s_BulldogCount] * m_SpawnSpacing, m_BulldogSpawnPoint.y, m_BulldogSpawnPoint.z);
                else
                    transform.position = new Vector3(m_BulldogSpawnPoint.x - m_OddSpawnPoints[s_BulldogCount] * m_SpawnSpacing, m_BulldogSpawnPoint.y, m_BulldogSpawnPoint.z);

                s_BulldogCount++;
            }
            else
            {
                //Debug.LogError("Runner Count:" + s_RunnerCount);
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

            if(s_BulldogCount == PhotonNetwork.CurrentRoom.PlayerCount) // Check bulldog count while placing the players. If there are no runners during this stage it means the game is won by the Bulldogs.
            {
                if(PhotonNetwork.IsMasterClient)
                    Utility.RaiseEvent(true, EventType.BulldogsWin, ReceiverGroup.All, EventCaching.DoNotCache, true);
            }
            if(!isFirstRound)
                StartCoroutine(ReleaseCameraDelayed(0.2f));
        }
        [PunRPC]
        public void SyncBulldogAndRunnerCounts(int[] countArr)
        {
            //Debug.LogError("Sync runners and bulldog counts: " + "R: " + s_RunnerCount + " B: " + s_BulldogCount);
            s_BulldogCount = countArr[0];
            s_RunnerCount = countArr[1];
        }
        [PunRPC]
        public void CrossFinishLine() // Use only on runners, Bulldogs can't cross the finish line
        {
            m_HasCrossedTheFinishLine = true;
        }
        [PunRPC]
        public void Init(Vector3 bulldogSpawnPoint, Vector3 runnerSpawnPoint, float spawnSpacing)
        {
            m_BulldogSpawnPoint = bulldogSpawnPoint;
            m_RunnerSpawnPoint = runnerSpawnPoint;  
            m_SpawnSpacing = spawnSpacing;
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
                    if(PhotonNetwork.IsMasterClient && !m_HasCrossedTheFinishLine && !m_IsBulldog) // If a runner hasn't crossed the line when the round ends, they turn into bulldogs.
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
            if(m_HasRoundStarted)
            {
                if(collision.transform.CompareTag("Bulldog") && !m_IsBulldog)
                {
                    photonView.RPC("BecomeBulldogByCollision", RpcTarget.All);
                }
            }
        }
        private void OnTriggerEnter(Collider other)
        {
            if (other.transform.CompareTag("FinishLine") && !m_IsBulldog)
            {
                photonView.RPC("CrossFinishLine", RpcTarget.All);
            }
        }
        IEnumerator ReleaseCameraDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            Debug.LogError("Released Camera");
            Utility.RaiseEvent(true, EventType.ReleaseCamera, ReceiverGroup.All, EventCaching.DoNotCache, true);
        }
    }
}
