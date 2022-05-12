using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class Powerup : MonoBehaviourPunCallbacks
{
    private Vector3 m_StartPos;
    public float appearTime = 2.5f;
    private MeshRenderer m_MR;
    private Collider m_Collider;
    void Start()
    {
        m_Collider = GetComponent<Collider>();
        m_MR = GetComponent<MeshRenderer>();
        m_StartPos = transform.position;
    }
    void Update()
    {
        transform.position = new Vector3 (transform.position.x, m_StartPos.y + Mathf.Sin(Time.time / 0.5f) / 4, transform.position.z);
    }
    [PunRPC]
    void Disappear_RPC()
    {
        m_Collider.enabled = false;
        m_MR.enabled = false;
        if(PhotonNetwork.IsMasterClient)
            StartCoroutine(Appear(appearTime));
    }
    [PunRPC]
    void Appear_RPC()
    {
        m_Collider.enabled = true;
        m_MR.enabled = true;
    }
    IEnumerator Appear(float time)
    {
        yield return new WaitForSeconds(time);
        if (PhotonNetwork.IsMasterClient)
            photonView.RPC("Appear_RPC", RpcTarget.All);
    }
    public void TriggerDisappear()
    {
        if(PhotonNetwork.IsMasterClient)
            photonView.RPC("Disappear_RPC", RpcTarget.All);
    }
}
