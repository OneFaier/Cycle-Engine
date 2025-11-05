using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoverSpaceshipAdvanced : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxForwardSpeed = 60f;    // vitesse max
    public float acceleration = 80f;       // accélération max (m/s²)
    public float turnSpeed = 40f;          // vitesse de rotation
    public float turnSmooth = 5f;          // lissage rotation

    [Header("Hover Settings")]
    public float hoverHeight = 2f;         // hauteur de sustentation
    public float hoverForce = 250f;        // force de sustentation
    public float hoverDamping = 3f;        // amortissement vertical
    public LayerMask groundLayer;          // couche du sol

    [Header("Control Cubes")]
    public IndicatorMouseClickFast speedCube;
    public IndicatorMouseClickFast directionCube;

    [Header("Pilotage")]
    public bool isPiloting = false;

    [Header("Gravité personnalisée")]
    public float gravityForce = 30f; // gravité (force vers le sol)

    private Rigidbody rb;
    private float yawInputSmooth = 0f;
    private RaycastHit groundHit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // on gère la gravité nous-mêmes
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.mass = 200f;
        rb.linearDamping = 0.5f;
        rb.angularDamping = 3f;
    }

    void FixedUpdate()
    {
        if (isPiloting)
            MoveAndTurn();

        ApplyHoverAndGravity();
    }

    void MoveAndTurn()
    {
        // Lecture des cubes
        float speedVal = speedCube ? speedCube.positionNormalized : 0f; // 0..1
        float dirVal = directionCube ? directionCube.positionNormalized : 0.5f;

        // Calcul direction de déplacement (suivre la pente)
        Vector3 forward = transform.forward;

        // Projection du vecteur de déplacement sur le plan du sol (évite de "planer" dans le vide)
        if (Physics.Raycast(transform.position, -transform.up, out groundHit, hoverHeight * 2f, groundLayer))
        {
            Vector3 groundNormal = groundHit.normal;
            forward = Vector3.ProjectOnPlane(forward, groundNormal).normalized;
        }

        // Vitesse désirée
        Vector3 desiredVelocity = forward * (speedVal * maxForwardSpeed);

        // Accélération limitée
        Vector3 requiredAccel = (desiredVelocity - rb.linearVelocity) / Time.fixedDeltaTime;
        if (requiredAccel.magnitude > acceleration)
            requiredAccel = requiredAccel.normalized * acceleration;

        rb.AddForce(requiredAccel, ForceMode.Acceleration);

        // Rotation (yaw)
        float yawInput = Mathf.Lerp(-1f, 1f, dirVal);
        yawInputSmooth = Mathf.Lerp(yawInputSmooth, yawInput, Time.fixedDeltaTime * turnSmooth);
        rb.AddTorque(Vector3.up * yawInputSmooth * turnSpeed, ForceMode.Acceleration);
    }

    void ApplyHoverAndGravity()
    {
        // On raycast sous le vaisseau pour trouver la hauteur du sol
        if (Physics.Raycast(transform.position, -transform.up, out groundHit, hoverHeight * 2f, groundLayer))
        {
            float heightError = hoverHeight - groundHit.distance;
            Vector3 upwardSpeed = Vector3.Project(rb.linearVelocity, transform.up);
            float lift = heightError * hoverForce - upwardSpeed.magnitude * hoverDamping;

            // Force de sustentation
            rb.AddForce(transform.up * lift, ForceMode.Acceleration);

            // Aligner la rotation avec la normale du sol (suivi des pentes)
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, groundHit.normal) * transform.rotation;
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 5f));
        }

        // Gravité personnalisée
        rb.AddForce(-transform.up * gravityForce, ForceMode.Acceleration);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position - transform.up * hoverHeight * 2f);
    }
#endif
}
