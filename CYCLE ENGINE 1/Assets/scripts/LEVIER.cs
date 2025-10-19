using UnityEngine;

public class LeverSelfRotate : MonoBehaviour
{
    public float minAngle = -45f;      // Angle minimal
    public float maxAngle = 45f;       // Angle maximal
    public float rotationSpeed = 50f;  // Vitesse de rotation

    private Camera playerCamera;
    private bool isGrabbed = false;
    private float currentAngle = 0f;

    void Start()
    {
        playerCamera = Camera.main;
    }

    void Update()
    {
        // Commencer à attraper le levier
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 10f))
            {
                if (hit.collider.gameObject == gameObject)
                    isGrabbed = true;
            }
        }

        // Relâcher le levier
        if (Input.GetMouseButtonUp(0))
            isGrabbed = false;

        // Rotation si attrapé
        if (isGrabbed)
        {
            float mouseX = Input.GetAxis("Mouse X") * rotationSpeed * Time.deltaTime;

            // Calcul nouvel angle et clamp
            currentAngle = Mathf.Clamp(currentAngle + mouseX, minAngle, maxAngle);

            // Applique la rotation sur lui-même (axe Z)
            transform.localRotation = Quaternion.Euler(0f, 0f, currentAngle);

            // DEBUG
            Debug.Log("Lever angle: " + currentAngle);
        }
    }
}
