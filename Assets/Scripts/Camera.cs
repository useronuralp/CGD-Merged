using UnityEngine;
using Photon.Pun;

/// <summary>
/// Camera class that is attached to players. The class determines which game object to follow during runtime. It has a very basic functionality.
/// </summary>
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
