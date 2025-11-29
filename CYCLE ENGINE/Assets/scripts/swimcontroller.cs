using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwimController : MonoBehaviour
{
    [Header("Mouvement nage")]
    public float swimSpeed = 3f;
    public float ascendSpeed = 2f;
    public float gravity = 9.81f;

    [Header("Caméra")]
    public Camera playerCamera;
    public float mouseSensitivity = 100f;
    public float maxLookAngle = 80f;

    [Header("Contrôle")]
    public bool canMove = true;

    private Rigidbody rb;
    private float xRotation = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

        // Verrouille le curseur et le rend invisible
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleMouseLook();
    }

    void FixedUpdate()
    {
        HandleSwimMovement();
        ApplyGravity();
    }

    void HandleMouseLook()
    {
        if (!canMove) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleSwimMovement()
    {
        if (!canMove) return;

        float x = Input.GetAxisRaw("Horizontal");
        float z = Input.GetAxisRaw("Vertical");
        float y = 0f;

        if (Input.GetKey(KeyCode.Space)) y += ascendSpeed;
        if (Input.GetKey(KeyCode.LeftControl)) y -= ascendSpeed;

        Vector3 moveDir = transform.right * x + transform.forward * z + Vector3.up * y;
        rb.linearVelocity = moveDir * swimSpeed;
    }

    void ApplyGravity()
    {
        if (!canMove) return;

        if (!Input.GetKey(KeyCode.Space))
            rb.linearVelocity += Vector3.down * gravity * Time.fixedDeltaTime;
    }
}