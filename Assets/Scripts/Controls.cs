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
        private float              m_MovementSpeed = 4.0f;
        private float              m_JumpForce = 7.0f;
        private bool               m_IsInputEnabled;
        private Vector3            m_MovementDirection;
        private UnityEngine.Camera m_TpsCamera;
        private float              m_TurnSpeed = 2000;

        private float m_DashTime = 0.05f;
        private float m_DashSpeed = 20.0f;

        private float m_FallMultiplier = 2.0f;
        private float m_BabyJumpMultiplier = 2.0f;
        private bool m_IsDashing = false;
        private float m_DistToGround;

        private Animator m_Animator;
        
        private void Awake()
        {
            m_Animator = GetComponent<Animator>();
            m_DistToGround = transform.GetComponent<Collider>().bounds.extents.y;
            m_TpsCamera = UnityEngine.Camera.main;
            m_RigidBody = GetComponent<Rigidbody>();
            m_IsInputEnabled = true;
            if (SceneManager.GetActiveScene().buildIndex != 2)
            {
                m_IsInputEnabled = false;
            }
        }
        private void Start()
        {
            EventManager.Get().OnEnableInput += OnEnableInput;
            EventManager.Get().OnDisableInput += OnDisableInput;
        }
        void Update()
        {
            //Debug.Log(IsGrounded());
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
                else if(Input.GetKeyDown(KeyCode.Alpha1))
                {
                    photonView.RPC("PlayGesture", RpcTarget.All, "Gesture_Whatever");
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2))
                {
                    photonView.RPC("PlayGesture", RpcTarget.All, "Gesture_Point");
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3))
                {
                    photonView.RPC("PlayGesture", RpcTarget.All, "Gesture_Taunt");
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4))
                {
                    photonView.RPC("PlayGesture", RpcTarget.All, "Gesture_Laughing");
                }
                else if (Input.GetKeyDown(KeyCode.Alpha5))
                {
                    photonView.RPC("PlayGesture", RpcTarget.All, "Gesture_Loser");
                }
                if (Input.GetMouseButtonDown(0))
                {
                    if(photonView.IsMine)
                    {
                        EventManager.Get().ReduceStamina(10);
                    }
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

            m_Animator.SetBool("isGrounded", IsGrounded());
            if(m_RigidBody.velocity.y < 0)
            {
                m_Animator.SetBool("isAscending", false);
            }
            else if(m_RigidBody.velocity.y > 0)
            {
                m_Animator.SetBool("isAscending", true);
            }


            if(m_IsInputEnabled)
                m_Animator.SetFloat("Blend", Mathf.Max(Mathf.Abs(Input.GetAxis("Horizontal")), Mathf.Abs(Input.GetAxis("Vertical"))));
            else
                m_Animator.SetFloat("Blend", 0);
        }
        public bool IsGrounded()
        {
            //m_RigidBody.velocity = new Vector3(0, 0, 0);
            Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + m_DistToGround, transform.position.z), -Vector3.up, Color.yellow, m_DistToGround + 0.3f);
            return Physics.Raycast(new Vector3(transform.position.x, transform.position.y + m_DistToGround, transform.position.z), -Vector3.up, m_DistToGround + 0.3f, ~LayerMask.GetMask("Ragdoll"));
        }

        [PunRPC]
        public void PlayGesture(string gestureName)
        {
            m_Animator.SetTrigger(gestureName);
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
                    TurnCharacterTowards(turnRotationVector, m_TurnSpeed);

                    if (m_IsDashing)
                    {
                        m_RigidBody.MovePosition(transform.position + m_DashSpeed * 1.7f * transform.forward * Time.deltaTime);
                    }
                    else
                    {
                        m_RigidBody.MovePosition(transform.position + m_MovementSpeed * 1.7f * transform.forward * Time.deltaTime);
                    }
                }
                else
                {
                    if(m_IsDashing)
                    {
                        m_RigidBody.MovePosition(transform.position + m_DashSpeed * 1.7f * transform.forward * Time.deltaTime);
                    }
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
        private void OnDisableInput()
        {
            if(photonView.IsMine)
                m_IsInputEnabled = false;
        }
        private void OnEnableInput()
        {
            if(photonView.IsMine)
                m_IsInputEnabled = true;
        }
    }
}
