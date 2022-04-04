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

    private float m_GetUpTimer;
    private float m_GetUpCooldown = 2.0f;
    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Animator = GetComponent<Animator>();
        m_GetUpTimer = m_GetUpCooldown;
        //DeactivateRagdoll();
        foreach (var collider in gameObject.GetComponentsInChildren<Collider>())
        {
            if (collider.gameObject != gameObject)
                collider.isTrigger = true;
        }
    }
    private void Start()
    {
        foreach(var collider in GetComponentsInChildren<Collider>())
        {
            if(collider.name.Contains("Hips"))
                m_Hips = collider.gameObject;
        }
    }
    void Update()
    {
        if(m_IsInRagdollState)
        {
            m_GetUpTimer -= Time.deltaTime;
            if(m_GetUpTimer <= 0.0)
            {
                m_GetUpTimer = m_GetUpCooldown;
                DeactivateRagdoll();
            }
        }
    }
    private void FixedUpdate()
    {
        if(m_IsInRagdollState)
        {
            m_Hips.GetComponent<Rigidbody>().MovePosition(m_Rigidbody.transform.position);
        }
    }
    private void ActivateRagdoll()
    {
        Debug.LogError("Activated Ragdoll");
        m_Hips.GetComponent<PhotonTransformView>().m_SynchronizePosition = true;
        //m_Hips.GetComponent<PhotonRigidbodyView>().m_SynchronizeAngularVelocity = true;
        //m_Hips.GetComponent<PhotonRigidbodyView>().m_SynchronizeVelocity = true;
        m_Colliders = gameObject.GetComponentsInChildren<Collider>();
        foreach (var collider in m_Colliders)
        {
            if (collider.gameObject != gameObject)
                collider.isTrigger = false;
        }
        m_Animator.enabled = false;
        m_IsInRagdollState = true;
        ResetRagdollVelocity();
    }
    
    private void DeactivateRagdoll()
    {
        Debug.LogError("Deactivated Ragdoll");
        m_Hips.GetComponent<PhotonTransformView>().m_SynchronizePosition = false;
        //m_Hips.GetComponent<PhotonRigidbodyView>().m_SynchronizeAngularVelocity = false;
        //m_Hips.GetComponent<PhotonRigidbodyView>().m_SynchronizeVelocity = false;
        m_Colliders = gameObject.GetComponentsInChildren<Collider>();
        foreach (var collider in m_Colliders)
        {
            if (collider.gameObject != gameObject)
                collider.isTrigger = true;
        }
        if (photonView.IsMine)
            EventManager.Get().EnableInput();
        m_Animator.enabled = true;
        m_IsInRagdollState = false;
        m_Rigidbody.velocity = Vector3.zero;
    }
    private void ResetRagdollVelocity()
    {
        m_Colliders = gameObject.GetComponentsInChildren<Collider>();
        foreach (var collider in m_Colliders)
        {
            if (collider.gameObject != gameObject)
                collider.GetComponent<Rigidbody>().velocity = Vector3.zero; // TODO: Stash
        }
    }
    public void Bounce(Vector3 hitDirection, Vector3 hitPoint, float force)
    {
        if(!m_IsInRagdollState)
        {
            if (photonView.IsMine)
                EventManager.Get().DisableInput();
            StartCoroutine(DelayedRagdoll(0.5f));
            m_Rigidbody.AddForceAtPosition(new Vector3(-hitDirection.x, 2, -hitDirection.z) * force, hitPoint, ForceMode.VelocityChange);
        }
    }
    IEnumerator DelayedRagdoll(float delay)
    {
        yield return new WaitForSeconds(delay);
        ActivateRagdoll();
    }
}
 