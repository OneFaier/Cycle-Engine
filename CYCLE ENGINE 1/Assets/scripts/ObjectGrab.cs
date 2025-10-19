using UnityEngine;

public class ObjectGrabTMP : MonoBehaviour
{
    [Header("Références")]
    public Camera playerCamera;

    [Header("Paramètres")]
    public float grabRange = 3f;
    public float holdDistance = 2f;

    private Rigidbody heldObject;

    void Update()
    {
        // --- Attraper un objet ---
        if (Input.GetMouseButtonDown(0) && heldObject == null)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, grabRange))
            {
                if (hit.collider.CompareTag("Grabbable"))
                {
                    Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                    if (rb != null)
                    {
                        heldObject = rb;
                        heldObject.useGravity = false;
                    }
                }
            }
        }

        // --- Lâcher l'objet ---
        if (Input.GetMouseButtonDown(1) && heldObject != null)
        {
            heldObject.useGravity = true;
            heldObject = null;
        }

        // --- Maintenir devant soi ---
        if (heldObject != null)
        {
            Vector3 targetPos = playerCamera.transform.position + playerCamera.transform.forward * holdDistance;
            heldObject.position = targetPos;
        }
    }
}
