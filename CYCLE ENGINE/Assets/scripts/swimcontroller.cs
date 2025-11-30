using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SwimController : MonoBehaviour
{
    [Header("Mouvement nage")]
    public float swimSpeed = 3f;
    public float verticalSwimSpeed = 3f;

    [Header("Caméra")]
    public Camera playerCamera;
    public float mouseSensitivity = 100f;
    public float maxLookAngle = 80f;

    [Header("Contrôle")]
    public bool canMove = true;

    [Header("Limites")]
    public float waterHeight = 0f;

    private Rigidbody rb;
    private float xRotation = 0f;
    private bool isInWater = false;

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
        isInWater = transform.position.y < waterHeight;
    }

    void FixedUpdate()
    {
        if (isInWater)
        {
            SwimMovement();
        }
        else
        {
            rb.useGravity = true;
        }
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

    void SwimMovement()
    {
        rb.useGravity = false;

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        // Direction 3D complète basée sur la caméra (inclut haut/bas)
        Vector3 moveDir = (playerCamera.transform.forward * z + playerCamera.transform.right * x).normalized;

        // Inputs supplémentaires verticaux
        float verticalInput = 0f;
        if (Input.GetKey(KeyCode.Space)) verticalInput = 1f;
        if (Input.GetKey(KeyCode.LeftControl)) verticalInput = -1f;

        // Combine vertical input (space/ctrl)
        moveDir += Vector3.up * verticalInput;
        moveDir.Normalize();

        // ARRÊT INSTANTANÉ : si pas d’input, vitesse = 0 directe
        if (x == 0 && z == 0 && verticalInput == 0)
        {
            rb.linearVelocity = Vector3.zero;
            return;
        }

        rb.linearVelocity = moveDir * swimSpeed;
    }
}
