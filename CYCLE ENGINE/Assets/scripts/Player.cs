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

    [Header("Camera Boost FPS")]
    public float boostDistance = 0.5f;
    public float boostDuration = 0.2f;
    private float boostTimer = 0f;
    private Vector3 cameraBoostOffset = Vector3.zero;

    [Header("Gear Lever Recoil")]
    public IndicatorGearMouse gearLever;
    public float gearBoostStrength = 2f;      // force du recul physique
    public float gearBoostSmoothTime = 0.2f;  // temps pour retour smooth
    public float cameraRecoilAmount = 0.1f;   // recul caméra
    private int lastGear = -1;
    private Vector3 gearBoostVelocity = Vector3.zero;

    // TPS smoothing
    private Vector3 tpsVelocity = Vector3.zero;
    public float tpsSmoothTime = 0.1f;

    private Rigidbody rb;
    private float xRotation = 0f;
    private bool isGrounded = true;
    private bool isTPS = false;

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
        HandleGearBoost();
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

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        if (fpsCamera != null && !isTPS)
            fpsCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    // ===================== MOVEMENT ======================
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
        tpsCamera.transform.position = Vector3.SmoothDamp(tpsCamera.transform.position, desiredPosition, ref tpsVelocity, tpsSmoothTime);

        Vector3 lookTarget = transform.position + Vector3.up * 1.5f;
        tpsCamera.transform.rotation = Quaternion.Slerp(tpsCamera.transform.rotation, Quaternion.LookRotation(lookTarget - tpsCamera.transform.position), 20f * Time.deltaTime);
    }

    // ===================== GEAR BOOST =====================
    void HandleGearBoost()
    {
        if (gearLever == null) return;

        int currentGear = gearLever.currentGear;

        if (currentGear != lastGear)
        {
            lastGear = currentGear;

            // Recul physique du joueur
            Vector3 backwardImpulse = -transform.forward * gearBoostStrength;
            rb.AddForce(backwardImpulse, ForceMode.Impulse);

            // Recul caméra FPS
            if (fpsCamera != null)
            {
                StartCoroutine(CameraRecoil());
            }
        }
    }

    private IEnumerator CameraRecoil()
    {
        float elapsed = 0f;
        Vector3 startPos = fpsCamera.transform.localPosition;
        Vector3 targetOffset = -Vector3.forward * cameraRecoilAmount;

        while (elapsed < gearBoostSmoothTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / gearBoostSmoothTime;
            fpsCamera.transform.localPosition = Vector3.Lerp(startPos + targetOffset, startPos, t);
            yield return null;
        }

        fpsCamera.transform.localPosition = startPos;
    }

    // ===================== UTIL ==========================
    public bool IsGrounded() => isGrounded;
}
