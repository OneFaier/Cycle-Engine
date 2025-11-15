using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleFPSController : MonoBehaviour
{
    [Header("Mouvement")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Caméra")]
    public Camera playerCamera;
    public float mouseSensitivity = 100f;
    public float maxLookAngle = 80f;

    [Header("Contrôle")]
    public bool canMove = true;

    [Header("Footsteps")]
    public AudioClip[] footstepClips;
    public AudioSource footstepAudioSource;
    public float stepDistance = 2f;
    public float minMoveSpeed = 0.1f;
    public float footstepVolume = 0.5f;

    private Rigidbody rb;
    private float xRotation = 0f;
    private bool isGrounded = true;

    // Footstep tracking
    private Vector3 lastPosition;
    private float distanceMoved = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        lastPosition = transform.position;

        if (!footstepAudioSource)
        {
            footstepAudioSource = gameObject.AddComponent<AudioSource>();
            footstepAudioSource.spatialBlend = 1f;
        }
    }

    void Update()
    {
        HandleMouseLook();   // Caméra fluide
        HandleJump();
        HandleFootsteps();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    // ---------------- Caméra fluide ----------------
    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    // ---------------- Mouvement ----------------
    void HandleMovement()
    {
        if (!canMove) return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = (transform.right * x + transform.forward * z).normalized;
        Vector3 targetVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);

        rb.linearVelocity = targetVelocity;
    }

    void HandleJump()
    {
        if (!canMove) return;

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
    }

    public bool IsGrounded() => isGrounded;

    // ---------------- Footsteps ----------------
    void HandleFootsteps()
    {
        if (!canMove || !isGrounded) return;
        if (footstepClips.Length == 0) return;

        Vector3 horizontalMovement = new Vector3(transform.position.x - lastPosition.x, 0f,
                                                 transform.position.z - lastPosition.z);

        distanceMoved += horizontalMovement.magnitude;

        if (distanceMoved >= stepDistance && horizontalMovement.magnitude > minMoveSpeed)
        {
            if (rb.linearVelocity.y <= 0.01f) // Ne joue pas le son en l'air
            {
                PlayFootstep();
            }
            distanceMoved = 0f;
        }

        lastPosition = transform.position;
    }

    void PlayFootstep()
    {
        AudioClip clip = footstepClips[Random.Range(0, footstepClips.Length)];
        footstepAudioSource.PlayOneShot(clip, footstepVolume);
    }
}
