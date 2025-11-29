using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwimController : MonoBehaviour
{
    [Header("Mouvement nage")]
    public float swimSpeed = 3f;
    public float gravity = 9.81f;

    [Header("Caméra")]
    public Camera playerCamera;
    public float mouseSensitivity = 100f;
    public float maxLookAngle = 80f;

    [Header("Contrôle")]
    public bool canMove = true;

    [Header("Limites")]
    public float waterHeight = 0f; // hauteur de la surface de l'eau

    private Rigidbody rb;
    private float xRotation = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

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
        LimitHeightSmooth();
    }

    void HandleMouseLook()
    {
        if (!canMove) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);
        playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * mouseX);
    }

    void HandleSwimMovement()
    {
        if (!canMove) return;

        // Déplacement selon les touches
        float x = Input.GetAxisRaw("Horizontal"); // A/D
        float z = Input.GetAxisRaw("Vertical");   // W/S

        // Direction du regard
        Vector3 forward = playerCamera.transform.forward;
        Vector3 right = playerCamera.transform.right;

        // Combinaison pour mouvement relatif à la caméra
        Vector3 moveDir = (forward * z + right * x).normalized;

        // Appliquer la vitesse
        rb.linearVelocity = moveDir * swimSpeed;
    }

    void ApplyGravity()
    {
        if (!canMove) return;

        if (transform.position.y < waterHeight)
        {
            rb.linearVelocity += Vector3.down * gravity * Time.fixedDeltaTime;
        }
    }

    void LimitHeightSmooth()
    {
        if (transform.position.y > waterHeight)
        {
            Vector3 vel = rb.linearVelocity;

            // Limite la vitesse ascendante pour rester à la surface
            if (vel.y > 0)
            {
                vel.y = Mathf.Min(vel.y, (waterHeight - transform.position.y) / Time.fixedDeltaTime);
                rb.linearVelocity = vel;
            }
        }
    }
}
