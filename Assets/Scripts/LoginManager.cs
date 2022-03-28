using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
namespace Game
{
    public class LoginManager : MonoBehaviourPunCallbacks
    {
        string gameVersion = "1";
        void Awake()
        {
            PhotonNetwork.AutomaticallySyncScene = true;
            PhotonNetwork.SerializationRate = 15;
        }
        public void Connect()
        {
            if (PhotonNetwork.IsConnected)
            {
                //PhotonNetwork.JoinRandomRoom();
            }
            else
            {
                PhotonNetwork.ConnectUsingSettings(); //Connects to Photon online server ("NameServer" I think)
                PhotonNetwork.GameVersion = gameVersion;
            }
        }
        //------------- Callbacks------------------------------
        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to MASTER server");
            SceneManager.LoadScene(1); //This loading needs no synchronization so I am not using PhotonNetwork.LoadLevel() here.
            PhotonNetwork.JoinLobby();
        }
        public override void OnJoinedLobby()
        {
            Debug.Log("Joined a lobby. OnJoinedLobby()");
        }
        public override void OnLeftLobby()
        {
            Debug.Log("Left a lobby. OnLeftLobby()");
        }
    }
}
