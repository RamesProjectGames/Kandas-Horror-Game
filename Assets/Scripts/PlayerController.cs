using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;

//[RequireComponent (typeof(CharacterController), typeof(Animator))]
[RequireComponent(typeof(CharacterController), typeof(AudioSource))]
public class PlayerController : MovableObjects
{
    [Header("Main Components")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private AudioSource audioSrc;
    [SerializeField] private GameObject face;
    //[SerializeField] private Animator anim;

    [Header("Ground Detection")]
    public Transform foot;
    public LayerMask groundMask, interactableMask;
    public AudioClip[] audioClip;

    [Header("Input Action")]
    private Vector3 input;
    private Vector3 up;
    private InputAction moveAction, lookAction, jumpAction, sprintAction, unlockAction;

    [Header("Numeric Values")]
    [SerializeField] private float jumpPow;
    [SerializeField] private float gravity = 9.81f, groundDist = 1f, speed = 150f, sprintMulti = 2.0f, jumpCd, maxStamina, staminaDecayRate;
    [SerializeField] private bool isExhausted;
    private float xMove, pitch, yaw, upVel, stamina, moveSpd;
    private bool isGrounded;
    private bool isSprinting;

    [Header("Player Camera Settings")]
    [SerializeField] private CinemachineCamera playerCam;
    [SerializeField] private Transform cam;
    public float lookSensitivity = 1f;
    public float smoothTime = 0.1f;
    public float minVerticalAngle = -20f, maxVerticalAngle = 20f;
    public float interactionAngle = 20f, interactionDist = 5f;
    private CinemachineBasicMultiChannelPerlin _noise;
    private CinemachineInputAxisController inputController;

    [Header("Bob Settings")]
    public float walkBobAmplitude = 0.5f;
    public float walkBobFrequency = 1.0f;
    public float idleBobAmplitude = 0.05f; // Slight "breathing" effect


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        audioSrc = GetComponent<AudioSource>();
        _noise = playerCam.GetComponent<CinemachineBasicMultiChannelPerlin>();

        inputController = playerCam.GetComponent<CinemachineInputAxisController>();
        
        Cursor.lockState = CursorLockMode.Locked;

        stamina = maxStamina;

        // Initialize input actions from InputSystem_Actions
        var inputActions = GetComponent<PlayerInput>();
        if (inputActions != null)
        {
            moveAction = inputActions.actions.FindAction("Move");
            lookAction = inputActions.actions.FindAction("Look");
            // jumpAction = inputActions.actions.FindAction("Jump");
            sprintAction = inputActions.actions.FindAction("Sprint");
            unlockAction = inputActions.actions.FindAction("Unlock");
        }
        else
        {
            Debug.LogWarning("PlayerInput component not found! Input will not work properly.");
        }

        agent = GetComponent<NavMeshAgent>();
        if (agent == null) agent = gameObject.AddComponent<NavMeshAgent>();

        // Configure NavMeshAgent for auto movement
        agent.angularSpeed = 300f;
        agent.acceleration = 8f;
        agent.stoppingDistance = 0.1f;
        agent.isStopped = true;
    }

    // Update is called once per frame
    void Update()
    {
        //Mouse Lock - Unlock when holding Alt
        {
            if (unlockAction != null && unlockAction.IsPressed())
            {
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                if(SettingManager.Instance.isPaused)
                {
                    Cursor.lockState = CursorLockMode.None;                    
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;                                    
                }
            }
        }
        if (inputController != null)
        {
            // Calculate your desired sensitivity
            float minSens = 0.1f;
            float maxSens = 50.0f;

            float sliderValue = SettingManager.Instance.settings.MouseSensitivity * lookSensitivity;
            float calculatedGain = Mathf.Lerp(minSens, maxSens, sliderValue);

            // Controllers is a list. Usually: Index 0 = Pan, Index 1 = Tilt
            foreach (var controller in inputController.Controllers)
            {
                if (controller.Name.Contains("Tilt") )
                {
                    // Multiplying by -1 flips the direction
                    controller.Input.Gain = -calculatedGain;
                }
                else
                {
                    controller.Input.Gain = calculatedGain;
                }
                controller.Driver.AccelTime = 0f;
                controller.Driver.DecelTime = 0f;
            }
        }
        inputController.enabled = Cursor.lockState == CursorLockMode.Locked;
        if(SettingManager.Instance.isPaused) return;

        //Jump
        //{
           if (isGrounded && upVel < 0f)
           {
               upVel = 0f;
               //anim.SetBool("isJumping", false);
           }
           else
           {
               upVel -= gravity * Time.deltaTime;
               jumpCd -= Time.deltaTime;
           }

        //    if (isGrounded && jumpAction != null && jumpAction.WasPerformedThisFrame() && jumpCd <= 0f)
        //    {
        //        //anim.SetBool("isJumping", true);
        //        upVel = Mathf.Sqrt(jumpPow * 2f * gravity);
        //        isGrounded = false;
        //        jumpCd = 1f;
        //    }
        //}

        //Movement
        if(agent.isStopped)
        {
            if(!SettingManager.Instance.settings.SprintToggle)
            {
                if (sprintAction != null && sprintAction.IsPressed() && !isExhausted)
                {
                    isSprinting = true;
                    stamina -= staminaDecayRate * Time.deltaTime;
                    if (stamina <= 0f)
                    {
                        isExhausted = true;
                    }
                    moveSpd = speed * sprintMulti;
                }
                else
                {
                    isSprinting = false;
                }
            }
            else
            {
                if (sprintAction != null && sprintAction.WasPressedThisFrame() && !isExhausted)
                {
                    isSprinting = true;
                    stamina -= staminaDecayRate * Time.deltaTime;
                    if (stamina <= 0f)
                    {
                        isExhausted = true;
                    }
                    moveSpd = speed * sprintMulti;
                }
                else
                {
                    isSprinting = false;
                }
            }
            if (isExhausted)
            {
                moveSpd = speed / sprintMulti;
                if (stamina >= maxStamina)
                {
                    isExhausted = false;
                }
            }
            else
            {
                moveSpd = speed;
            }
            if(!isSprinting)
            {
                stamina += staminaDecayRate / 2f * Time.deltaTime;
            }

            Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            
            // Use camera forward and right for movement relative to camera view
            Vector3 camForward = playerCam.transform.forward;
            Vector3 camRight = playerCam.transform.right;
            
            // Flatten camera forward to prevent upward/downward movement bias
            camForward.y = 0;
            camForward.Normalize();
            
            Vector3 hor = camRight * moveInput.x * moveSpd * Time.deltaTime;
            Vector3 ver = camForward * moveInput.y * moveSpd * Time.deltaTime;
            up = transform.up * upVel;

            input = hor + ver;

            // if (input == Vector3.zero)
            // {
            //     //anim.SetFloat("Speed", 0f);
            //     //anim.SetBool("isWalking", false);
            //     //anim.SetBool("isRunning", false);
            //     _noise.AmplitudeGain = Mathf.Lerp(_noise.AmplitudeGain, idleBobAmplitude, Time.deltaTime * 5f);
            //     _noise.FrequencyGain = Mathf.Lerp(_noise.FrequencyGain, 0.5f, Time.deltaTime * 5f);
            // }
            // else if (sprintAction == null || !sprintAction.IsPressed())
            // {
            //     _noise.AmplitudeGain = Mathf.Lerp(_noise.AmplitudeGain, walkBobAmplitude, Time.deltaTime * 5f);
            //     _noise.FrequencyGain = Mathf.Lerp(_noise.FrequencyGain, walkBobFrequency, Time.deltaTime * 5f);
            //     //anim.SetFloat("Speed", Mathf.Sign(Input.GetAxis("Vertical")) * input.magnitude);
            //     //anim.SetBool("isWalking", true);
            //     //anim.SetBool("isRunning", false);
            // }
            // else
            // {
            //     _noise.AmplitudeGain = Mathf.Lerp(_noise.AmplitudeGain, idleBobAmplitude, Time.deltaTime * 5f);
            //     _noise.FrequencyGain = Mathf.Lerp(_noise.FrequencyGain, 0.5f, Time.deltaTime * 5f);
            //     //anim.SetFloat("Speed", Input.GetAxis("Vertical") * input.magnitude);
            //     //anim.SetBool("isWalking", false);
            //     //anim.SetBool("isRunning", true);
            // }

            // Adjustments for cinemachine
            if (input == Vector3.zero)
            {
                // Smoothly transition to subtle breathing/idle
                _noise.AmplitudeGain = Mathf.Lerp(_noise.AmplitudeGain, idleBobAmplitude, Time.deltaTime * 5f);
                _noise.FrequencyGain = Mathf.Lerp(_noise.FrequencyGain, 0.5f, Time.deltaTime * 5f);
            }
            else
            {
                // Player is moving
                float targetAmp = isSprinting ? walkBobAmplitude * 1.5f : walkBobAmplitude;
                float targetFreq = isSprinting ? walkBobFrequency * 1.5f : walkBobFrequency;

                _noise.AmplitudeGain = Mathf.Lerp(_noise.AmplitudeGain, targetAmp, Time.deltaTime * 5f);
                _noise.FrequencyGain = Mathf.Lerp(_noise.FrequencyGain, targetFreq, Time.deltaTime * 5f);
            }
        }
        else
        {
            if (agent.remainingDistance <= agent.stoppingDistance)
            {
                Vector3 posPoint = agent.destination - transform.position;
                transform.rotation = Quaternion.LookRotation(posPoint);
                agent.isStopped = true;
            }
        }
        
        
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            // Sync body to camera's horizontal direction
            transform.rotation = Quaternion.Euler(0, playerCam.transform.eulerAngles.y, 0);
        }
        //Rotation
        // {
        //     Vector2 lookInput = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;
        //     yaw = transform.localEulerAngles.y + lookInput.x * SettingManager.Instance.settings.MouseSensitivity * lookSensitivity;

        //     pitch -= SettingManager.Instance.settings.MouseSensitivity * lookInput.y * lookSensitivity;

        //     // Clamp pitch between lookAngle
        //     pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        //     transform.localEulerAngles = new Vector3(0, yaw, 0);
        //     face.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        // }
    }

    private void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(foot.position, groundDist, groundMask);
        //Debug.Log($"Player is Grounded: {isGrounded}");
        if(controller.enabled)
        {
            controller.SimpleMove(moveSpd * Time.fixedDeltaTime * input);
            controller.Move(Time.fixedDeltaTime * up);
        }
    }

    private void LateUpdate()
    {
        // cam.localRotation = Quaternion.Euler(pitch, 0, 0);
        // transform.Rotate(Vector3.up * xMove);
    }

    private void OnFootstep(AnimationEvent animEvent)
    {
        audioSrc.clip = audioClip[animEvent.intParameter];
        audioSrc.Play();
    }
    private void OnLand(AnimationEvent animEvent)
    {
        audioSrc.clip = audioClip[Random.Range(0, audioClip.Length)];
        audioSrc.Play();
    }

    #region Agent (auto) Movement
    public override IEnumerator Teleport(Vector3 pos)
    {
        controller.enabled = false;
        yield return new WaitForEndOfFrame();
        agent.Warp(pos);
        transform.LookAt(transform.forward);
        agent.ResetPath();
        yield return new WaitForEndOfFrame();
        controller.enabled = true;
    }

    public override IEnumerator Move(Vector3 pos)
    {
        agent.SetDestination(pos);
        agent.isStopped = false;
        yield return new WaitForEndOfFrame();
    }
    #endregion
}
