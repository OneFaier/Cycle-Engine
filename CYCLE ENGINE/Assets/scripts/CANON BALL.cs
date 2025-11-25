using UnityEngine;

public class CannonBall : MonoBehaviour
{
    [Header("Projectile Settings")]
    public float speed = 80f;
    public float explosionRadius = 10f;
    public float explosionForce = 1000f;
    public GameObject explosionVFX;

    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.linearVelocity = transform.forward * speed;
    }

    void OnCollisionEnter(Collision collision)
    {
        // Vérifie si l'objet touché est un BreakableRock
        BreakableRock rock = collision.collider.GetComponentInParent<BreakableRock>();
        if (rock != null)
        {
            // Applique le point d'impact pour générer l'effet
            rock.Break();

        }

        Explode();
    }

    void Explode()
    {
        // Effet visuel
        if (explosionVFX)
            Instantiate(explosionVFX, transform.position, Quaternion.identity);

        // Applique la force à tous les rigidbodies proches
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        foreach (Collider col in colliders)
        {
            if (col.attachedRigidbody != null)
            {
                Rigidbody targetRb = col.attachedRigidbody;
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