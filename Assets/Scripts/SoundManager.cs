using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;
public class SoundManager : MonoBehaviour
{
    private AudioSource m_AudioSource;

    private AudioClip m_LongRoadSoundtrack;
    private AudioClip m_TreetopKingdomSoundtrack;
    private bool m_IsTransitioning = false;
    void Start()
    {
        m_LongRoadSoundtrack = Resources.Load<AudioClip>("Audio/Music Tracks/SummerFunk");
        m_TreetopKingdomSoundtrack = Resources.Load<AudioClip>("Audio/Music Tracks/SeriousFunkin'Business");
        m_AudioSource = transform.GetComponent<AudioSource>();
        if(SceneManagerHelper.ActiveSceneBuildIndex == 3)
        { 
            m_AudioSource.clip = m_LongRoadSoundtrack;
            m_AudioSource.Play();
            StartCoroutine(StartFade(2, 0.1f));
            EventManager.Get().OnChangeTrack += OnChangeTrack;
        }
    }


    public IEnumerator StartFade(float duration, float targetVolume) //This fnc is useful for slowly fading in / out a music track.
    {
        m_IsTransitioning = true;
        float currentTime = 0;
        float start = m_AudioSource.volume;

        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            m_AudioSource.volume = Mathf.Lerp(start, targetVolume, currentTime / duration);
            yield return null;
        }
        m_IsTransitioning = false; 
        yield break;
    }
    void OnChangeTrack(int trackID)
    {
        if (trackID == 1)
            StartCoroutine(TransitionToFirstLevel());
        else if (trackID == 2)
            StartCoroutine(TransitionToSecondLevel());
        else if (trackID == 3)
            StartCoroutine(TransitionToNoMusic());
    }
    public IEnumerator TransitionToNoMusic() 
    {
        StartCoroutine(StartFade(0.5f, 0.0f));
        yield return new WaitWhile(() => m_IsTransitioning == true);
        m_AudioSource.clip = null;
        m_AudioSource.Stop();
        StartCoroutine(StartFade(1, 0.1f));
        yield break;
    }
    public IEnumerator TransitionToSecondLevel() 
    {
        StartCoroutine(StartFade(2, 0.0f));
        yield return new WaitWhile(() => m_IsTransitioning == true);
        m_AudioSource.clip = m_TreetopKingdomSoundtrack;
        m_AudioSource.Play();
        StartCoroutine(StartFade(2, 0.1f));
        yield return new WaitWhile(() => m_IsTransitioning == true);
        yield break;
    }
    public IEnumerator TransitionToFirstLevel() 
    {
        StartCoroutine(StartFade(2, 0.0f));
        yield return new WaitWhile(() => m_IsTransitioning == true);
        m_AudioSource.clip = m_LongRoadSoundtrack;
        m_AudioSource.Play();
        StartCoroutine(StartFade(2, 0.1f));
        yield return new WaitWhile(() => m_IsTransitioning == true);
        yield break;
    }
}
