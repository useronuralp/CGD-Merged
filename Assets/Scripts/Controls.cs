using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.SceneManagement;
using TMPro;
namespace Game
{
    public class Controls : MonoBehaviourPunCallbacks, IOnEventCallback
    {
        private Rigidbody          m_RigidBody;
        private float              m_MovementSpeed = 4.0f;
        private float              m_JumpForce = 6.0f;
        private bool               m_IsInputEnabled;
        private Vector3            m_MovementDirection;
        private UnityEngine.Camera m_TpsCamera;
        private float              m_TurnSpeed = 3000;
        private bool               m_IsRagdolling = false;
        private bool               m_IsEmoting = false;
        private bool               m_IsGettingUp = false;
        private bool               m_IsSpectating = false;
        private bool               m_HasGameEnded = false;
        private GameObject         m_ScorePanel;
        private bool               m_IsRolling = false;
        private bool               m_IsTargeting = false;
        private bool               m_IsStunned = false;
        private Cinemachine.CinemachineFreeLook m_CinemachineFLComponent;


        private TextMeshProUGUI    m_PowerupName;

        private int                m_JumpCount = 0;

        private Stamina            m_StaminaScript;

        private bool               DoOnce = false;
        private bool               DoOnce2 = true;
        private bool               DoOnce3 = true;

        private bool               m_HasDoubleJump = false;

        private bool               m_HasWaterBaloon = false;

        private GameObject         m_Reticle;
        private AudioSource        m_AudioSource;
        private Dictionary<string, AudioClip> m_EmoteSounds;
        private float m_DashTime = 0.3f;
        private float m_RollTime = 0.5f;
        private float m_DashSpeed = 7.0f;

        private float m_FallMultiplier = 3.0f;
        private float m_BabyJumpMultiplier = 2.0f;
        private bool m_IsDashing = false;
        private float m_DistToGround;
        private string[] m_GestureNames = { "Whatever_Gesture", "Loser", "Taunt", "Pointing_Gesture", "Laughing" };

        private Animator m_Animator;

        private void OnDestroy()
        {
            EventManager.Get().OnEnableInput -= OnEnableInput;
            EventManager.Get().OnDisableInput -= OnDisableInput;
            EventManager.Get().OnRagdolling -= OnRagdolling;
            EventManager.Get().OnNotRagdolling -= OnNotRagdolling;
            EventManager.Get().OnStartedGettingUp -= OnStartedGettingUp;
            EventManager.Get().OnStoppedGettingUp -= OnStoppedGettingUp;
            EventManager.Get().OnStartingSpectating -= OnStartingSpectating;
            EventManager.Get().OnStoppingSpectating -= OnStoppingSpectating;
            EventManager.Get().OnStopAllCoroutines -= OnStopAllCoroutines;
            EventManager.Get().OnGameEnd -= OnEndGame;
            EventManager.Get().OnGetStunned -= OnGetStunned;
            EventManager.Get().OnActivateDoubleJump -= OnActivateDoubleJump;
            EventManager.Get().OnActivateWaterBaloon -= OnActivateWaterBaloon;
            EventManager.Get().OnStunWearsOff -= OnStunWearsOff;
        }
        private void Start()
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

            m_CinemachineFLComponent = GameObject.Find("PlayerCamera").transform.GetComponent<Cinemachine.CinemachineFreeLook>();
            m_Reticle = GameObject.Find("UI").transform.Find("Canvas").Find("Reticle").gameObject;
            m_PowerupName = transform.Find("PowerupCanvas").Find("PowerupSlot").Find("PowerupName").GetComponent<TextMeshProUGUI>();
            m_EmoteSounds = new Dictionary<string, AudioClip>();
            m_EmoteSounds.Add("Gesture_Whatever", Resources.Load<AudioClip>("Audio/Gesture Sounds/Whatever"));
            m_EmoteSounds.Add("Gesture_Laughing", Resources.Load<AudioClip>("Audio/Gesture Sounds/Laughing"));
            m_EmoteSounds.Add("Gesture_Loser", Resources.Load<AudioClip>("Audio/Gesture Sounds/Loser"));
            m_EmoteSounds.Add("Gesture_Taunt", Resources.Load<AudioClip>("Audio/Gesture Sounds/Taunt"));
            m_EmoteSounds.Add("Gesture_Point", Resources.Load<AudioClip>("Audio/Gesture Sounds/Pointing"));
            m_AudioSource = GetComponent<AudioSource>();
            m_StaminaScript = GetComponent<Stamina>();
            m_ScorePanel = GameObject.Find("UI").transform.Find("Canvas").Find("ScorePanel").gameObject;
            EventManager.Get().OnEnableInput += OnEnableInput;
            EventManager.Get().OnDisableInput += OnDisableInput;
            EventManager.Get().OnRagdolling += OnRagdolling;
            EventManager.Get().OnNotRagdolling += OnNotRagdolling;
            EventManager.Get().OnStartedGettingUp += OnStartedGettingUp;
            EventManager.Get().OnStoppedGettingUp += OnStoppedGettingUp;
            EventManager.Get().OnStartingSpectating += OnStartingSpectating;
            EventManager.Get().OnStoppingSpectating += OnStoppingSpectating;
            EventManager.Get().OnStopAllCoroutines += OnStopAllCoroutines;
            EventManager.Get().OnGameEnd += OnEndGame;
            EventManager.Get().OnGetStunned += OnGetStunned;
            EventManager.Get().OnActivateDoubleJump += OnActivateDoubleJump;
            EventManager.Get().OnActivateWaterBaloon += OnActivateWaterBaloon;
            EventManager.Get().OnStunWearsOff += OnStunWearsOff;
        }
        void Update()
        {
            //Return if the instance is not local. We don't want to control other people's characters.
            if (photonView.IsMine == false)
            {
                return;
            }
            if(IsEmoting())
            {
                DoOnce = true;
                m_IsInputEnabled = false;
                m_IsEmoting = true;
            }
            else if(m_IsGettingUp)
            {
                m_IsInputEnabled = false;
            }
            else
            {
                if(DoOnce && !m_IsRagdolling && !m_IsDashing && !m_IsRolling)
                {
                    DoOnce = false;
                    if(!Cursor.visible)
                        m_IsInputEnabled = true;
                    m_IsEmoting = false;
                }
            }

            if (m_IsInputEnabled && !m_HasGameEnded && !m_IsStunned)
            {
                if (Input.GetKeyDown(KeyCode.Space) && (IsGrounded() || m_HasDoubleJump))
                {
                    if(m_JumpCount == 1)
                    {
                        m_HasDoubleJump = false; 
                        EventManager.Get().DeactivateDoubleJump();
                    }
                    m_JumpCount++;
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
                else if (Input.GetKeyDown(KeyCode.Q))
                {
                    if (GetComponent<PlayerManager>().m_IsBulldog && m_StaminaScript.m_CurrentStamina >= 50.0f)
                    {
                        StartCoroutine(Dive());
                        if (photonView.IsMine)
                            m_StaminaScript.ReduceStamina(50);
                    }
                    else
                    {
                        if (IsGrounded() && m_StaminaScript.m_CurrentStamina == 100.0f)
                        {
                            if (photonView.IsMine)
                                m_StaminaScript.ReduceStamina(100);
                            StartCoroutine(Roll());
                        }
                    }
                }
                else if(m_HasWaterBaloon && Input.GetMouseButton(1))
                {
                    ActivateTargeting();
                }
                else
                {
                    if(m_Reticle.activeInHierarchy)
                    {
                        DeactivateTargeting();    
                    }
                }
                if(Input.GetMouseButtonDown(0) && Input.GetMouseButton(1) && m_HasWaterBaloon)
                {
                    var throwDirection = m_TpsCamera.transform.forward * 10000 - m_TpsCamera.transform.position;
                    var baloon = PhotonNetwork.Instantiate("Water_balloon", new Vector3(transform.position.x, transform.position.y + 1.5f, transform.position.z), transform.rotation);
                    baloon.GetComponent<Rigidbody>().AddForce(throwDirection.normalized * 50.0f, ForceMode.Impulse);
                    Physics.IgnoreCollision(GetComponent<Collider>(), baloon.GetComponent<Collider>());
                    m_HasWaterBaloon = false;
                    EventManager.Get().DeactivateWaterballoon();
                }
            }
            if (Input.GetKey(KeyCode.Tab))
            {
                m_ScorePanel.SetActive(true);
            }
            else
            {
                m_ScorePanel.SetActive(false);
            }


            if (Input.GetKeyDown(KeyCode.LeftControl) && !m_IsDashing && !m_IsRagdolling && !m_IsEmoting && !m_IsGettingUp && !m_IsSpectating && !m_HasGameEnded && !m_IsRolling && !m_IsStunned)
            {
                EventManager.Get().ToggleCursor();
            }
           
            if (m_RigidBody.velocity.y < 0)
            {
                m_RigidBody.velocity += (m_FallMultiplier - 1) * Physics.gravity.y * Time.deltaTime * Vector3.up;
            }

            bool result = IsGrounded();
            m_Animator.SetBool("isGrounded", result);
            if(IsGrounded())
            {
                if(DoOnce3)
                {
                    DoOnce3 = false;
                    m_JumpCount = 0;
                }
            }
            else
            {
                DoOnce3 = true;
            }

            if (m_RigidBody.velocity.y < 0)
            {
                m_Animator.SetBool("isAscending", false);
            }
            else if(m_RigidBody.velocity.y > 0)
            {
                m_Animator.SetBool("isAscending", true);
            }


            if(m_IsInputEnabled && !m_IsStunned && !m_IsTargeting)
                m_Animator.SetFloat("Blend", Mathf.Max(Mathf.Abs(Input.GetAxis("Horizontal")), Mathf.Abs(Input.GetAxis("Vertical"))));
            else if(m_IsInputEnabled && !m_IsStunned && m_IsTargeting)
                m_Animator.SetFloat("Blend", 0.135f);
            else
                m_Animator.SetFloat("Blend", 0);
        }
        void ActivateTargeting()
        {
            m_CinemachineFLComponent.m_Orbits[0].m_Height = 3;
            m_CinemachineFLComponent.m_Orbits[0].m_Radius = 2;
            m_CinemachineFLComponent.m_Orbits[1].m_Height = 2;
            m_CinemachineFLComponent.m_Orbits[1].m_Radius = 2;
            m_CinemachineFLComponent.m_Orbits[2].m_Height = 0.5f;
            m_CinemachineFLComponent.m_Orbits[2].m_Radius = 2;
            m_CinemachineFLComponent.m_XAxis.m_MaxSpeed = 200;
            m_CinemachineFLComponent.m_YAxis.m_MaxSpeed = 3;
            if (DoOnce2)
            {
                DoOnce2 = false;
                m_CinemachineFLComponent.m_YAxis.Value = 0.35f;
                transform.Find("NameCanvas").Find("PlayerName").gameObject.SetActive(false);
            }
            m_MovementSpeed = 2.0f;
            m_Reticle.SetActive(true);
            m_IsTargeting = true;
        }
        void DeactivateTargeting()
        {
            DoOnce2 = true;
            m_CinemachineFLComponent.m_Orbits[0].m_Height = 5;
            m_CinemachineFLComponent.m_Orbits[0].m_Radius = 6;
            m_CinemachineFLComponent.m_Orbits[1].m_Height = 5;
            m_CinemachineFLComponent.m_Orbits[1].m_Radius = 6;
            m_CinemachineFLComponent.m_Orbits[2].m_Height = 0.5f;
            m_CinemachineFLComponent.m_Orbits[2].m_Radius = 6;
            m_CinemachineFLComponent.m_XAxis.m_MaxSpeed = 500;
            m_CinemachineFLComponent.m_YAxis.m_MaxSpeed = 6;
            m_MovementSpeed = 4.0f;
            m_Reticle.SetActive(false);
            m_IsTargeting = false;
            transform.Find("NameCanvas").Find("PlayerName").gameObject.SetActive(true);
        }
        public bool IsGrounded()
        {
            Vector3 center = new Vector3(transform.position.x, transform.position.y + m_DistToGround, transform.position.z);
            Vector3 XPlus = new Vector3(transform.position.x + 0.2f, transform.position.y + m_DistToGround, transform.position.z);
            Vector3 XMinus = new Vector3(transform.position.x - 0.2f, transform.position.y + m_DistToGround, transform.position.z); 
            Vector3 ZPlus = new Vector3(transform.position.x, transform.position.y + m_DistToGround, transform.position.z + 0.2f);
            Vector3 ZMinus = new Vector3(transform.position.x, transform.position.y + m_DistToGround, transform.position.z - 0.2f);


            LayerMask mask = ~(LayerMask.GetMask("Ragdoll") | LayerMask.GetMask("Ignore Raycast") | LayerMask.GetMask("HitBox"));

            // Last parameter here is not what I though it was. But it is working flawlessly even though I made a mistake while setting this up.
            Debug.DrawRay(center, -Vector3.up, Color.yellow, m_DistToGround + 0.3f);
            Debug.DrawRay(XPlus, -Vector3.up, Color.yellow, m_DistToGround + 0.3f);
            Debug.DrawRay(XMinus, -Vector3.up, Color.yellow, m_DistToGround + 0.3f);
            Debug.DrawRay(ZPlus, -Vector3.up, Color.yellow, m_DistToGround + 0.3f);
            Debug.DrawRay(ZMinus, -Vector3.up, Color.yellow, m_DistToGround + 0.3f);
            return Physics.Raycast(center, -Vector3.up, m_DistToGround + 0.3f, mask) |
                Physics.Raycast(XPlus, -Vector3.up, m_DistToGround + 0.3f, mask) |
                Physics.Raycast(XMinus, -Vector3.up, m_DistToGround + 0.3f, mask) |
                Physics.Raycast(ZPlus, -Vector3.up, m_DistToGround + 0.3f, mask) |
                Physics.Raycast(ZMinus, -Vector3.up, m_DistToGround + 0.3f, mask);
        }
        public float DistanceToGround()
        {
            Ray ray = new Ray(new Vector3(transform.position.x, transform.position.y + m_DistToGround, transform.position.z), -Vector3.up);
            Physics.Raycast(ray, out RaycastHit hitInfo);
            return hitInfo.distance;
        }
        [PunRPC]
        public void PlayGesture(string gestureName)
        {
            m_IsEmoting = true;
            m_Animator.SetTrigger(gestureName);
            m_AudioSource.PlayOneShot(m_EmoteSounds[gestureName]);
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

            if(m_IsInputEnabled && !m_IsStunned && !m_IsTargeting)
            {
                if (m_MovementDirection != Vector3.zero)
                {
                    Vector3 turnRotationVector = new Vector3(m_MovementDirection.x, 0, m_MovementDirection.z);
                    TurnCharacterTowards(turnRotationVector, m_TurnSpeed);
                    m_RigidBody.MovePosition(transform.position + 1.7f * m_MovementSpeed * Time.deltaTime * transform.forward);
                }
            }
            else if(m_IsInputEnabled && !m_IsStunned && m_IsTargeting)
            {
                Vector3 turnRotationVector = new Vector3(m_TpsCamera.transform.forward.x, 0, m_TpsCamera.transform.forward.z);
                TurnCharacterTowards(turnRotationVector, m_TurnSpeed);
                Vector3 movementDir = m_TpsCamera.transform.forward * vertical + m_TpsCamera.transform.right * horizontal;
                m_RigidBody.MovePosition(transform.position + movementDir * m_MovementSpeed * Time.deltaTime);
            }

            if(m_IsDashing)
            {
                m_RigidBody.MovePosition(transform.position + m_DashSpeed * 1.7f * transform.forward * Time.deltaTime);
            }
            else if(m_IsRolling)
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
            while (Time.time < startTime + m_DashTime)
            {
                m_IsDashing = true;
                yield return null;
            }
            m_Animator.SetBool("Dive", false);
            m_IsDashing = false;
            if (GameObject.Find("PlayerCamera").GetComponent<Cinemachine.CinemachineFreeLook>().m_XAxis.m_InputAxisName != "" && GameObject.Find("PlayerCamera").GetComponent<Cinemachine.CinemachineFreeLook>().m_YAxis.m_InputAxisName != "")
            {
                if (!m_IsRagdolling)
                    OnEnableInput();
            }
        }
        IEnumerator Roll()
        {
            float startTime = Time.time;
            m_IsInputEnabled = false;
            m_Animator.SetBool("Roll", true);
            while (Time.time < startTime + m_RollTime)
            {
                m_IsRolling = true;
                yield return null;
            }
            m_Animator.SetBool("Roll", false);
            m_IsRolling = false;
            if (GameObject.Find("PlayerCamera").GetComponent<Cinemachine.CinemachineFreeLook>().m_XAxis.m_InputAxisName != "" && GameObject.Find("PlayerCamera").GetComponent<Cinemachine.CinemachineFreeLook>().m_YAxis.m_InputAxisName != "")
            {
                if (!m_IsRagdolling)
                    OnEnableInput();
            }
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
                        m_Animator.SetBool("Roll", false);
                        m_IsDashing = false;
                        m_IsRolling = false;
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
        public void OnRagdolling()
        {
            m_IsRagdolling = true;
        }
        public void OnNotRagdolling()
        {
            m_IsRagdolling = false;
        }
        bool IsEmoting()
        {
            foreach(string name in m_GestureNames)
            {
                if (m_Animator.GetCurrentAnimatorStateInfo(0).IsName(name) && m_Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1.0f)
                    return true;
            }
            return false;
        }
        void OnStartedGettingUp()
        {

            m_IsGettingUp = true;
            m_IsInputEnabled = false;
        }
        void OnStoppedGettingUp()
        {
            m_IsGettingUp = false;
        }
        void OnStartingSpectating()
        {
            if(photonView.IsMine)
                m_IsSpectating = true;
        }
        void OnStoppingSpectating()
        {
            if (photonView.IsMine)
                m_IsSpectating = false;
        }
        void OnStopAllCoroutines()
        {
            StopAllCoroutines();
            m_Animator.SetBool("Dive", false);
            m_Animator.SetBool("Roll", false);
            m_IsDashing = false;
            m_IsRolling = false;
        }
        void OnEndGame()
        {
            if(photonView.IsMine)
            {
                m_HasGameEnded = true;
                m_IsInputEnabled = false;
            }
        }
        void ResetPowerups()
        {
            m_JumpCount = 0;
            EventManager.Get().DeactivateDoubleJump();
            EventManager.Get().DeactivateWaterballoon();
            m_HasDoubleJump = false;
            m_HasWaterBaloon = false;
        }
        void OnActivateDoubleJump()
        {
            ResetPowerups();
            m_HasDoubleJump = true;
        }
        void OnActivateWaterBaloon()
        {
            //Position of these are currently synced with a transform view, which is not ideal. Try to sync it with and RPC call and simulate the ball localy on every machine.
            ResetPowerups();
            m_HasWaterBaloon = true;
        }
        void OnGetStunned()
        {
            if(photonView.IsMine && !m_IsRagdolling)
            {
                DeactivateTargeting();
                m_IsStunned = true;
            }
        }
        void OnStunWearsOff()
        {
            if(photonView.IsMine)
                m_IsStunned = false;
        }
    }
}
