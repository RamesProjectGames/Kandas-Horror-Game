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
    private float xMove, xrot, yrot, upVel;
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
        Vector3 hor = transform.right * Input.GetAxis("Horizontal");
        Vector3 ver = transform.forward * Input.GetAxis("Vertical");
        up = transform.up * upVel;
        input = hor + ver;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            moveSpd = 200f;
        else
            moveSpd = 100f;

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
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        yrot += mouseX;

        xrot -= mouseY;
        xrot = Mathf.Clamp(xrot, minVerticalAngle/2, maxVerticalAngle);

        // Apply rotation with optional smoothing
        // Player body only rotates horizontally
        Quaternion playerTargetRotation = Quaternion.Euler(0f, yrot, 0f);
        transform.rotation = Quaternion.Slerp(transform.rotation, playerTargetRotation,
                                              smoothTime * Time.deltaTime * 20f);

        // Camera rotates both horizontally AND vertically
        Quaternion camTargetRotation = Quaternion.Euler(xrot, yrot, 0f);
        playerCam.transform.rotation = Quaternion.Slerp(playerCam.transform.rotation,
                                                        camTargetRotation,
                                                        smoothTime * Time.deltaTime * 20f);

        // Face rotates vertically only (relative to player)
        // Use localRotation to keep it relative to player body
        Quaternion faceTargetRotation = Quaternion.Euler(xrot, 0f, 0f);
        face.transform.localRotation = Quaternion.Slerp(face.transform.localRotation,
                                                        faceTargetRotation,
                                                        smoothTime * Time.deltaTime * 20f);
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
        cam.localRotation = Quaternion.Euler(xrot, 0, 0);
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
