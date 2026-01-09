using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleFPSController : MonoBehaviour
{
    [Header("Mouvement")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Caméras")]
    public Camera fpsCamera;           // caméra 1ᵉre personne
    public Camera tpsCamera;           // caméra 3ᵉ personne
    public Vector3 tpsOffset = new Vector3(0f, 2f, -4f); // offset caméra TPS

    [Header("Souris / Look")]
    public float mouseSensitivity = 100f;
    public float maxLookAngle = 80f;
    public bool mouseLookEnabled = true; // pour désactiver rotation souris (ex : siège)

    [Header("Contrôle")]
    public bool canMove = true;

    [Header("Camera Boost FPS")]
    public float boostDistance = 0.5f;     // distance de recul lors du boost
    public float boostDuration = 0.2f;     // durée de l’impulsion
    private float boostTimer = 0f;
    private Vector3 cameraBoostOffset = Vector3.zero;

    private Rigidbody rb;
    private float xRotation = 0f;
    private bool isGrounded = true;
    private bool isTPS = false;

    // Pour détecter changement de gear / vitesse
    private int lastGear = 0;
    public IndicatorGearMouse gearLever;   // référence au levier de vitesse

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (fpsCamera != null) fpsCamera.enabled = true;
        if (tpsCamera != null) tpsCamera.enabled = false;

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
        UpdateFPSCameraBoost();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    // ===================== CAMERA FPS BOOST ===================

    void UpdateFPSCameraBoost()
    {
        if (fpsCamera == null) return;

        if (boostTimer > 0f)
        {
            boostTimer -= Time.deltaTime;
            // interpolation pour revenir à zéro
            cameraBoostOffset = Vector3.Lerp(cameraBoostOffset, Vector3.zero, Time.deltaTime / boostDuration);
        }
        else
        {
            cameraBoostOffset = Vector3.zero;
        }

        fpsCamera.transform.localPosition = cameraBoostOffset;
    }

    // ===================== LOCK MOUSE =====================
    void HandleMouseLock()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            Cursor.lockState = CursorLockMode.Locked;
        if (Cursor.visible)
            Cursor.visible = false;
    }

    // ===================== CAMERA =========================
    void HandleMouseLook()
    {
        if (!mouseLookEnabled) return;
        Camera cam = isTPS ? tpsCamera : fpsCamera;
        if (cam == null) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }

    // ===================== MOVEMENT ======================
    void HandleMovement()
    {
        if (!canMove) return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = (transform.right * x + transform.forward * z).normalized;
        rb.linearVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);
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

    // ===================== SWITCH FPS / TPS =====================
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
        tpsCamera.transform.position = transform.position + tpsOffset;
        tpsCamera.transform.LookAt(transform.position + Vector3.up * 1.5f);
    }

    // ===================== UTIL ==========================
    public bool IsGrounded() => isGrounded;
}
