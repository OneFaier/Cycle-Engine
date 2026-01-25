using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpaceshipAdvanced : MonoBehaviour
{
    [Header("Audio Multipliers")]
    public float propulsionVolumeMultiplier = 1.0f; // Multiplie le volume du son principal
    public float propulsionPitchMultiplierPublic = 1.0f; // Multiplie le pitch du son principal
    public float extraVolumeMultiplier = 1.0f; // Multiplie le volume du son secondaire
    public float extraPitchMultiplierPublic = 1.0f; // Multiplie le pitch du son secondaire

    #region Movement Settings
    [Header("Movement Settings")]
    public float[] gearSpeeds = { 0f, 20f, 40f, 65f, 90f };
    public float acceleration = 80f;
    public float turnSpeed = 40f;
    public float turnSmooth = 5f;
    #endregion

    #region Hover Settings
    [Header("Hover Settings")]
    public float hoverHeight = 2f;
    public float hoverForce = 250f;
    public float hoverDamping = 5f;
    public LayerMask groundLayer;
    public Transform[] hoverPoints;
    public Transform hoverOrigin;
    #endregion

    #region Controls
    [Header("Controls")]
    public IndicatorGearMouse gearLever;
    public IndicatorMouseClickFast directionCube;
    #endregion

    #region Boost / Propulsion
    [Header("Boost / Propulsion")]
    public float gearImpulse = 120f;
    public AudioSource propulsionAudio;
    public AudioSource extraAudio;
    public float propulsionPitchMultiplier = 0.05f;

    [Header("Propulsion Particles")]
    public GameObject propulsionEffectObject;
    public int gearToActivateParticles = 3;
    #endregion

    Rigidbody rb;
    float yawInputSmooth;
    int lastGear = 0;
    float currentRoll;
    float rollVelocity;

    const float STOP_THRESHOLD = 0.6f;
    const float LOW_SPEED_THRESHOLD = 2f;
    public float maxRollAngle = 20f;

    // Smoothing audio
    float propulsionVolumeSmooth;
    float extraVolumeSmooth;
    float propulsionPitchSmooth;
    float extraPitchSmooth;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.mass = 200f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

#if UNITY_6000_0_OR_NEWER
        rb.linearDamping = 0.2f;
        rb.angularDamping = 2f;
#else
        rb.drag = 0.2f;
        rb.angularDrag = 2f;
#endif

        if (hoverOrigin != null)
            rb.centerOfMass = rb.transform.InverseTransformPoint(hoverOrigin.position);
        else
            rb.centerOfMass = Vector3.zero;

        if (propulsionEffectObject)
            propulsionEffectObject.SetActive(false);
    }

    void FixedUpdate()
    {
        ApplyHover();
        HandleMovement();
        UpdatePropulsionParticles();
    }

    void UpdatePropulsionAudio(float speed, int gear)
    {
        if (propulsionAudio == null && extraAudio == null) return;

        float maxSpeed = gearSpeeds[gearSpeeds.Length - 1];
        float speedRatio = Mathf.Clamp01(speed / maxSpeed);

        // ---- Propulsion Audio ----
        if (propulsionAudio != null)
        {
            propulsionAudio.spatialBlend = 0f;
            propulsionAudio.volume = Mathf.Lerp(0.5f, 1.5f, speedRatio) * propulsionVolumeMultiplier; // Multiplicateur public
            propulsionAudio.pitch = Mathf.Lerp(1f, 2f, speedRatio) * propulsionPitchMultiplierPublic; // Multiplicateur public
        }

        // ---- Extra Audio ----
        if (extraAudio != null)
        {
            extraAudio.spatialBlend = 0f;
            extraAudio.volume = Mathf.Lerp(0.5f, 1.5f, speedRatio) * extraVolumeMultiplier; // Multiplicateur public
            extraAudio.pitch = Mathf.Lerp(0.8f, 1.8f, speedRatio) * extraPitchMultiplierPublic; // Multiplicateur public
        }
    }

    void HandleMovement()
    {
        int gear = gearLever ? gearLever.GetLimitedGear() : 0;
        float targetSpeed = gearSpeeds[Mathf.Clamp(gear, 0, gearSpeeds.Length - 1)];

#if UNITY_6000_0_OR_NEWER
        Vector3 velocity = rb.linearVelocity;
#else
        Vector3 velocity = rb.velocity;
#endif

        float speed = Vector3.Dot(velocity, transform.forward);
        float absSpeed = velocity.magnitude;

        UpdatePropulsionAudio(speed, gear);

        if (gear == 0 && absSpeed < STOP_THRESHOLD)
        {
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, Time.fixedDeltaTime * 6f);
            rb.linearDamping = 4f;
#else
            rb.velocity = Vector3.Lerp(rb.velocity, Vector3.zero, Time.fixedDeltaTime * 6f);
            rb.drag = 4f;
#endif
            return;
        }

#if UNITY_6000_0_OR_NEWER
        rb.linearDamping = 0.2f;
#else
        rb.drag = 0.2f;
#endif

        float speedError = targetSpeed - speed;
        float accel = Mathf.Clamp(speedError, -acceleration, acceleration);
        rb.AddForce(transform.forward * accel, ForceMode.Acceleration);

        if (gear > lastGear)
        {
            rb.AddForce(transform.forward * gearImpulse, ForceMode.Impulse);
        }
        lastGear = gear;

        float dirVal = directionCube ? directionCube.GetLimitedNormalized() : 0.5f;
        float yawInput = Mathf.Lerp(-1f, 1f, dirVal);
        yawInputSmooth = Mathf.Lerp(yawInputSmooth, yawInput, Time.fixedDeltaTime * turnSmooth);

        rb.AddTorque(Vector3.up * yawInputSmooth * turnSpeed, ForceMode.Acceleration);

        if (velocity.magnitude > LOW_SPEED_THRESHOLD)
        {
            Vector3 newDir = Vector3.Slerp(velocity.normalized, transform.forward, Time.fixedDeltaTime * turnSmooth);
#if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = newDir * rb.linearVelocity.magnitude;
#else
            rb.velocity = newDir * rb.velocity.magnitude;
#endif
        }

        float targetRoll = -yawInput * maxRollAngle;
        currentRoll = Mathf.SmoothDampAngle(currentRoll, targetRoll, ref rollVelocity, 0.18f);

        Quaternion rollRotation = Quaternion.AngleAxis(currentRoll, transform.forward);
        Quaternion forwardRotation = Quaternion.LookRotation(transform.forward, transform.up);
        Quaternion targetRotation = rollRotation * forwardRotation;

        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRotation, Time.fixedDeltaTime * 6f));
    }

    void ApplyHover()
    {
        if (!GetAverageGround(out Vector3 avgNormal, out float avgDist)) return;

        float heightError = hoverHeight - avgDist;

#if UNITY_6000_0_OR_NEWER
        float verticalSpeed = Vector3.Dot(rb.linearVelocity, transform.up);
#else
        float verticalSpeed = Vector3.Dot(rb.velocity, transform.up);
#endif

        float lift = heightError * hoverForce - verticalSpeed * hoverDamping;
        rb.AddForce(transform.up * lift, ForceMode.Acceleration);

        Quaternion targetRot = Quaternion.FromToRotation(transform.up, avgNormal) * transform.rotation;
        rb.MoveRotation(Quaternion.Slerp(rb.rotation, targetRot, Time.fixedDeltaTime * 6f));
    }

    void UpdatePropulsionParticles()
    {
        if (propulsionEffectObject == null) return;

        int gear = gearLever ? gearLever.GetLimitedGear() : 0;

        if (gear >= gearToActivateParticles)
        {
            if (!propulsionEffectObject.activeSelf)
                propulsionEffectObject.SetActive(true);
        }
        else
        {
            if (propulsionEffectObject.activeSelf)
                propulsionEffectObject.SetActive(false);
        }
    }

    bool GetAverageGround(out Vector3 avgNormal, out float avgDist)
    {
        avgNormal = Vector3.zero;
        avgDist = 0f;
        int count = 0;

        foreach (var p in hoverPoints)
        {
            if (!p) continue;
            if (Physics.Raycast(p.position, -p.up, out RaycastHit hit, hoverHeight * 2f, groundLayer))
            {
                avgNormal += hit.normal;
                avgDist += hit.distance;
                count++;
            }
        }

        if (count == 0) return false;

        avgNormal.Normalize();
        avgDist /= count;
        return true;
    }
}
