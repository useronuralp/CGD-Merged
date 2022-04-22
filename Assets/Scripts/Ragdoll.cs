using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class Ragdoll : MonoBehaviourPunCallbacks 
{
    private Animator m_Animator;
    public bool m_IsInRagdollState = false;
    private Rigidbody m_Rigidbody;

    private GameObject m_Hips;
    private Rigidbody m_HipsRigidBody;
    private PhotonTransformView m_HipsTransformView;

    private float m_GetUpTimer;
    private float m_GetUpCooldown = 1.0f;
    private List<Collider> m_RagdollColliders;
    private void Awake()
    {
        m_RagdollColliders = new List<Collider>();
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Animator = GetComponent<Animator>();
        m_GetUpTimer = m_GetUpCooldown;

        foreach (var collider in gameObject.GetComponentsInChildren<Collider>())
        {
            if (collider.gameObject != gameObject)
            {
                m_RagdollColliders.Add(collider);
                collider.isTrigger = true;
                collider.gameObject.GetComponent<Rigidbody>().isKinematic = true;
            }
        }
    }
    private void Start()
    {
        foreach (var collider in m_RagdollColliders)
        {
            if (collider.name.Contains("Hips"))
            {
                m_Hips = collider.gameObject;
                m_HipsRigidBody = m_Hips.GetComponent<Rigidbody>();
                m_HipsTransformView = m_Hips.GetComponent<PhotonTransformView>();
                return;
            }
        }
    }
    void Update()
    {
        if(m_IsInRagdollState && photonView.IsMine)
        {
            if(m_Rigidbody.velocity.magnitude <= 1)
            {
                m_GetUpTimer -= Time.deltaTime;
                if(m_GetUpTimer <= 0.0)
                {
                    m_GetUpTimer = m_GetUpCooldown;
                    photonView.RPC("DeactivateRagdoll", RpcTarget.All);
                }
            }
            else
            {
                m_GetUpTimer = m_GetUpCooldown;
            }
        }
    }
    private void FixedUpdate()
    {
        if(m_IsInRagdollState)
            m_HipsRigidBody.MovePosition(m_Rigidbody.transform.position);
    }
    [PunRPC]
    private void ActivateRagdoll(Game.SenderType type)
    {
        if(!m_IsInRagdollState)
        {
            if(photonView.IsMine)
            {
                EventManager.Get().DisableInput(type);
                EventManager.Get().StartRagdolling();
            }
            m_HipsTransformView.m_SynchronizePosition = true;
            foreach (var collider in m_RagdollColliders)
            {
                collider.isTrigger = false;
                collider.gameObject.GetComponent<Rigidbody>().isKinematic = false;
            }

            m_Animator.enabled = false;
            m_IsInRagdollState = true;
            ResetRagdollVelocity();
        }
    }
    [PunRPC]
    private void DeactivateRagdoll()
    {
        m_Rigidbody.rotation = Quaternion.Euler(0, m_Hips.transform.rotation.eulerAngles.y, 0);
        m_GetUpTimer = m_GetUpCooldown; // Reset timer for everyone.
        if(photonView.IsMine)
            EventManager.Get().StopRagdolling();
        m_HipsTransformView.m_SynchronizePosition = false;
        foreach (var collider in m_RagdollColliders)
        {
            collider.isTrigger = true;
            collider.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
        m_Animator.SetTrigger("GetUp");
        if (photonView.IsMine)
            EventManager.Get().StartedGettingUp();
        m_Animator.enabled = true;
        m_Rigidbody.velocity = Vector3.zero;
        m_IsInRagdollState = false;
    }
    public void EnableInput() //To be used by animation events.
    {
        if (photonView.IsMine)
        {
            EventManager.Get().StoppedGettingUp();
            EventManager.Get().EnableInput();
        }
    }
    public void DisableInput()
    {
        if (photonView.IsMine)
        {
            EventManager.Get().StartedGettingUp();
            EventManager.Get().DisableInput(Game.SenderType.Standard);
        }
    }
    private void ResetRagdollVelocity()
    {
        foreach (var collider in m_RagdollColliders)
                collider.GetComponent<Rigidbody>().velocity = Vector3.zero;
    }
    public void Bounce(Vector3 hitDirection, Vector3 hitPoint, float force)
    {
        if(!m_IsInRagdollState && photonView.IsMine)
        {
            photonView.RPC("ActivateRagdoll", RpcTarget.All, Game.SenderType.HitByObstacle);
            m_Rigidbody.AddForceAtPosition(new Vector3(-hitDirection.x, 2, -hitDirection.z) * force, hitPoint, ForceMode.VelocityChange);
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        if(!m_IsInRagdollState && photonView.IsMine)
        {
            if(collision.transform.CompareTag("Bulldog") || collision.transform.CompareTag("Runner") || collision.transform.CompareTag("Player") )
            {
                if(!GetComponent<Game.Controls>().IsGrounded() && GetComponent<Game.Controls>().DistanceToGround() >= 3.5f)
                {
                    photonView.RPC("ActivateRagdoll", RpcTarget.All, Game.SenderType.Standard);
                    collision.transform.GetComponent<Ragdoll>().photonView.RPC("ActivateRagdoll", RpcTarget.All, Game.SenderType.Standard);
                }
            }
        }
    }
}
 