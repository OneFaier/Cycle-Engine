using UnityEngine;

public class CannonProjectile : MonoBehaviour
{
    public float speed = 80f;
    public float explosionRadius = 10f;  // rayon de l'effet de repousse
    public float explosionForce = 1000f; // force max appliquée
    public GameObject explosionVFX;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        Explode();
    }

    void Explode()
    {
        // Effet visuel
        if (explosionVFX)
            Instantiate(explosionVFX, transform.position, Quaternion.identity);

        // Détecte tous les rigidbodies dans le rayon
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider col in colliders)
        {
            if (col.attachedRigidbody != null)
            {
                Rigidbody targetRb = col.attachedRigidbody;

                // Calcule la direction et la force en fonction de la distance
                Vector3 dir = (targetRb.position - transform.position).normalized;
                float distance = Vector3.Distance(targetRb.position, transform.position);
                float forceMultiplier = 1f - Mathf.Clamp01(distance / explosionRadius);

                targetRb.AddForce(dir * explosionForce * forceMultiplier, ForceMode.Impulse);
            }
        }

        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
#endif
}