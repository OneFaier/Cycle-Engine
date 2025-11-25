using UnityEngine;

public class SnapWithSurface : MonoBehaviour
{
    public Camera playerCamera;
    public float grabRange = 3f;
    public float holdDistance = 2f;
    public float snapDistance = 2f; // tolérance pour snap (1 à 2 mètres)

    private Rigidbody heldObject = null;

    void Update()
    {
        // Attraper objet
        if (Input.GetMouseButtonDown(0) && heldObject == null)
        {
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            if (Physics.Raycast(ray, out RaycastHit hit, grabRange))
            {
                Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
                if (rb != null && hit.collider.CompareTag("Grabbable"))
                {
                    heldObject = rb;
                    heldObject.useGravity = false;

                    // Détache si déjà snapé
                    if (heldObject.transform.parent != null)
                        heldObject.transform.SetParent(null);
                }
            }
        }

        // Lâcher objet
        if (Input.GetMouseButtonDown(1) && heldObject != null)
        {
            SnapPoint bestSnap = FindClosestSnapPoint(heldObject);
            if (bestSnap != null)
            {
                // Position la plus proche sur le collider du SnapPoint
                Vector3 snapPos = bestSnap.GetComponent<Collider>().ClosestPoint(heldObject.transform.position);
                heldObject.transform.position = snapPos;

                // Aligner rotation
                heldObject.transform.rotation = bestSnap.transform.rotation;

                // Devenir enfant
                heldObject.transform.SetParent(bestSnap.transform);
            }

            heldObject.useGravity = true;
            heldObject = null;
        }

        // Maintenir l’objet devant soi
        if (heldObject != null)
        {
            Vector3 targetPos = playerCamera.transform.position + playerCamera.transform.forward * holdDistance;
            // Utiliser velocity pour collisions (optionnel)
            Vector3 moveDir = targetPos - heldObject.position;
            heldObject.linearVelocity = moveDir / Time.deltaTime;
        }
    }

    SnapPoint FindClosestSnapPoint(Rigidbody obj)
    {
        SnapPoint[] snaps = FindObjectsOfType<SnapPoint>();
        SnapPoint closest = null;
        float minDist = snapDistance;

        foreach (SnapPoint snap in snaps)
        {
            Collider snapCollider = snap.GetComponent<Collider>();
            if (snapCollider == null) continue;

            // Distance de la surface la plus proche
            float dist = Vector3.Distance(snapCollider.ClosestPoint(obj.position), obj.position);

            if (dist < minDist)
            {
                closest = snap;
                minDist = dist;
            }
        }
        return closest;
    }
}
