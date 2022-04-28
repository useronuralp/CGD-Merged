using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Powerup : MonoBehaviour
{
    private Vector3 m_StartPos;
    void Start()
    {
        m_StartPos = transform.position;
    }
    void Update()
    {
        if(Photon.Pun.PhotonNetwork.IsMasterClient)
        {
            transform.position = new Vector3 (transform.position.x, m_StartPos.y + Mathf.Sin(Time.time / 0.5f), transform.position.z);
        }
    }
}
