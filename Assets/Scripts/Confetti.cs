using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Confetti : MonoBehaviour
{
    private bool DoOnce = true;
    private GameObject m_Sys;
    private GameObject m_SmokeSys;
    private float m_BeatDropTime = 9.8f;
    private AudioSource m_AudioSource;
    private AudioClip m_CannonSound;
    void Start()
    {
        m_CannonSound = Resources.Load<AudioClip>("Audio/SFX/CannonSound");
        m_AudioSource = GameObject.Find("MusicPlayer").GetComponent<AudioSource>();
        m_SmokeSys = transform.Find("Smoke Particles Circle").gameObject;
        m_Sys = transform.Find("Confetti Particles Cone").gameObject;
    }

    // Update is called once per frame
    void Update()
    {
        if(m_BeatDropTime <= 0)
        {
            if(DoOnce)
            {
                DoOnce = false;
                m_SmokeSys.SetActive(true);
                m_Sys.SetActive(true);
                m_AudioSource.PlayOneShot(m_CannonSound, 0.1f);
            }
        }
        m_BeatDropTime -= Time.deltaTime;
    }
}
