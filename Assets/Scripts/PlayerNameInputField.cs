using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using TMPro;
namespace Game
{
    public class PlayerNameInputField : MonoBehaviour
    {
        // Store the PlayerPref Key to avoid typos
        public GameObject PlayerNameBox;
        const string m_PlayerNamePrefKey = "Player Name";
        void Start()
        {
            string defaultName = string.Empty;
            InputField _inputField = GetComponent<InputField>();
            PlayerNameBox.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString(m_PlayerNamePrefKey);
            defaultName = PlayerPrefs.GetString(m_PlayerNamePrefKey);
            if (_inputField)
            {
                if (PlayerPrefs.HasKey(m_PlayerNamePrefKey))
                {
                    defaultName = PlayerPrefs.GetString(m_PlayerNamePrefKey);
                    _inputField.text = defaultName;
                }
            }
            PhotonNetwork.NickName = defaultName;
        }
        public void SetPlayerName(string value)
        {
            // #Important
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("Player Name is null or empty");
                return;
            }
            PhotonNetwork.NickName = value;
            PlayerPrefs.SetString(m_PlayerNamePrefKey, value);
            PlayerNameBox.GetComponent<TextMeshProUGUI>().text = PlayerPrefs.GetString(m_PlayerNamePrefKey);
        }
    }
}
