using UnityEngine;

public class SnapPointDirection : MonoBehaviour
{
    public string snapCategory = "Direction"; // Catégorie du SnapPoint
    public float snapRange = 3f;              // Rayon d’attraction
    public float snapSpeed = 10f;             // Vitesse de l’attraction
    public float attachDistance = 0.1f;       // Distance pour fixer l’objet

    private void FixedUpdate()
    {
        Rigidbody[] grabbables = FindObjectsOfType<Rigidbody>();
        foreach (Rigidbody rb in grabbables)
        {
            if (!rb.CompareTag("Grabbable")) continue;

            // Vérifie la catégorie de l'objet
            ObjectCategory objCategory = rb.GetComponent<ObjectCategory>();
            if (objCategory == null) continue;
            if (objCategory.category != snapCategory) continue;

            float distance = Vector3.Distance(rb.position, transform.position);

            if (distance <= snapRange)
            {
                // Attire l’objet vers le SnapPoint
                Vector3 direction = (transform.position - rb.position).normalized;
                rb.linearVelocity = direction * snapSpeed;

                // Snap complet si proche
                if (distance <= attachDistance)
                {
                    rb.position = transform.position;
                    rb.rotation = transform.rotation;
                    rb.linearVelocity = Vector3.zero;
                    rb.useGravity = false;

                    // **DEVIENT ENFANT DU SNAPPOINT**
                    if (rb.transform.parent != transform)
                        rb.transform.SetParent(transform);
                }
            }
            else
            {
                // Détache si trop loin
                if (rb.transform.parent == transform)
                {
                    rb.transform.SetParent(null);
                    rb.useGravity = true;
                }
            }
        }
    }
}
