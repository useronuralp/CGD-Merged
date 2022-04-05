using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
public class Ragdoll : MonoBehaviourPunCallbacks 
{
    private Collider[] m_Colliders;
    private Animator m_Animator;
    private bool m_IsInRagdollState = false;
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
    private void ActivateRagdoll()
    {
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
    [PunRPC]
    private void DeactivateRagdoll()
    {
        m_HipsTransformView.m_SynchronizePosition = false;
        foreach (var collider in m_RagdollColliders)
        {
            collider.isTrigger = true;
            collider.gameObject.GetComponent<Rigidbody>().isKinematic = true;
        }
        if (photonView.IsMine)
            EventManager.Get().EnableInput();
        m_Animator.enabled = true;
        m_IsInRagdollState = false;
        m_Rigidbody.velocity = Vector3.zero;
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
            EventManager.Get().DisableInput();
            StartCoroutine(DelayedRagdoll(0.1f));
            m_Rigidbody.AddForceAtPosition(new Vector3(-hitDirection.x, 2, -hitDirection.z) * force, hitPoint, ForceMode.VelocityChange);
        }
    }
    IEnumerator DelayedRagdoll(float delay)
    {
        yield return new WaitForSeconds(delay);
        photonView.RPC("ActivateRagdoll", RpcTarget.All);
    }
}
 