using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class SendMessage : MonoBehaviour
{
    private bool m_InputFieldFocused;
    public TMPro.TMP_InputField m_InputField;
    private string m_Message;
    private void Start()
    {
        EventManager.Get().OnDroppingChatFocus += OnDroppingChatFocus;
    }
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return) && m_InputFieldFocused)
        {
            if(m_Message != "")
            {
                transform.parent.Find("Scroll View").Find("Viewport").Find("Content").GetComponent<ChatBox>().Send(m_Message);
                m_InputField.text = "";
            }
            m_InputField.ActivateInputField();
        }
        else if(Input.GetKeyDown(KeyCode.Return) && !m_InputFieldFocused)
        {
            EventSystem.current.SetSelectedGameObject(m_InputField.gameObject, null);
            EventManager.Get().ToggleCursor(true);
            m_InputFieldFocused = true;
        }
    }
    public void OnFocused()
    {
        transform.Find("Text Area").Find("Placeholder").GetComponent<TMPro.TextMeshProUGUI>().text = string.Empty;
        m_InputFieldFocused = true;
    }
    public void OnDefocused()
    {
        transform.Find("Text Area").Find("Placeholder").GetComponent<TMPro.TextMeshProUGUI>().text = "Type...";
        m_InputFieldFocused = false;
    }
    public void OnMessageChanged(string message)
    {
        m_Message = message;
    }
    public void OnDroppingChatFocus()
    {
        m_InputField.DeactivateInputField();
        EventSystem.current.SetSelectedGameObject(null, null);
        transform.Find("Text Area").Find("Placeholder").GetComponent<TMPro.TextMeshProUGUI>().text = "Type...";
    }
}
