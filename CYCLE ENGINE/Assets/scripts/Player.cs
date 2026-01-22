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

    [Header("Souris / Look")]
    public float mouseSensitivity = 100f;
    public float maxLookAngle = 80f;
    public bool mouseLookEnabled = true;

    [Header("Contrôle")]
    public bool canMove = true;

    // ===================== ACCELERATION CAMERA INERTIA =====================
    [Header("Acceleration Inertia Camera")]
    public float accelCameraDelay = 0.08f;      // délai après changement de gear
    public float accelCameraPull = 0.15f;       // recul max caméra
    public float accelCameraPullSpeed = 8f;     // vitesse de tirage arrière
    public float accelCameraReturnSpeed = 6f;   // vitesse de retour

    [Header("Gear Lever")]
    public IndicatorGearMouse gearLever;

    // TPS smoothing
    public float tpsSmoothTime = 0.1f;

    // ---------------- PRIVATE ----------------
    private Rigidbody rb;
    private float xRotation = 0f;
    private bool isGrounded = true;
    private bool isTPS = false;

    private Vector3 tpsVelocity = Vector3.zero;

    private int lastGear = -1;
    private Vector3 accelCameraOffset = Vector3.zero;
    private Coroutine accelRoutine;

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
        HandleGearAccelerationCamera();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    // ===================== LOCK MOUSE =====================
    void HandleMouseLock()
    {
        if (Cursor.lockState != CursorLockMode.Locked)
            Cursor.lockState = CursorLockMode.Locked;
        if (Cursor.visible)
            Cursor.visible = false;
    }

    // ===================== CAMERA LOOK =====================
    void HandleMouseLook()
    {
        if (!mouseLookEnabled) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        if (fpsCamera != null && !isTPS)
            fpsCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    // ===================== MOVEMENT =====================
    void HandleMovement()
    {
        if (!canMove) return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = (transform.right * x + transform.forward * z).normalized;
        Vector3 targetVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);

        rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, 0.8f);
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

        Vector3 desiredPosition = transform.position + tpsOffset;
        tpsCamera.transform.position = Vector3.SmoothDamp(
            tpsCamera.transform.position,
            desiredPosition,
            ref tpsVelocity,
            tpsSmoothTime
        );

        Vector3 lookTarget = transform.position + Vector3.up * 1.5f;
        tpsCamera.transform.rotation = Quaternion.Slerp(
            tpsCamera.transform.rotation,
            Quaternion.LookRotation(lookTarget - tpsCamera.transform.position),
            20f * Time.deltaTime
        );
    }

    // ===================== GEAR ACCELERATION CAMERA =====================
    void HandleGearAccelerationCamera()
    {
        if (gearLever == null || fpsCamera == null) return;

        int currentGear = gearLever.currentGear;

        if (currentGear != lastGear)
        {
            lastGear = currentGear;

            if (accelRoutine != null)
                StopCoroutine(accelRoutine);

            accelRoutine = StartCoroutine(AccelerationCameraInertia(currentGear));
        }
    }

    IEnumerator AccelerationCameraInertia(int gear)
    {
        // Petit délai pour éviter le kick instantané
        yield return new WaitForSeconds(accelCameraDelay);

        float gearFactor = Mathf.Clamp01(gear / 5f); // adapte si + ou - de gears
        Vector3 targetBack = -Vector3.forward * accelCameraPull * (0.5f + gearFactor);

        // Tirage arrière progressif
        while (Vector3.Distance(accelCameraOffset, targetBack) > 0.01f)
        {
            accelCameraOffset = Vector3.Lerp(
                accelCameraOffset,
                targetBack,
                Time.deltaTime * accelCameraPullSpeed
            );

            fpsCamera.transform.localPosition = accelCameraOffset;
            yield return null;
        }

        // Retour doux vers position neutre
        while (accelCameraOffset.magnitude > 0.01f)
        {
            accelCameraOffset = Vector3.Lerp(
                accelCameraOffset,
                Vector3.zero,
                Time.deltaTime * accelCameraReturnSpeed
            );

            fpsCamera.transform.localPosition = accelCameraOffset;
            yield return null;
        }

        accelCameraOffset = Vector3.zero;
        fpsCamera.transform.localPosition = Vector3.zero;
    }

    // ===================== UTIL =====================
    public bool IsGrounded() => isGrounded;
}
