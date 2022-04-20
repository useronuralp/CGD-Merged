using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using TMPro;

public class ChatBox : MonoBehaviourPunCallbacks
{
    public int MaxMessages = 25;

    private List<TextMeshProUGUI> m_MessageList = new List<TextMeshProUGUI>();
    public TextMeshProUGUI MessagePrefab;
    public void Send(string message)
    {
        photonView.RPC("SendMessage_RPC", RpcTarget.All, message, PhotonNetwork.NickName);
    }
    [PunRPC]
    void SendMessage_RPC(string message, string nickname)
    {
        if (m_MessageList.Count > MaxMessages)
        {
            Destroy(m_MessageList[0].gameObject);
            m_MessageList.Remove(m_MessageList[0]);
        }

        var instantiated = Instantiate(MessagePrefab, transform);
        m_MessageList.Add(instantiated);

        instantiated.GetComponent<TextMeshProUGUI>().text = "[" + nickname + "] " + message; 
    }
}
