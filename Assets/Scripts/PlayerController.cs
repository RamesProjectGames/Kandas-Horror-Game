using UnityEngine;

//[RequireComponent (typeof(CharacterController), typeof(Animator))]
[RequireComponent (typeof(CharacterController), typeof(AudioSource))]
public class PlayerController : MonoBehaviour
{
    [Header("Built-in")]
    [SerializeField] private CharacterController controller;
    [SerializeField] private AudioSource audioSrc;
    [SerializeField] private GameObject face;
    [SerializeField] private Camera playerCam;
    //[SerializeField] private Animator anim;
    [SerializeField] private Transform cam;
    public Transform ground;
    public LayerMask layerMask;
    public AudioClip[] audioClip;
    private Vector2 lockAxis;
    private Vector3 input, up;

    [Header("Numeric Values")]
    [SerializeField] private float jumpPow;
    [SerializeField] private float gravity = 9.81f, groundDist = 1f, moveSpd = 100f, jumpCd;
    private float xMove, pitch, yaw, upVel;
    private bool isGrounded;

    [Header("Mouse Settings")]
    public float mouseSensitivity = 2.0f;
    public float smoothTime = 0.1f;

    [Header("Rotation Limits")]
    public float minVerticalAngle = -20f;
    public float maxVerticalAngle = 20f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //anim = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();
        audioSrc = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        //Jump
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

        if (isGrounded && Input.GetKeyDown(KeyCode.Space) && jumpCd <= 0f)
        {
            //anim.SetBool("isJumping", true);
            upVel = Mathf.Sqrt(jumpPow * 2f * gravity);
            isGrounded = false;
            jumpCd = 1f;
        }

        //Movement
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            moveSpd = 200f;
        else
            moveSpd = 100f;

        Vector3 hor = transform.right * Input.GetAxis("Horizontal") * moveSpd * Time.deltaTime;
        Vector3 ver = transform.forward * Input.GetAxis("Vertical") * moveSpd * Time.deltaTime;
        up = transform.up * upVel;

        input = hor + ver;

        if (input == Vector3.zero)
        {
            //anim.SetFloat("Speed", 0f);
            //anim.SetBool("isWalking", false);
            //anim.SetBool("isRunning", false);
        }
        else if (!Input.GetKey(KeyCode.LeftShift) && !Input.GetKey(KeyCode.RightShift))
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

        //Rotation
        yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;

        pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");

        // Clamp pitch between lookAngle
        pitch = Mathf.Clamp(pitch, minVerticalAngle, maxVerticalAngle);

        transform.localEulerAngles = new Vector3(0, yaw, 0);
        face.transform.localEulerAngles = new Vector3(pitch, 0, 0);
    }
    
    private void FixedUpdate()
    {
        isGrounded = Physics.CheckSphere(ground.position, groundDist, layerMask);
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
