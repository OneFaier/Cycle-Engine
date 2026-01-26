using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class SimpleFPSController : MonoBehaviour
{
    [Header("Mouvement")]
    public float moveSpeed = 5f;
    public float jumpForce = 5f;

    [Header("Caméra FPS")]
    public Camera fpsCamera;

    // --- variables TPS conservées mais inutilisées pour ne rien casser ---
    [HideInInspector] public Camera tpsCamera = null;
    [HideInInspector] public Vector3 tpsOffset = Vector3.zero;
    [HideInInspector] public float tpsSmoothTime = 0.1f;
    [HideInInspector] public bool isTPS = false;
    [HideInInspector] private Vector3 tpsVelocity;

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

    [Header("Gear / Camera Effects")]
    public IndicatorGearMouse gearLever; // référence au levier
    public float cameraShiftAmount = 0.3f; // recul / avancée
    public float cameraShiftSpeed = 5f;    // vitesse de lerp
    private Vector3 cameraDefaultLocalPos;
    private float targetCameraZ = 0f;
    private int lastGear = 0;

    private Rigidbody rb;
    private float xRotation = 0f;
    private bool isGrounded = true;

    private float distanceMoved = 0f;
    private Vector3 lastPosition;
    private int lastFootstepIndex = -1;

    // Variables utilisées par d'autres scripts (steapoint, etc.)
    [HideInInspector] public Transform seatTransform = null;
    [HideInInspector] public bool isSeated = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        if (fpsCamera != null)
        {
            fpsCamera.enabled = true;
            cameraDefaultLocalPos = fpsCamera.transform.localPosition;
        }

        lastPosition = transform.position;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLock();
        HandleMouseLook();
        HandleJump();
        HandleFootsteps();
        UpdateCameraGearEffect();
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

        if (fpsCamera != null)
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

    // --- Effet caméra lors du changement de gear ---
    void UpdateCameraGearEffect()
    {
        if (fpsCamera == null || gearLever == null) return;

        int currentGear = gearLever.GetLimitedGear();

        // Si la vitesse du gear change vraiment
        if (currentGear != lastGear)
        {
            if (currentGear > lastGear)
                targetCameraZ = -cameraShiftAmount; // recul
            else if (currentGear < lastGear)
                targetCameraZ = cameraShiftAmount * 0.6f; // avance plus soft

            lastGear = currentGear;
        }

        // Lerp vers la position
        Vector3 camPos = fpsCamera.transform.localPosition;
        camPos.z = Mathf.Lerp(camPos.z, targetCameraZ, Time.deltaTime * cameraShiftSpeed);
        fpsCamera.transform.localPosition = camPos;

        // Reset target une fois arrivé
        if (Mathf.Abs(camPos.z - targetCameraZ) < 0.01f)
            targetCameraZ = 0f;
    }
}
