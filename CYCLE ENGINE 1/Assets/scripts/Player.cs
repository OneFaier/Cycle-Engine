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
    public bool canMove = true; // ✅ Ajouté pour désactiver les mouvements si nécessaire

    private Rigidbody rb;
    private float xRotation = 0f;
    private bool isGrounded = true;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook(); // Toujours actif, même si canMove = false
        HandleJump();
    }

    void FixedUpdate()
    {
        HandleMovement();
    }

    void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        transform.Rotate(Vector3.up * mouseX);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    void HandleMovement()
    {
        if (!canMove) return; // ✅ Bloque uniquement le mouvement

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = (transform.right * x + transform.forward * z).normalized;
        Vector3 targetVelocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);

        rb.linearVelocity = targetVelocity;
    }

    void HandleJump()
    {
        if (!canMove) return; // ✅ Bloque le jump si canMove = false

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
}
