using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Waterbaloon : MonoBehaviourPunCallbacks
{
    private float m_SelfDestroyTimer = 2.0f;
    private MeshRenderer m_Renderer;
    private Collider m_Collider;
    private bool DoOnce = true;
    private bool m_Disabled = false;
    private AudioSource m_AudioSource;
    private AudioClip m_SplashSound;
    private void Start()
    {
        m_SplashSound = Resources.Load<AudioClip>("Audio/SFX/SplashSound");
        m_AudioSource = GetComponent<AudioSource>();
        m_Renderer = GetComponent<MeshRenderer>();
        m_Collider = GetComponent<Collider>();
    }
    void Update()
    {
        if(m_SelfDestroyTimer <= 0 )
        {
            if(DoOnce && !m_Disabled)
            {
                DoOnce = false;
                if(photonView.IsMine)
                    StartCoroutine(DisableInteraction());
            }
        }
        m_SelfDestroyTimer -= Time.deltaTime;
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(!m_Disabled && photonView.IsMine)
            StartCoroutine(DisableInteraction());
    }
    [PunRPC]
    void Explode(Vector3 explodePosition)
    {
        m_AudioSource.PlayOneShot(m_SplashSound);
        m_Disabled = true;
        m_Renderer.enabled = false;
        m_Collider.enabled = false;

        Collider[] colliders = Physics.OverlapSphere(explodePosition, 2.0f); // Stun area adjustments can be done here.
        if(PhotonNetwork.IsMasterClient)
        {
            foreach(Collider col in colliders)
            {
                if(col.gameObject.CompareTag("Bulldog"))
                {
                    col.gameObject.transform.GetComponent<Game.PlayerManager>().GetStunned();
                }
            }
        }
    }
    IEnumerator DisableInteraction()
    {
        photonView.RPC("Explode", RpcTarget.All, transform.position);
        var sys = PhotonNetwork.Instantiate("SplashVFX", transform.position, transform.rotation);
        yield return new WaitForSeconds(sys.GetComponent<ParticleSystem>().main.startLifetime.constantMax);
        m_AudioSource.Stop();
        PhotonNetwork.Destroy(sys);
        Destroy(gameObject);
    }
}
