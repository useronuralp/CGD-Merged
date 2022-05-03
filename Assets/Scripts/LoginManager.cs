using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;
namespace Game
{
    public class LoginManager : MonoBehaviourPunCallbacks
    {
        string gameVersion = "1";
        private GameObject m_ErrorMessage;
        private float m_Timer = 3;
        private bool m_MessageUp = false;
        void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.SerializationRate = 15;
        }
        private void Start()
        {
            m_ErrorMessage = GameObject.Find("Canvas").transform.Find("Error").gameObject;
        }
        private void Update()
        {
            if(m_MessageUp)
            {
                if(m_Timer <= 0)
                {
                    m_MessageUp = false;
                    m_ErrorMessage.SetActive(false);
                    m_Timer = 3;
                }
                m_Timer -= Time.deltaTime;
            }
        }
        public void Connect()
        {
            if (!PhotonNetwork.IsConnected)
            {
                if(PhotonNetwork.NickName != "Player Name" && PhotonNetwork.NickName != "" && PhotonNetwork.NickName.Length >= 3 && PhotonNetwork.NickName.Length <= 10)
                {
                    PhotonNetwork.ConnectUsingSettings(); //Connects to Photon online server ("NameServer" I think)
                    PhotonNetwork.GameVersion = gameVersion;
                }
                else
                {
                    m_ErrorMessage.SetActive(true);
                    m_ErrorMessage.GetComponent<TextMeshProUGUI>().text = "Enter a proper name";
                    m_MessageUp = true;
                }
            }
        }
        //------------- Callbacks------------------------------
        public override void OnConnectedToMaster()
        {
            //Debug.Log("Connected to MASTER server");
            SceneManager.LoadScene(1); //This loading needs no synchronization so I am not using PhotonNetwork.LoadLevel() here.
            PhotonNetwork.JoinLobby();
        }
        public override void OnJoinedLobby()
        {
            //Debug.Log("Joined a lobby. OnJoinedLobby()");
        }
        public override void OnLeftLobby()
        {
            //Debug.Log("Left a lobby. OnLeftLobby()");
        }
    }
}
