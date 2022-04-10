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
        private float              m_TurnSpeed = 3000;
        private bool               m_IsRagdolling = false;
        private bool               m_IsEmoting = false;
        private bool               DoOnce = false;

        private float m_DashTime = 0.3f;
        private float m_DashSpeed = 7.0f;

        private float m_FallMultiplier = 2.0f;
        private float m_BabyJumpMultiplier = 2.0f;
        private bool m_IsDashing = false;
        private float m_DistToGround;
        private string[] m_GestureNames = { "Whatever_Gesture", "Loser", "Taunt", "Pointing_Gesture", "Laughing" };

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
            EventManager.Get().OnRagdolling += OnRagdolling;
            EventManager.Get().OnNotRagdolling += OnNotRagdolling;
        }
        void Update()
        {
            //Return if the instance is not local. We don't want to control other people's characters.
            if (photonView.IsMine == false)
            {
                return;
            }
            if(isEmoting())
            {
                DoOnce = true;
                m_IsInputEnabled = false;
                m_IsEmoting = true;
            }
            else
            {
                if(DoOnce && !m_IsRagdolling && !m_IsDashing)
                {
                    DoOnce = false;
                    m_IsInputEnabled = true;
                    m_IsEmoting = false;
                }
            }

            if (m_IsInputEnabled)
            {
                if (Input.GetKeyDown(KeyCode.Space) && IsGrounded())
                {
                    m_RigidBody.velocity = Vector3.up * m_JumpForce;
                }
                else if(Input.GetKeyDown(KeyCode.Alpha1) && IsGrounded())
                {
                    photonView.RPC("PlayGesture", RpcTarget.All, "Gesture_Whatever");
                }
                else if (Input.GetKeyDown(KeyCode.Alpha2) && IsGrounded())
                {
                    photonView.RPC("PlayGesture", RpcTarget.All, "Gesture_Point");
                }
                else if (Input.GetKeyDown(KeyCode.Alpha3) && IsGrounded())
                {
                    photonView.RPC("PlayGesture", RpcTarget.All, "Gesture_Taunt");
                }
                else if (Input.GetKeyDown(KeyCode.Alpha4) && IsGrounded())
                {
                    photonView.RPC("PlayGesture", RpcTarget.All, "Gesture_Laughing");
                }
                else if (Input.GetKeyDown(KeyCode.Alpha5) && IsGrounded())
                {
                    photonView.RPC("PlayGesture", RpcTarget.All, "Gesture_Loser");
                }
                if (Input.GetMouseButtonDown(0))
                {
                    if(photonView.IsMine)
                        GetComponent<Stamina>().ReduceStamina(10);
                    StartCoroutine(Dive());
                }
            }
            if (Input.GetKeyDown(KeyCode.LeftControl) && !m_IsDashing && !m_IsRagdolling && !m_IsEmoting)
            {
                EventManager.Get().ToggleCursor();
            }


            if (m_RigidBody.velocity.y < 0)
            {
                m_RigidBody.velocity += (m_FallMultiplier - 1) * Physics.gravity.y * Time.deltaTime * Vector3.up;
            }
            else if (m_RigidBody.velocity.y > 0 && !Input.GetKey(KeyCode.Space))
            {
                m_RigidBody.velocity += (m_BabyJumpMultiplier - 1) * Physics.gravity.y * Time.deltaTime * Vector3.up;
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
            Debug.DrawRay(new Vector3(transform.position.x, transform.position.y + m_DistToGround, transform.position.z), -Vector3.up, Color.yellow, m_DistToGround + 0.3f);
            return Physics.Raycast(new Vector3(transform.position.x, transform.position.y + m_DistToGround, transform.position.z), -Vector3.up, m_DistToGround + 0.3f, ~LayerMask.GetMask("Ragdoll"));
        }

        [PunRPC]
        public void PlayGesture(string gestureName)
        {
            m_IsEmoting = true;
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
                    m_RigidBody.MovePosition(transform.position + 1.7f * m_MovementSpeed * Time.deltaTime * transform.forward);
                }
            }
            if(m_IsDashing)
            {
                m_RigidBody.MovePosition(transform.position + m_DashSpeed * 1.7f * transform.forward * Time.deltaTime);
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
                StopAllCoroutines(); // TODO: Might cause bugs, did not test this.
                m_IsInputEnabled = false;
            }
        }
        IEnumerator Dive()
        {
            float startTime = Time.time;
            m_IsInputEnabled = false;
            m_Animator.SetBool("Dive", true);
            m_RigidBody.velocity += new Vector3(0, 4, 0);
            while (Time.time < startTime + m_DashTime)
            {
                m_IsDashing = true;
                yield return null;
            }
            m_Animator.SetBool("Dive", false);
            m_IsDashing = false;
            StartCoroutine(EnableInputDelayed(0.2f));
        }
        private void OnDisableInput(SenderType type)
        {
            if (photonView.IsMine)
            {
                switch(type)
                {
                    case SenderType.Standard:
                        m_IsInputEnabled = false;
                        break;

                    case SenderType.HitByObstacle:
                        m_Animator.SetBool("Dive", false);
                        m_IsDashing = false;
                        StopAllCoroutines(); // Dive(), EnableInputDelayed(float delay);
                        m_IsInputEnabled = false;
                        break;
                }
            }
        }
        private void OnEnableInput()
        {
            if(photonView.IsMine)
                m_IsInputEnabled = true;
        }
        IEnumerator EnableInputDelayed(float delay)
        {
            yield return new WaitForSeconds(delay);
            if(GameObject.Find("PlayerCamera").GetComponent<Cinemachine.CinemachineFreeLook>().m_XAxis.m_InputAxisName != "" && GameObject.Find("PlayerCamera").GetComponent<Cinemachine.CinemachineFreeLook>().m_YAxis.m_InputAxisName != "")
                OnEnableInput();
        }
        public void OnRagdolling()
        {
            m_IsRagdolling = true;
        }
        public void OnNotRagdolling()
        {
            m_IsRagdolling = false;
        }
        bool isEmoting()
        {
            foreach(string name in m_GestureNames)
            {
                if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName(name) && m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
                    return true;
            }
            return false;
        }
    }
}
