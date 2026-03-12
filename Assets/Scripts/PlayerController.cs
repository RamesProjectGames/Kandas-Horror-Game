using Dialogue;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using UnityEngine.UI;

//[RequireComponent (typeof(CharacterController), typeof(Animator))]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MovableObjects
{
    [Header("Main Components")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private AudioSource audioSrc;
    [SerializeField] private GameObject face;
    //[SerializeField] private Animator anim;

    [Header("Ground Detection")]
    public Transform foot;
    public LayerMask groundMask;
    public AudioClip[] audioClip;

    [Header("Input Action")]
    private Vector3 input;
    private Vector3 up;
    private InputAction moveAction, lookAction, jumpAction, sprintAction, crouchAction, unlockAction;

    [Header("Numeric Values")]
    [SerializeField] private float jumpPow;
    [SerializeField] private float gravity = 9.81f, groundDist = 1f, speed = 150f, sprintMulti = 2.0f, jumpCd, maxStamina, staminaDecayRate;
    [SerializeField] private bool isExhausted;
    private float xMove, pitch, yaw, upVel, stamina, moveSpd;
    private bool isGrounded;
    private bool isSprinting;
    private bool isCrouching;

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

    [Header("Footsteps")]
    // reference to the centralized sound manager – typically on the same GameObject
    public FootstepsSoundManager footstepManager;
    // base step distance at normal walking speed; adjusted dynamically based on moveSpd
    public float baseStepDistance = 2f;
    private Vector3 _lastFootstepPosition;
    private float _footstepDistanceAccum;

    [Header("Hiding")]
    public PlayerHiding Hiding;

    [Header("Stamina")]
    public Image staminaFillImage;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        Hiding = GetComponent<PlayerHiding>();
        audioSrc = GetComponent<AudioSource>();
        _noise = playerCam.GetComponent<CinemachineBasicMultiChannelPerlin>();

        inputController = playerCam.GetComponent<CinemachineInputAxisController>();
        
        Cursor.lockState = CursorLockMode.Locked;

        stamina = maxStamina;
        staminaFillImage.gameObject.SetActive(false);

        // Initialize input actions from InputSystem_Actions
        var inputActions = GetComponent<PlayerInput>();
        if (inputActions != null)
        {
            moveAction = inputActions.actions.FindAction("Move");
            lookAction = inputActions.actions.FindAction("Look");
            // jumpAction = inputActions.actions.FindAction("Jump");
            sprintAction = inputActions.actions.FindAction("Sprint");
            unlockAction = inputActions.actions.FindAction("Unlock");
            crouchAction = inputActions.actions.FindAction("Crouch");
        }
        else
        {
            Debug.LogWarning("PlayerInput component not found! Input will not work properly.");
        }

        agent = GetComponent<NavMeshAgent>();

        // footsteps helper
        if (footstepManager == null)
            footstepManager = GetComponent<FootstepsSoundManager>();
        _lastFootstepPosition = transform.position;
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
                Cursor.visible = true;
            }
            else
            {
                if(SettingManager.Instance.isPaused)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                }
            }
        }
        if (SettingManager.Instance.isPaused || DialogueSystem.Instance.isRunningConvo)
        {
            if(SettingManager.Instance.isPaused)
                lookAction.Disable();
            return;
        }
        else if(!lookAction.enabled)
        {
            lookAction.Enable();
        }
        if (inputController != null)
        {

            float sliderValue = SettingManager.Instance.settings.MouseSensitivity ;
            float calculatedGain = Mathf.Lerp(SettingManager.Instance.minimumMouseSensitivity, SettingManager.Instance.maximumMouseSensitivity, sliderValue) * lookSensitivity;

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

        //Movement - skip input if player is hiding
        if (Hiding != null && Hiding.IsHiding())
        {
            // when hidden we don't process movement input or physics
        }
        else if(agent.isStopped)
        {
            if(!SettingManager.Instance.settings.SprintToggle)
            {
                if (sprintAction != null && sprintAction.IsPressed() && !isExhausted)
                {
                    isSprinting = true;
                    if (stamina <= 0f)
                    {
                        stamina= 0;
                        isExhausted = true;
                    }
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
                    if (stamina <= 0f)
                    {
                        stamina= 0;
                        isExhausted = true;
                    }
                }
                else
                {
                    isSprinting = false;
                }
            }
            if(!SettingManager.Instance.settings.CrouchToggle)
            {
                if (crouchAction != null && crouchAction.IsPressed())
                {
                    isCrouching = true;
                }
                else
                {
                    isCrouching = false;
                }
            }
            else
            {
                if (crouchAction != null && crouchAction.WasPressedThisFrame())
                {
                    isCrouching = true;
                }
                else
                {
                    isCrouching = false;
                }
            }
            if (isExhausted)
            {
                moveSpd = speed / (sprintMulti * sprintMulti);
                if (stamina >= maxStamina)
                {
                    stamina = maxStamina;
                    isExhausted = false;
                }
            }
            else
            {
                if (isSprinting)
                {
                    moveSpd = speed * (!isCrouching ? sprintMulti : 1f);
                }
                else if(isCrouching)
                {
                    moveSpd = speed / (sprintMulti * sprintMulti);
                }
                else
                {
                    moveSpd = speed;
                }
            }
            

            if(!Hiding.IsHiding())
            {
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
            }
            if(!isSprinting)
            {
                stamina += staminaDecayRate * Time.deltaTime * (isCrouching ? sprintMulti : 1f);
            }
            if(stamina < maxStamina)
            {
                staminaFillImage.gameObject.SetActive(true);
                staminaFillImage.fillAmount = stamina / maxStamina;
            }
            else
            {
                stamina = maxStamina;
                staminaFillImage.gameObject.SetActive(false);
            }
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
                // _noise.AmplitudeGain = Mathf.Lerp(_noise.AmplitudeGain, idleBobAmplitude, Time.deltaTime * 5f);
                // _noise.FrequencyGain = Mathf.Lerp(_noise.FrequencyGain, 0.5f, Time.deltaTime * 5f);
            }
            else
            {
                // Player is moving
                float targetAmp = isSprinting ? walkBobAmplitude * 1.5f : walkBobAmplitude;
                float targetFreq = isSprinting ? walkBobFrequency * 1.5f : walkBobFrequency;
                if(isSprinting)
                {
                    stamina -= staminaDecayRate * Time.deltaTime;
                    if(stamina <= 0)
                    {
                        stamina = 0;
                    }
                } 
                // _noise.AmplitudeGain = Mathf.Lerp(_noise.AmplitudeGain, targetAmp, Time.deltaTime * 5f);
                // _noise.FrequencyGain = Mathf.Lerp(_noise.FrequencyGain, targetFreq, Time.deltaTime * 5f);
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
        
        
        // Player body rotation is handled by Cinemachine camera - do not force rotation here
        // Uncomment only if you need manual body rotation separate from camera
        // if (Cursor.lockState == CursorLockMode.Locked)
        // {
        //     transform.rotation = Quaternion.Euler(0, playerCam.transform.eulerAngles.y, 0);
        // }
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

        // accumulate distance travelled this frame and trigger a step when we've covered enough ground
        if (footstepManager != null && isGrounded && input.magnitude > 0.01f)
        {
            // scale step distance inversely with speed: faster movement = more frequent steps
            // use speed (normal walk speed, ~150) as baseline
            float effectiveStepDistance = moveSpd > 0.1f 
                ? baseStepDistance * (speed / moveSpd) 
                : baseStepDistance;
            
            float dist = Vector3.Distance(transform.position, _lastFootstepPosition);
            _footstepDistanceAccum += dist;
            if (_footstepDistanceAccum >= effectiveStepDistance)
            {
                _footstepDistanceAccum -= effectiveStepDistance;
                footstepManager.PlayFootstep();
            }
        }
        else if(footstepManager != null)
        {
            footstepManager.StopFootstep();
        }
        _lastFootstepPosition = transform.position;
    }

    private void LateUpdate()
    {
        // cam.localRotation = Quaternion.Euler(pitch, 0, 0);
        // transform.Rotate(Vector3.up * xMove);
        if (controller.enabled)
        {
            controller.SimpleMove(moveSpd * Time.fixedDeltaTime * input);
            controller.Move(Time.fixedDeltaTime * up);
        }
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

    public override IEnumerator Move(Vector3 pos, float speed = 150f)
    {
        agent.SetDestination(pos);
        agent.isStopped = false;
        yield return new WaitForEndOfFrame();
    }
    #endregion
}
