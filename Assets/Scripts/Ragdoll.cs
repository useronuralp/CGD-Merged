using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ragdoll : MonoBehaviour
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
        DeactivateRagdoll();
        m_GetUpTimer = m_GetUpCooldown;
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
            m_Rigidbody.position = new Vector3(m_Hips.transform.position.x, m_Hips.transform.position.y - 0.5f, m_Hips.transform.position.z);
            if(m_GetUpTimer <= 0.0)
            {
                m_GetUpTimer = m_GetUpCooldown;
                DeactivateRagdoll();
            }
        }
        //Debug.Log("Is ragdoll: " + m_IsInRagdollState);
        if(Input.GetKeyDown(KeyCode.E))
        {
            if(m_IsInRagdollState)
                DeactivateRagdoll();
            else
                ActivateRagdoll();
        }
    }
    private void ActivateRagdoll()
    {
        m_Colliders = gameObject.GetComponentsInChildren<Collider>();
        foreach (var collider in m_Colliders)
        {
            if (collider.gameObject != gameObject)
                collider.isTrigger = false;
        }
        EventManager.Get().DisableInput();
        m_Animator.enabled = false;
        m_IsInRagdollState = true;
        m_Rigidbody.useGravity = false;
        GetComponent<Collider>().isTrigger = true;
        m_Rigidbody.velocity = Vector3.zero;
        ResetRagdollVelocity();
    }
    private void DeactivateRagdoll()
    {
        m_Colliders = gameObject.GetComponentsInChildren<Collider>();
        foreach (var collider in m_Colliders)
        {
            if (collider.gameObject != gameObject)
                collider.isTrigger = true;
        }
        EventManager.Get().EnableInput();
        m_Animator.enabled = true;
        m_IsInRagdollState = false;
        m_Rigidbody.useGravity = true;
        GetComponent<Collider>().isTrigger = false;
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
    public void Bounce(Vector3 hitDirection, Vector3 hitPoint)
    {
        ActivateRagdoll();
        m_Hips.GetComponent<Rigidbody>().AddForceAtPosition(new Vector3(-hitDirection.x, 1, -hitDirection.z) * 30.0f, hitPoint, ForceMode.VelocityChange);
    }
}
 