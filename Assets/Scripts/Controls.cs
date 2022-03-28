using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
namespace Game
{
    public class Controls : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        private Rigidbody          m_RigidBody;
        private float              m_MovementSpeed = 5.0f;
        private float              m_JumpForce = 8.0f;
        private bool               m_IsInputEnabled;
        private Vector3            m_MovementDirection;
        private UnityEngine.Camera m_TpsCamera;

        private float m_DashTime = 0.05f;
        private float m_DashSpeed = 20.0f;

        private float m_FallMultiplier = 4.0f;
        private float m_BabyJumpMultiplier = 3.0f;
        private bool m_IsDashing = false;
        private float m_DistToGround;
        
        private void Awake()
        {
            m_DistToGround = transform.GetComponent<Collider>().bounds.extents.y;
            m_TpsCamera = UnityEngine.Camera.main;
            m_RigidBody = GetComponent<Rigidbody>();
            m_IsInputEnabled = true;
            if (SceneManager.GetActiveScene().buildIndex != 2)
            {
                m_IsInputEnabled = false;
            }
        }
        void Update()
        {
            //Return if the instance is not local. We don't want to control other people's characters.
            if (photonView.IsMine == false)
            {
                return;
            }
            if (m_IsInputEnabled)
            {
                if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
                {
                    m_RigidBody.velocity = Vector3.up * m_JumpForce;
                }
                if(Input.GetMouseButtonDown(0))
                {
                    StartCoroutine(Dash());
                }
            }

            if (m_RigidBody.velocity.y < 0)
            {
                m_RigidBody.velocity += Vector3.up * Physics.gravity.y * (m_FallMultiplier - 1) * Time.deltaTime;
            }
            else if (m_RigidBody.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
            {
                m_RigidBody.velocity += Vector3.up * Physics.gravity.y * (m_BabyJumpMultiplier - 1) * Time.deltaTime;
            }
        }
        public bool IsGrounded()
        {
            //m_RigidBody.velocity = new Vector3(0, 0, 0);
            return Physics.Raycast(transform.position, -Vector3.up, m_DistToGround + 0.1f);
        }

        private void FixedUpdate()
        {
            //Return if the instance is not local. We don't want to control other people's characters.
            if (photonView.IsMine == false)
            {
                return;
            }
            float horizontal = Input.GetAxisRaw("Horizontal");
            float vertical = Input.GetAxisRaw("Vertical");

            //Determine the direction of the movement, we put both the camera orientation and the player input into the calculation here.
            m_MovementDirection = m_TpsCamera.transform.forward * vertical + m_TpsCamera.transform.right * horizontal;
            m_MovementDirection.Normalize();


            if(m_IsInputEnabled)
            {
                if (m_MovementDirection != Vector3.zero)
                {
                    Vector3 turnRotationVector = new Vector3(m_MovementDirection.x, 0, m_MovementDirection.z);
                    TurnCharacterTowards(turnRotationVector, 700.0f);

                    if(m_IsDashing)
                        m_RigidBody.MovePosition(transform.position + m_DashSpeed * 1.7f * transform.forward * Time.deltaTime);
                    else
                        m_RigidBody.MovePosition(transform.position + m_MovementSpeed * 1.7f * transform.forward * Time.deltaTime);
                }
                else
                {
                    if(m_IsDashing)
                        m_RigidBody.MovePosition(transform.position + m_DashSpeed * 1.7f * transform.forward * Time.deltaTime);
                }
            }
        }
        void TurnCharacterTowards(Vector3 direction, float turnSpeed) 
        {
            Quaternion turnDirectionQuaternion = Quaternion.LookRotation(direction, Vector3.up);
            m_RigidBody.MoveRotation(Quaternion.RotateTowards(transform.rotation, turnDirectionQuaternion, turnSpeed * Time.deltaTime));
        }
        public void OnEvent(ExitGames.Client.Photon.EventData photonEvent)
        {
            if (photonEvent.Code == (byte)EventType.RoundStart)
            {
                m_IsInputEnabled = true;
            }
            else if(photonEvent.Code == (byte)EventType.RoundEnd)
            {
                m_IsInputEnabled = false;
            }
        }
        IEnumerator Dash()
        {
            float startTime = Time.time;

            while(Time.time < startTime + m_DashTime)
            {
                m_IsDashing = true;
                //m_RigidBody.MovePosition(transform.position + m_DashSpeed * Time.deltaTime * transform.forward);

                yield return null;
            }
            m_IsDashing = false;
        }
    }
}
