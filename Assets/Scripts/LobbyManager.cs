using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
namespace Game
{
    public class LobbyManager : MonoBehaviourPunCallbacks
    {
        [SerializeField]
        private GameObject LobbyPanel;
        [SerializeField]
        private GameObject RoomsPanel;
        [SerializeField]
        private GameObject EnterRoomNamePanel;

        [SerializeField]
        private byte MaxPlayersPerRoom = 20;
        //[SerializeField]
        //private float _playerTTL = 300000;
        public GameObject RoomButtonPrefab;

        private Dictionary<string, RoomInfo> _cachedRoomList;
        private string _roomName = string.Empty;
        private void Awake()
        {
            _cachedRoomList = new Dictionary<string, RoomInfo>();
        }
        //Lobby Functions----------

        public void OnCreateRoomButtonPressed()
        {
            EnterRoomNamePanel.SetActive(true);
            RoomsPanel.SetActive(false);
            LobbyPanel.SetActive(false);
            EnterRoomNamePanel.transform.Find("InputField").GetComponent<TMP_InputField>().text = PhotonNetwork.NickName + "'s Game";
        }
        public void OnRoomNameEntered(string name)
        {
            _roomName = name;
        }
        public void OnCreateButtonPressed()
        {
            RoomsPanel.SetActive(false);
            LobbyPanel.SetActive(false);
            if (_roomName != string.Empty)
                PhotonNetwork.CreateRoom(_roomName, new RoomOptions { MaxPlayers = MaxPlayersPerRoom, IsVisible = true}); //Creates a room AND joins it.
            else
                Debug.Log("Room name is empty");
        }
        public void OnJoinRoomButtonPressed()
        {
            RoomsPanel.SetActive(true);
            LobbyPanel.SetActive(false);
            EnterRoomNamePanel.SetActive(false);
        }
        public void OnRoomListBackButtonPressed()
        {
            LobbyPanel.SetActive(true);
            RoomsPanel.SetActive(false);
            EnterRoomNamePanel.SetActive(false);
        }
        public void OnEnterRoomNameBackButtonPressed()
        {
            LobbyPanel.SetActive(true);
            RoomsPanel.SetActive(false);
            EnterRoomNamePanel.SetActive(false);
        }
        public void JoinRoom(Transform button)
        {
            string roomName = button.transform.Find("RoomNameText").GetComponent<TextMeshProUGUI>().text;
            PhotonNetwork.JoinRoom(roomName);
        }
        //Callbacks-------------------------
        public override void OnConnectedToMaster()
        {
            Debug.Log("Connected to MASTER server");
            SceneManager.LoadScene(1); //This loading needs no synchronization so I am not using PhotonNetwork.LoadLevel() here.
            PhotonNetwork.JoinLobby();
        }
        public override void OnDisconnected(DisconnectCause cause)
        {
            Debug.LogWarningFormat("OnDisconnected() was called by PUN with reason {0}", cause);
            PhotonNetwork.LoadLevel(0);
        }
        public override void OnJoinRandomFailed(short returnCode, string message)
        {
            Debug.LogErrorFormat("Joining a random room failed. Reason : {0}", message);
        }
        public override void OnCreatedRoom()
        {
            Debug.Log("OnCreatedRoom() is called.");
        }
        public override void OnCreateRoomFailed(short returnCode, string message)
        {
            Debug.LogErrorFormat("OnCreateRoomFailed() called. Failed to create a room. With message: {0}", message);
        }
        public override void OnJoinedRoom()
        {
            Debug.Log("Connected to a GAME server. OnJoinedRoom() Called");
            if (PhotonNetwork.CurrentRoom.PlayerCount == 1) //Load the level only if there is one player in the room. If there is more than one player, they will automatically get synced by Photon.
            {
                PhotonNetwork.LoadLevel(2);
            }
        }
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            var content = RoomsPanel.transform.Find("Viewport").Find("Content");
            ClearRoomList();
            UpdateCachedRoomList(roomList);

            foreach (var info in _cachedRoomList)
            {
                GameObject newRoomButton = Instantiate(RoomButtonPrefab, content);

                newRoomButton.transform.Find("RoomNameText").GetComponent<TextMeshProUGUI>().text = info.Value.Name;
                if (!info.Value.IsOpen)
                    newRoomButton.transform.Find("RoomNameText").GetComponent<TextMeshProUGUI>().text += " (In Progress...)";
                newRoomButton.transform.Find("PlayerCountText").GetComponent<TextMeshProUGUI>().text = info.Value.PlayerCount + "/" + info.Value.MaxPlayers;

                newRoomButton.GetComponent<Button>().onClick.AddListener(delegate { JoinRoom(newRoomButton.transform); } );
            }
        }
        public void ClearRoomList()
        {
            var content = RoomsPanel.transform.Find("Viewport").Find("Content");
            foreach (Transform roombButton in content)
                Destroy(roombButton.gameObject);
        }
        private void UpdateCachedRoomList(List<RoomInfo> roomList)
        {
            foreach (RoomInfo info in roomList)
            {
                // Remove room from cached room list if it got closed, became invisible or was marked as removed
                if (!info.IsVisible || info.RemovedFromList)
                {
                    if (_cachedRoomList.ContainsKey(info.Name))
                    {
                        _cachedRoomList.Remove(info.Name);
                    }
                    continue;
                }
                // Update cached room info
                if (_cachedRoomList.ContainsKey(info.Name))
                {
                    _cachedRoomList[info.Name] = info;
                }
                // Add new room info to cache
                else
                {
                    _cachedRoomList.Add(info.Name, info);
                }
            }
        }
    }
}
