using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SendMessage : MonoBehaviour
{
    private bool m_IsInputFieldFocused;
    private string m_Message;

    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) && m_IsInputFieldFocused)
        {
            if(m_Message != "")
            {
                transform.parent.Find("Scroll View").Find("Viewport").Find("Content").GetComponent<ChatBox>().Send(m_Message);
            }
        }
    }
    public void OnFocused()
    {
        m_IsInputFieldFocused = true;
    }
    public void OnDefocused()
    {
        m_IsInputFieldFocused = false;
    }
    public void OnMessageChanged(string message)
    {
        m_Message = message;
    }
}
