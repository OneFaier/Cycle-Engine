using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class HoverSpaceshipSimple : MonoBehaviour
{
    [Header("Movement Settings")]
    public float maxForwardSpeed = 60f;    // vitesse max
    public float acceleration = 120f;      // force d'accélération
    public float turnSpeed = 30f;          // vitesse de rotation réduite
    public float turnSmooth = 5f;          // damping pour la rotation

    [Header("Control Cubes")]
    public IndicatorMouseClickFast speedCube;
    public IndicatorMouseClickFast directionCube;

    [Header("Pilotage")]
    public bool isPiloting = false;

    private Rigidbody rb;
    private float yawInputSmooth = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = true;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.angularDamping = 5f;   // damping pour stabiliser la rotation
        rb.linearDamping = 2f;           // légère résistance pour le mouvement
    }

    void FixedUpdate()
    {
        if (!isPiloting) return;

        MoveAndTurn();
    }

    void MoveAndTurn()
    {
        // Lecture des cubes
        float speedVal = speedCube != null ? speedCube.positionNormalized : 0f;
        float dirVal = directionCube != null ? directionCube.positionNormalized : 0.5f;

        // --- Mouvement avant/arrière plus réactif ---
        Vector3 forwardVel = transform.forward * speedVal * maxForwardSpeed;
        Vector3 velocityChange = forwardVel - rb.linearVelocity;
        rb.AddForce(velocityChange * acceleration, ForceMode.Acceleration);

        // --- Rotation Yaw plus stable ---
        float yawInput = Mathf.Lerp(-1f, 1f, dirVal);
        yawInputSmooth = Mathf.Lerp(yawInputSmooth, yawInput, Time.fixedDeltaTime * turnSmooth);
        rb.AddTorque(Vector3.up * yawInputSmooth * turnSpeed, ForceMode.Acceleration);
    }
}