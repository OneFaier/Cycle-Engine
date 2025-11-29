using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SimpleSubmarineController : MonoBehaviour
{
    [Header("Controls (Cubes)")]
    public IndicatorMouseClickFast speedCube;      // 0 → stop, 1 → max forward
    public IndicatorMouseClickFast directionCube;  // 0 → gauche, 0.5 → centre, 1 → droite
    public IndicatorMouseClickFast elevationCube;  // 0 → descendre, 0.5 → neutre, 1 → monter

    [Header("Movement Settings")]
    public float maxForwardSpeed = 25f;
    public float acceleration = 18f;
    public float turnSpeed = 40f;

    [Header("Elevation Settings")]
    public float verticalSpeed = 12f;
    public float pitchAmount = 12f;        // combien il penche
    public float pitchSmooth = 4f;

    [Header("Ground Protection")]
    public LayerMask groundLayer;
    public float minGroundDistance = 3f;   // jamais descendre sous cette distance
    public float groundCheckDistance = 6f;

    private Rigidbody rb;
    private float currentPitch = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.linearDamping = 1f;
        rb.angularDamping = 3f;
    }

    void FixedUpdate()
    {
        float speedVal = speedCube ? speedCube.positionNormalized : 0f;
        float dirVal = directionCube ? directionCube.positionNormalized : 0.5f;
        float elevVal = elevationCube ? elevationCube.positionNormalized : 0.5f;

        MoveForward(speedVal);
        Turn(dirVal);
        MoveVertical(elevVal);
        ApplyPitchTilt(elevVal);
        PreventGoingTooLow();
    }

    void MoveForward(float input)
    {
        Vector3 desiredVel = transform.forward * (input * maxForwardSpeed);
        Vector3 diff = desiredVel - rb.linearVelocity;

        Vector3 accel = diff;
        if (accel.magnitude > acceleration)
            accel = accel.normalized * acceleration;

        rb.AddForce(accel, ForceMode.Acceleration);
    }

    void Turn(float input)
    {
        float yaw = Mathf.Lerp(-1f, 1f, input);
        rb.AddTorque(Vector3.up * yaw * turnSpeed, ForceMode.Acceleration);
    }

    void MoveVertical(float input)
    {
        float elev = Mathf.Lerp(-1f, 1f, input);

        rb.AddForce(Vector3.up * elev * verticalSpeed, ForceMode.Acceleration);
    }

    void ApplyPitchTilt(float elevVal)
    {
        float elev = Mathf.Lerp(-1f, 1f, elevVal);

        // monte → penche en avant (pitch négatif)
        // descend → penche en arrière (pitch positif)
        float targetPitch = -elev * pitchAmount;

        currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.fixedDeltaTime * pitchSmooth);

        Quaternion rot = Quaternion.Euler(currentPitch, transform.eulerAngles.y, 0f);
        rb.MoveRotation(rot);
    }

    void PreventGoingTooLow()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance, groundLayer))
        {
            if (hit.distance < minGroundDistance)
            {
                float pushForce = (minGroundDistance - hit.distance) * 30f;
                rb.AddForce(Vector3.up * pushForce, ForceMode.Acceleration);
            }
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.down * groundCheckDistance);
    }
#endif
}
