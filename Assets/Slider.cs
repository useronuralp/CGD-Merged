using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Slider : MonoBehaviour
{
    public AudioMixer m_Mixer;
    private UnityEngine.UI.Slider m_Slider;
    private void Start()
    {
        m_Slider = GetComponent<UnityEngine.UI.Slider>();
        float volume = PlayerPrefs.GetFloat("volume", 1);
        m_Slider.value = volume;
        m_Mixer.SetFloat("volume", Mathf.Log10(volume) * 20);
    }
    public void OnValueChanged(float value)
    {
        m_Mixer.SetFloat("volume", Mathf.Log10(value) * 20);
        PlayerPrefs.SetFloat("volume", value);
    }
}
