using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// This class is attached to Jammo in main menu scene. It randomly changes the eye texture.
/// </summary>

public class EyeChanger : MonoBehaviour
{
    private Material m_JammoEyes;
    private Dictionary<string, Vector2> m_EyeTypes;
    private float m_EyeChangeTimer = 1.0f;
    private int m_CurrentEye = 0;
    void Start()
    {
        m_JammoEyes = transform.Find("head_eyes_low").GetComponent<Renderer>().material;
        m_EyeTypes = new Dictionary<string, Vector2>();
        m_EyeTypes.Add("Default", new Vector2(0.0f, 0.0f));
        m_EyeTypes.Add("Happy", new Vector2(0.33f, 0.0f));
        m_EyeTypes.Add("Angry", new Vector2(0.66f, 0.0f));
        m_EyeTypes.Add("Dead", new Vector2(0.0f, 0.66f));
        m_EyeTypes.Add("Sad", new Vector2(0.33f, 0.66f));
    }

    // Update is called once per frame
    void Update()
    {
        m_EyeChangeTimer -= Time.deltaTime;
        if(m_EyeChangeTimer <= 0.0f)
        {
            if(m_CurrentEye == 0)
            {
                m_JammoEyes.SetTextureOffset("_MainTex", m_EyeTypes["Happy"]);
                m_CurrentEye = 1;
            }
            else
            {
                m_JammoEyes.SetTextureOffset("_MainTex", m_EyeTypes["Default"]);
                m_CurrentEye = 0;
            }
            m_EyeChangeTimer = Random.Range(0.5f, 5.0f);
        }
    }
}
