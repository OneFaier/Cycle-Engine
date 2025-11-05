using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoverSpaceshipAdvanced : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxForwardSpeed = 60f;    
    public float acceleration = 80f;       
    public float turnSpeed = 40f;          
    public float turnSmooth = 5f;

    [Header("Hover Settings")]
    public float hoverHeight = 2f;         
    public float hoverForce = 250f;        
    public float hoverDamping = 3f;        
    public LayerMask groundLayer;

    [Header("Control Cubes")]
    public IndicatorMouseClickFast speedCube;
    public IndicatorMouseClickFast directionCube;

    [Header("Pilotage")]
    public bool isPiloting = true;  // 🔹 toujours true par défaut

    [Header("Gravité personnalisée")]
    public float gravityForce = 30f;       
    public float gravityMultiplier = 2f;   
    public float maxGravityDistance = 20f; 

    [Header("Recul Canon")]
    public float cannonImpulseForce = 50f; 

    private Rigidbody rb;
    private float yawInputSmooth = 0f;
    private RaycastHit groundHit;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
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
        float speedVal = speedCube ? speedCube.positionNormalized : 0f;
        float dirVal = directionCube ? directionCube.positionNormalized : 0.5f;

        Vector3 forward = transform.forward;

        if (Physics.Raycast(transform.position, -transform.up, out groundHit, hoverHeight * 2f, groundLayer))
        {
            Vector3 groundNormal = groundHit.normal;
            forward = Vector3.ProjectOnPlane(forward, groundNormal).normalized;
        }

        Vector3 desiredVelocity = forward * (speedVal * maxForwardSpeed);

        Vector3 requiredAccel = (desiredVelocity - rb.linearVelocity) / Time.fixedDeltaTime;
        if (requiredAccel.magnitude > acceleration)
            requiredAccel = requiredAccel.normalized * acceleration;

        rb.AddForce(requiredAccel, ForceMode.Acceleration);

        float yawInput = Mathf.Lerp(-1f, 1f, dirVal);
        yawInputSmooth = Mathf.Lerp(yawInputSmooth, yawInput, Time.fixedDeltaTime * turnSmooth);
        rb.AddTorque(Vector3.up * yawInputSmooth * turnSpeed, ForceMode.Acceleration);
    }

    void ApplyHoverAndGravity()
    {
        bool grounded = Physics.Raycast(transform.position, -transform.up, out groundHit, hoverHeight * 2f, groundLayer);

        if (grounded)
        {
            float heightError = hoverHeight - groundHit.distance;
            float verticalSpeed = Vector3.Dot(rb.linearVelocity, transform.up);
            float lift = heightError * hoverForce - verticalSpeed * hoverDamping;

            rb.AddForce(transform.up * lift, ForceMode.Acceleration);

            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, groundHit.normal) * transform.rotation;
            rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 5f));
        }

        Vector3 gravityDirection = Vector3.down;
        float gravityStrength = gravityForce;

        if (Physics.Raycast(transform.position, Vector3.down, out groundHit, maxGravityDistance, groundLayer))
        {
            float dist = groundHit.distance;
            float t = Mathf.InverseLerp(0, maxGravityDistance, dist);
            gravityStrength = gravityForce * Mathf.Lerp(0.5f, gravityMultiplier, t);
        }

        rb.AddForce(gravityDirection * gravityStrength, ForceMode.Acceleration);
    }

    public void ApplyCannonImpulse(Vector3 cannonDirection)
    {
        if (rb != null)
            rb.AddForce(-cannonDirection.normalized * cannonImpulseForce, ForceMode.Impulse);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position - transform.up * hoverHeight * 2f);
    }
#endif
}
