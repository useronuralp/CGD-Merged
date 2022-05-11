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
        private GameObject m_Camera;
        private Animator m_CameraAnimator;
        private bool m_IsCameraMoved = false;
        private bool m_IsCameraZoomed = false;
        private GameObject m_GameTitle;
        private Animator m_JammoAnimator;
        private GameObject m_Jammo;
        private List<GameObject> m_JammoParts;

        private int m_ActiveHeadPiece = 0;
        private int m_ActiveEyePiece = 0;
        private int m_ActiveBodyColor = 0;

        private Dictionary<int, GameObject> m_HeadItems;
        private Dictionary<int, GameObject> m_EyeItems;
        private Dictionary<int, Material> m_BodyColors;

        private GameObject m_CustomizationBackButton;
        private GameObject m_CustomizationPanel;

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
        private void Start()
        {
            m_Jammo = GameObject.Find("Custom Rogue Bot").transform.Find("Jammo_Player").gameObject;

            m_JammoParts = new List<GameObject>();
            foreach(Transform transform in m_Jammo.transform)
            {
                if(transform.gameObject.name != "Armature.001" && transform.gameObject.name != "head_eyes_low")
                {
                    m_JammoParts.Add(transform.gameObject);
                }
            }

            m_HeadItems = new Dictionary<int, GameObject>();
            m_EyeItems = new Dictionary<int, GameObject>();
            m_BodyColors = new Dictionary<int, Material>();

            m_CustomizationPanel = GameObject.Find("Canvas").transform.Find("CustomizationPanel").gameObject;
            m_HeadItems.Add(1, RecursiveFindChild(m_Jammo.transform, "Top Hat").gameObject);
            m_HeadItems.Add(2, RecursiveFindChild(m_Jammo.transform, "FlamingoHat").gameObject);
            m_HeadItems.Add(3, RecursiveFindChild(m_Jammo.transform, "SafariHat").gameObject);
            m_HeadItems.Add(4, RecursiveFindChild(m_Jammo.transform, "WolfEars").gameObject);
            m_HeadItems.Add(5, RecursiveFindChild(m_Jammo.transform, "StrawHat").gameObject);
            m_HeadItems.Add(6, RecursiveFindChild(m_Jammo.transform, "VikingHat").gameObject);
            m_HeadItems.Add(7, RecursiveFindChild(m_Jammo.transform, "Headphones").gameObject);
            m_HeadItems.Add(8, RecursiveFindChild(m_Jammo.transform, "Crown").gameObject);

            m_EyeItems.Add(1, RecursiveFindChild(m_Jammo.transform, "Glasses").gameObject);
            m_BodyColors.Add(0, Resources.Load<Material>("JammoMaterials/m_jammo_metal_red"));
            m_BodyColors.Add(1, Resources.Load<Material>("JammoMaterials/m_jammo_metal_black"));
            m_BodyColors.Add(2, Resources.Load<Material>("JammoMaterials/m_jammo_metal_blue"));
            m_BodyColors.Add(3, Resources.Load<Material>("JammoMaterials/m_jammo_metal_yellow"));

            m_ActiveHeadPiece = PlayerPrefs.GetInt("HeadItem", 0);
            m_ActiveEyePiece = PlayerPrefs.GetInt("EyeItem", 0);
            m_ActiveBodyColor = PlayerPrefs.GetInt("BodyColor", 0);
            ActivateProperItem(m_HeadItems, m_ActiveHeadPiece);
            ActivateProperItem(m_EyeItems, m_ActiveEyePiece);
            ActivateBodyColor(m_ActiveBodyColor);

            m_CustomizationBackButton = GameObject.Find("UI").transform.Find("LobbyPanel").Find("CustomizationBackButton").gameObject;
            m_GameTitle = GameObject.Find("TitleCanvas").transform.Find("Title").gameObject;
            m_JammoAnimator = m_Jammo.transform.GetComponent<Animator>();
            m_Camera = GameObject.Find("Main Camera");
            m_CameraAnimator = m_Camera.GetComponent<Animator>();
        }
        //Lobby Functions----------

        public void OnCreateRoomButtonPressed()
        {
            if(!m_IsCameraZoomed)
            {
                EnterRoomNamePanel.SetActive(true);
                RoomsPanel.SetActive(false);
                EnterRoomNamePanel.transform.Find("InputField").GetComponent<TMP_InputField>().text = PhotonNetwork.NickName + "'s Game";
                if(!m_IsCameraMoved)
                {
                    m_IsCameraMoved = true;
                    m_CameraAnimator.SetTrigger("MoveRight");
                }
            }
        }
        public void OnRoomNameEntered(string name)
        {
            _roomName = name;
        }
        public void OnCreateButtonPressed()
        {
            RoomsPanel.SetActive(false);
            //LobbyPanel.SetActive(false);
            if (_roomName != string.Empty)
                PhotonNetwork.CreateRoom(_roomName, new RoomOptions { MaxPlayers = MaxPlayersPerRoom, IsVisible = true}); //Creates a room AND joins it.
            else
                Debug.Log("Room name is empty");
        }
        public void OnJoinRoomButtonPressed()
        {
            if(!m_IsCameraZoomed)
            {
                RoomsPanel.SetActive(true);
                EnterRoomNamePanel.SetActive(false);
                if (!m_IsCameraMoved)
                {
                    m_IsCameraMoved = true;
                    m_CameraAnimator.SetTrigger("MoveRight");
                }
            }
        }
        public void OnRoomListBackButtonPressed()
        {
            LobbyPanel.SetActive(true);
            RoomsPanel.SetActive(false);
            EnterRoomNamePanel.SetActive(false);
            if (m_IsCameraMoved)
            {
                m_IsCameraMoved = false;
                m_CameraAnimator.SetTrigger("MoveBack");
            }
        }
        public void OnEnterRoomNameBackButtonPressed()
        {
            LobbyPanel.SetActive(true);
            RoomsPanel.SetActive(false);
            EnterRoomNamePanel.SetActive(false);
            if (m_IsCameraMoved)
            {
                m_IsCameraMoved = false;
                m_CameraAnimator.SetTrigger("MoveBack");
            }
        }
        public void OnCustomizeButtonPressed()
        {
            if(!m_IsCameraMoved && !m_IsCameraZoomed)
            {
                m_CustomizationPanel.SetActive(true);
                m_JammoAnimator.SetTrigger("Idle");
                m_CameraAnimator.SetTrigger("Zoom");
                m_GameTitle.SetActive(false);
                StartCoroutine(ButtonSwitch(true));
                m_IsCameraZoomed = true;
            }
        }
        public void OnCustomizationBackButtonPressed()
        {
            if(m_IsCameraZoomed)
            {
                m_CustomizationPanel.SetActive(false);
                m_JammoAnimator.SetTrigger(Random.Range(1,10).ToString());
                m_CameraAnimator.SetTrigger("ZoomBack");
                m_GameTitle.SetActive(true);
                StartCoroutine(ButtonSwitch(false));
                m_IsCameraZoomed = false;
            }
        }
        void ActivateProperItem(Dictionary<int, GameObject> dict, int itemID)
        {
            if(itemID == 0)
            {
                foreach (var pair in dict)
                {
                    pair.Value.SetActive(false);
                }
                return;
            }
            foreach (var pair in dict)
            {
                if (pair.Key == itemID)
                {
                    pair.Value.SetActive(true);
                }
                else
                {
                    pair.Value.SetActive(false);
                }
            }
        }
        void ActivateBodyColor(int colorID)
        {
            for (int i = 0; i< m_JammoParts.Count; i++)
            {
                m_JammoParts[i].GetComponent<Renderer>().material = m_BodyColors[colorID];
            }
        }
        public void OnHeadRightButtonPressed()
        {
            m_ActiveHeadPiece = ++m_ActiveHeadPiece % (m_HeadItems.Count + 1);
            PlayerPrefs.SetInt("HeadItem", m_ActiveHeadPiece);
            ActivateProperItem(m_HeadItems, m_ActiveHeadPiece);
        }
        public void OnHeadLeftButtonPressed()
        {
            m_ActiveHeadPiece = --m_ActiveHeadPiece;
            if (m_ActiveHeadPiece < 0)
                m_ActiveHeadPiece = m_HeadItems.Count;
            PlayerPrefs.SetInt("HeadItem", m_ActiveHeadPiece);
            ActivateProperItem(m_HeadItems, m_ActiveHeadPiece);
        }
        public void OnEyesRightButtonPressed()
        {
            m_ActiveEyePiece = ++m_ActiveEyePiece % (m_EyeItems.Count + 1);
            PlayerPrefs.SetInt("EyeItem", m_ActiveEyePiece);
            ActivateProperItem(m_EyeItems, m_ActiveEyePiece);
        }
        public void OnEyesLeftButtonPressed()
        {
            m_ActiveEyePiece = --m_ActiveEyePiece;
            if (m_ActiveEyePiece < 0)
                m_ActiveEyePiece = m_EyeItems.Count;
            PlayerPrefs.SetInt("EyeItem", m_ActiveEyePiece);
            ActivateProperItem(m_EyeItems, m_ActiveEyePiece);
        }
        public void OnColorRightButtonPressed()
        {
            m_ActiveBodyColor = ++m_ActiveBodyColor % m_BodyColors.Count;
            PlayerPrefs.SetInt("BodyColor", m_ActiveBodyColor);
            ActivateBodyColor(m_ActiveBodyColor);
        }
        public void OnColorLeftButtonPressed()
        {
            m_ActiveBodyColor = --m_ActiveBodyColor;
            if (m_ActiveBodyColor < 0)
                m_ActiveBodyColor = m_BodyColors.Count - 1;
            PlayerPrefs.SetInt("BodyColor", m_ActiveBodyColor);
            ActivateBodyColor(m_ActiveBodyColor);
        }
        IEnumerator ButtonSwitch(bool onOrOff)
        {
            yield return new WaitForSeconds(0.3f);
            if(onOrOff)
                m_CustomizationBackButton.SetActive(true);
            else
                m_CustomizationBackButton.SetActive(false);

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
                GameObject.Find("Canvas").transform.Find("BlackScreen").GetComponent<Animator>().SetTrigger("FadeOutLobby");
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
        Transform RecursiveFindChild(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }
                else
                {
                    Transform found = RecursiveFindChild(child, childName);
                    if (found != null)
                    {
                        return found;
                    }
                }
            }
            return null;
        }
    }
}
