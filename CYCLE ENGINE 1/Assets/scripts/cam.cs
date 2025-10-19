using UnityEngine;

[RequireComponent(typeof(Camera))]
public class FreeCamera : MonoBehaviour
{
    public float moveSpeed = 10f;
    public float sprintMultiplier = 2f;
    public float mouseSensitivity = 100f;
    public float maxLookAngle = 89f;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        HandleLook();
        HandleMovement();
    }

    void HandleLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX, Space.World);
    }

    void HandleMovement()
    {
        float speed = moveSpeed * (Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1f);

        Vector3 move = transform.right * Input.GetAxis("Horizontal") +
                       transform.forward * Input.GetAxis("Vertical");

        transform.position += move * speed * Time.deltaTime;
    }
}