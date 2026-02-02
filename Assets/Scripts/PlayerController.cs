using UnityEngine;
using UnityEngine.InputSystem;

//[RequireComponent (typeof(CharacterController), typeof(Animator))]
[RequireComponent(typeof(CharacterController), typeof(AudioSource))]
public class PlayerController : MonoBehaviour
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

    [Header("Player Camera Settings")]
    [SerializeField] private Camera playerCam;
    [SerializeField] private Transform cam;
    public float mouseSensitivity = .2f;
    public float smoothTime = 0.1f;
    public float minVerticalAngle = -20f, maxVerticalAngle = 20f;
    public float interactionAngle = 20f, interactionDist = 5f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        audioSrc = GetComponent<AudioSource>();

        stamina = maxStamina;

        // Initialize input actions from InputSystem_Actions
        var inputActions = GetComponent<PlayerInput>();
        if (inputActions != null)
        {
            moveAction = inputActions.actions.FindAction("Move");
            lookAction = inputActions.actions.FindAction("Look");
            jumpAction = inputActions.actions.FindAction("Jump");
            sprintAction = inputActions.actions.FindAction("Sprint");
            unlockAction = inputActions.actions.FindAction("Unlock");
        }
        else
        {
            Debug.LogWarning("PlayerInput component not found! Input will not work properly.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        //Mouse Lock - Unlock when holding Alt
        {
            if (unlockAction != null && unlockAction.IsPressed())
                Cursor.lockState = CursorLockMode.None;
            else
                Cursor.lockState = CursorLockMode.Locked;
        }

        //Jump
        {
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

            if (isGrounded && jumpAction != null && jumpAction.WasPerformedThisFrame() && jumpCd <= 0f)
            {
                //anim.SetBool("isJumping", true);
                upVel = Mathf.Sqrt(jumpPow * 2f * gravity);
                isGrounded = false;
                jumpCd = 1f;
            }
        }

        //Movement
        {
            if (sprintAction != null && sprintAction.IsPressed() && !isExhausted)
            {
                stamina -= staminaDecayRate * Time.deltaTime;
                if (stamina <= 0f)
                {
                    isExhausted = true;
                }
                moveSpd = speed * sprintMulti;
            }
            else if (isExhausted)
            {
                moveSpd = speed / sprintMulti;
                if (stamina >= maxStamina)
                {
                    isExhausted = false;
                }
                else
                {
                    stamina += (staminaDecayRate / 2f) * Time.deltaTime;
                }
            }
            else
            {
                moveSpd = speed;
            }

            Vector2 moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;
            Vector3 hor = transform.right * moveInput.x * moveSpd * Time.deltaTime;
            Vector3 ver = transform.forward * moveInput.y * moveSpd * Time.deltaTime;
            up = transform.up * upVel;

            input = hor + ver;

            if (input == Vector3.zero)
            {
                //anim.SetFloat("Speed", 0f);
                //anim.SetBool("isWalking", false);
                //anim.SetBool("isRunning", false);
            }
            else if (sprintAction == null || !sprintAction.IsPressed())
            {
                //anim.SetFloat("Speed", Mathf.Sign(Input.GetAxis("Vertical")) * input.magnitude);
                //anim.SetBool("isWalking", true);
                //anim.SetBool("isRunning", false);
            }
            else
            {
                //anim.SetFloat("Speed", Input.GetAxis("Vertical") * input.magnitude);
                //anim.SetBool("isWalking", false);
                //anim.SetBool("isRunning", true);
            }
        }

        //Rotation
        {
            Vector2 lookInput = lookAction != null ? lookAction.ReadValue<Vector2>() : Vector2.zero;
            yaw = transform.localEulerAngles.y + lookInput.x * mouseSensitivity;

            pitch -= mouseSensitivity * lookInput.y;

            // Clamp pitch between lookAngle
            pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

            transform.localEulerAngles = new Vector3(0, yaw, 0);
            face.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        }
    }

    private void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(foot.position, groundDist, groundMask);
        //Debug.Log($"Player is Grounded: {isGrounded}");
        controller.SimpleMove(moveSpd * Time.fixedDeltaTime * input);
        controller.Move(Time.fixedDeltaTime * up);
    }

    private void LateUpdate()
    {
        cam.localRotation = Quaternion.Euler(pitch, 0, 0);
        transform.Rotate(Vector3.up * xMove);
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
}
