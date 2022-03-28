using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

namespace Game
{
    public class Camera : MonoBehaviourPunCallbacks
    {
        private Cinemachine.CinemachineFreeLook m_Camera;
        private void Awake()
        {
            m_Camera = GameObject.Find("PlayerCamera").GetComponent<Cinemachine.CinemachineFreeLook>();
        }
        public void StartFollowing()
        {
            m_Camera.Follow = transform;
            m_Camera.LookAt = transform;
        }
    }
}
