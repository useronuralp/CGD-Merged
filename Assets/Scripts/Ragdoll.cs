using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
namespace Game
{
    public class Ragdoll : MonoBehaviourPunCallbacks
    {
        private Animator m_Animator;
        public bool m_IsInRagdollState = false;
        private Rigidbody m_Rigidbody;

        private GameObject m_Hips;
        private Rigidbody m_HipsRigidBody;
        private PhotonTransformView m_HipsTransformView;

        private float m_DistToGround;

        private float m_GetUpTimer;
        private float m_GetUpCooldown = 1.0f;
        private List<Collider> m_RagdollColliders;

        private AudioSource m_AudioSource;
        private List<AudioClip> m_GetHitSounds;
        private List<AudioClip> m_RagdollSounds;

        private float m_RagdollSoundTimer = 0.5f;
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
            m_DistToGround = transform.GetComponent<Collider>().bounds.extents.y;
            m_GetHitSounds = new List<AudioClip>();
            m_RagdollSounds = new List<AudioClip>();
            m_GetHitSounds.Add(Resources.Load<AudioClip>("Audio/Collision Sounds/Ah"));
            m_GetHitSounds.Add(Resources.Load<AudioClip>("Audio/Collision Sounds/B"));
            m_GetHitSounds.Add(Resources.Load<AudioClip>("Audio/Collision Sounds/bip"));
            m_GetHitSounds.Add(Resources.Load<AudioClip>("Audio/Collision Sounds/oomph"));
            m_GetHitSounds.Add(Resources.Load<AudioClip>("Audio/Collision Sounds/ou"));
            m_GetHitSounds.Add(Resources.Load<AudioClip>("Audio/Collision Sounds/ur"));

            m_RagdollSounds.Add(Resources.Load<AudioClip>("Audio/Ragdolling/Falling"));
            m_RagdollSounds.Add(Resources.Load<AudioClip>("Audio/Ragdolling/Falling 2"));
            m_RagdollSounds.Add(Resources.Load<AudioClip>("Audio/Ragdolling/Falling 3"));
            m_RagdollSounds.Add(Resources.Load<AudioClip>("Audio/Ragdolling/Falling 4"));
            m_AudioSource = GetComponent<AudioSource>();
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
            if (m_IsInRagdollState && photonView.IsMine)
            {
                m_RagdollSoundTimer -= Time.deltaTime;
                if (m_RagdollSoundTimer <= 0.0f && !IsGrounded())
                {
                    m_RagdollSoundTimer = 0.5f;
                    photonView.RPC("PlayRagdollSound", RpcTarget.All, Random.Range(0, 4));
                }
                if (m_Rigidbody.velocity.magnitude <= 1)
                {
                    m_GetUpTimer -= Time.deltaTime;
                    if (m_GetUpTimer <= 0.0)
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
            if (m_IsInRagdollState)
                m_HipsRigidBody.MovePosition(m_Rigidbody.transform.position);
        }
        [PunRPC]
        private void ActivateRagdoll(Game.SenderType type)
        {
            if (!m_IsInRagdollState)
            {
                if (photonView.IsMine)
                {
                    EventManager.Get().ChangeEyes("Dead");
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
            if (photonView.IsMine)
            {
                if(GetComponent<PlayerManager>().m_IsBulldog)
                {
                    EventManager.Get().ChangeEyes("Angry");
                }
                else
                {
                    EventManager.Get().ChangeEyes("Default");
                }
                EventManager.Get().StopRagdolling();
            }
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
            m_RagdollSoundTimer = 0.5f;
        }
        public bool IsGrounded()
        {
            Vector3 center = new Vector3(transform.position.x, transform.position.y + m_DistToGround, transform.position.z);
            Vector3 XPlus = new Vector3(transform.position.x + 0.2f, transform.position.y + m_DistToGround, transform.position.z);
            Vector3 XMinus = new Vector3(transform.position.x - 0.2f, transform.position.y + m_DistToGround, transform.position.z);
            Vector3 ZPlus = new Vector3(transform.position.x, transform.position.y + m_DistToGround, transform.position.z + 0.2f);
            Vector3 ZMinus = new Vector3(transform.position.x, transform.position.y + m_DistToGround, transform.position.z - 0.2f);

            LayerMask mask = ~(LayerMask.GetMask("Ragdoll") | LayerMask.GetMask("Ignore Raycast") | LayerMask.GetMask("HitBox"));

            return Physics.Raycast(center, -Vector3.up, m_DistToGround + 0.3f, mask) |
                Physics.Raycast(XPlus, -Vector3.up, m_DistToGround + 0.3f, mask) |
                Physics.Raycast(XMinus, -Vector3.up, m_DistToGround + 0.3f, mask) |
                Physics.Raycast(ZPlus, -Vector3.up, m_DistToGround + 0.3f, mask) |
                Physics.Raycast(ZMinus, -Vector3.up, m_DistToGround + 0.3f, mask);
        }
        public void EnableInput() //To be used by animation events.
        {
            if (photonView.IsMine)
            {
                EventManager.Get().StoppedGettingUp();
                if (!Cursor.visible) // If the cursor is on screen player is either typing in chat or trying to navigate the menu so don't enable the input if that is the case.
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
            if (!m_IsInRagdollState && photonView.IsMine)
            {
                photonView.RPC("PlayGetHitSound", RpcTarget.All, Random.Range(0, 6));
                photonView.RPC("ActivateRagdoll", RpcTarget.All, Game.SenderType.HitByObstacle);
                m_Rigidbody.AddForceAtPosition(new Vector3(-hitDirection.x, 2, -hitDirection.z) * force, hitPoint, ForceMode.VelocityChange);
            }
        }
        [PunRPC]
        void PlayGetHitSound(int clipIndex)
        {
            m_AudioSource.Stop();
            m_AudioSource.PlayOneShot(m_GetHitSounds[clipIndex]);
            m_RagdollSoundTimer = 0.5f;
        }
        [PunRPC]
        void PlayRagdollSound(int clipIndex)
        {
            m_AudioSource.Stop();
            m_AudioSource.PlayOneShot(m_RagdollSounds[clipIndex]);
        }
        private void OnCollisionEnter(Collision collision)
        {
            if (!m_IsInRagdollState && photonView.IsMine && Game.PlaygroundManager.s_HasRoundStarted)
            {
                if (collision.transform.CompareTag("Bulldog") || collision.transform.CompareTag("Runner") || collision.transform.CompareTag("Player"))
                {
                    if (!GetComponent<Game.Controls>().IsGrounded() && GetComponent<Game.Controls>().DistanceToGround() >= 2.2f)
                    {
                        photonView.RPC("ActivateRagdoll", RpcTarget.All, Game.SenderType.Standard);
                        collision.transform.GetComponent<Ragdoll>().photonView.RPC("ActivateRagdoll", RpcTarget.All, Game.SenderType.Standard);
                    }
                }
            }
            else if (m_IsInRagdollState)
            {
                photonView.RPC("PlayGetHitSound", RpcTarget.All, Random.Range(0, 6));
            }
        }
    }
}
 