using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class SimpleFPSController : MonoBehaviour
{
    [Header("Mouvement")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Caméras")]
    public Camera fpsCamera;
    public Camera tpsCamera;
    public Vector3 tpsOffset = new Vector3(0f, 2f, -4f);
    public float tpsSmoothTime = 0.1f;   // Smooth TPS

    [Header("Souris / Look")]
    public float mouseSensitivity = 100f;
    public float maxLookAngle = 80f;
    public bool mouseLookEnabled = true;

    [Header("Contrôle")]
    public bool canMove = true;

    [Header("Footsteps")]
    public AudioSource footstepSource;
    public AudioClip[] footstepClips;
    public float stepDistance = 2f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private float xRotation = 0f;
    private bool isGrounded = true;
    private bool isTPS = false;

    private Vector3 tpsVelocity;

    private float distanceMoved = 0f;
    private Vector3 lastPosition;
    private int lastFootstepIndex = -1;

    [HideInInspector] public Transform seatTransform = null;
    [HideInInspector] public bool isSeated = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (fpsCamera != null) fpsCamera.enabled = true;
        if (tpsCamera != null) tpsCamera.enabled = false;

        lastPosition = transform.position;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLock();
        HandleMouseLook();
        HandleJump();
        HandleCameraSwitch();
        UpdateTPSCameraPosition();
        HandleFootsteps();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMouseLock()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            Cursor.lockState = CursorLockMode.Locked;
        if (Cursor.visible)
            Cursor.visible = false;
    }

    void HandleMouseLook()
    {
        if (!mouseLookEnabled || isSeated) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        if (fpsCamera != null && !isTPS)
            fpsCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleMovement()
    {
        if (!canMove || isSeated) return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = (transform.right * x + transform.forward * z).normalized;
        Vector3 targetVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);

        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, 0.8f);
    }

    void HandleJump()
    {
        if (!canMove || isSeated) return;

        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            isGrounded = false;
        }
    }

    void OnCollisionStay(Collision collision)
    {
        if (((1 << collision.gameObject.layer) & groundLayer) != 0)
            isGrounded = true;
    }

    void HandleCameraSwitch()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            isTPS = !isTPS;
            if (fpsCamera != null) fpsCamera.enabled = !isTPS;
            if (tpsCamera != null) tpsCamera.enabled = isTPS;
        }
    }

    void UpdateTPSCameraPosition()
    {
        if (!isTPS || tpsCamera == null) return;

        Vector3 targetPos = transform.position + tpsOffset;
        tpsCamera.transform.position = Vector3.SmoothDamp(
            tpsCamera.transform.position,
            targetPos,
            ref tpsVelocity,
            tpsSmoothTime
        );

        Vector3 lookTarget = transform.position + Vector3.up * 1.5f;
        tpsCamera.transform.rotation = Quaternion.LookRotation(lookTarget - tpsCamera.transform.position);
    }

    void HandleFootsteps()
    {
        if (footstepClips.Length == 0 || footstepSource == null) return;
        if (isSeated) return;

        if (!Physics.Raycast(transform.position + Vector3.up * 0.1f, Vector3.down, 1.2f, groundLayer)) return;

        Vector3 delta = transform.position - lastPosition;
        distanceMoved += new Vector3(delta.x, 0f, delta.z).magnitude;

        if (distanceMoved >= stepDistance)
        {
            PlayFootstep();
            distanceMoved = 0f;
        }

        lastPosition = transform.position;
    }

    void PlayFootstep()
    {
        int index;
        do
        {
            index = Random.Range(0, footstepClips.Length);
        } while (index == lastFootstepIndex && footstepClips.Length > 1);

        lastFootstepIndex = index;
        footstepSource.PlayOneShot(footstepClips[index]);
    }

    public bool IsGrounded() => isGrounded;
}
