using System.Collections.Generic;
using Photon.Pun;
using TMPro;

public class ChatBox : MonoBehaviourPunCallbacks
{
    public int MaxMessages = 25; // Number of maximum messages that can be stored in the chat box. After this value the oldest messages start to get deleted.

    private List<TextMeshProUGUI> m_MessageList = new List<TextMeshProUGUI>();
    public TextMeshProUGUI MessagePrefab; // The prefab we will be creating and posting on chat whenever someone types someting. 
    public void Send(string message)
    {
        photonView.RPC("SendMessage_RPC", RpcTarget.All, message, PhotonNetwork.NickName, Game.PlayerManager.s_LocalPlayerInstance.GetComponent<Game.PlayerManager>().m_IsBulldog);
    }
    [PunRPC]
    void SendMessage_RPC(string message, string nickname, bool isBulldog)
    {
        if (m_MessageList.Count > MaxMessages)
        {
            Destroy(m_MessageList[0].gameObject);
            m_MessageList.Remove(m_MessageList[0]);
        }
        var instantiated = Instantiate(MessagePrefab, transform);
        m_MessageList.Add(instantiated);


        if(SceneManagerHelper.ActiveSceneBuildIndex == 2)
        {
            instantiated.GetComponent<TextMeshProUGUI>().text = $"<color=green>[" + nickname + "]: </color>" + message;
        }
        else if(SceneManagerHelper.ActiveSceneBuildIndex == 3)
        {
            if(isBulldog)
                instantiated.GetComponent<TextMeshProUGUI>().text = $"<color=red>[" + nickname + "]: </color>" + message; 
            else
                instantiated.GetComponent<TextMeshProUGUI>().text = $"<color=#1F8FAD>[" + nickname + "]: </color>" + message;
        }
    }
}
