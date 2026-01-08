using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class overboard: MonoBehaviour
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

    [Header("Hover Points (à placer manuellement)")]
    [Tooltip("Place ici tes empties : avant, arrière, gauche, droite, centre...")]
    public Transform[] hoverPoints;

    [Header("Centre de gravité / pivot du vaisseau")]
    public Transform hoverOrigin;

    [Header("Control Cubes (UI)")]
    public IndicatorMouseClickFast speedCube;
    public IndicatorMouseClickFast directionCube;

    [Header("Pilotage")]
    public bool isPiloting = true;

    [Header("Gravité personnalisée")]
    public float gravityForce = 30f;
    public float gravityMultiplier = 2f;
    public float maxGravityDistance = 20f;

    [Header("Recul Canon")]
    public float cannonImpulseForce = 50f;

    [Header("Engine Sound")]
    public AudioSource engineAudio;
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;
    public float pitchSensitivity = 1f;
    public float volumeSensitivity = 0.5f;

    private Rigidbody rb;
    private float yawInputSmooth = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.mass = 200f;

#if UNITY_6000_0_OR_NEWER
        rb.linearDamping = 0.5f;
        rb.angularDamping = 3f;
#else
        rb.drag = 0.5f;
        rb.angularDrag = 3f;
#endif
    }

    void FixedUpdate()
    {
        if (isPiloting)
            MoveAndTurn();

        ApplyHoverAndGravity();

        UpdateEngineSound();
    }

    void MoveAndTurn()
    {
        float speedVal = speedCube ? speedCube.positionNormalized : 0f;
        float dirVal = directionCube ? directionCube.positionNormalized : 0.5f;

        Vector3 forward = transform.forward;

        if (GetAverageGroundNormal(out Vector3 avgNormal))
            forward = Vector3.ProjectOnPlane(forward, avgNormal).normalized;

#if UNITY_6000_0_OR_NEWER
        Vector3 desiredVelocity = forward * (speedVal * maxForwardSpeed);
        Vector3 requiredAccel = (desiredVelocity - rb.linearVelocity) / Time.fixedDeltaTime;
#else
        Vector3 desiredVelocity = forward * (speedVal * maxForwardSpeed);
        Vector3 requiredAccel = (desiredVelocity - rb.velocity) / Time.fixedDeltaTime;
#endif

        if (requiredAccel.magnitude > acceleration)
            requiredAccel = requiredAccel.normalized * acceleration;

        rb.AddForce(requiredAccel, ForceMode.Acceleration);

        float yawInput = Mathf.Lerp(-1f, 1f, dirVal);
        yawInputSmooth = Mathf.Lerp(yawInputSmooth, yawInput, Time.fixedDeltaTime * turnSmooth);
        rb.AddTorque(Vector3.up * yawInputSmooth * turnSpeed, ForceMode.Acceleration);
    }

    void ApplyHoverAndGravity()
    {
        
        if (Time.time < hoverDisabledUntil) return;
        
        if (GetAverageGround(out Vector3 avgPoint, out Vector3 avgNormal, out float avgDist))
        {
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

        if (Physics.Raycast(GetHoverOrigin(), Vector3.down, out RaycastHit groundHit, maxGravityDistance, groundLayer))
        {
            float t = Mathf.InverseLerp(0, maxGravityDistance, groundHit.distance);
            float gravityStrength = gravityForce * Mathf.Lerp(0.5f, gravityMultiplier, t);
            rb.AddForce(Vector3.down * gravityStrength, ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
        }
    }

    void UpdateEngineSound()
    {
        if (engineAudio == null) return;

#if UNITY_6000_0_OR_NEWER
        float forwardSpeed = Vector3.Dot(rb.linearVelocity, transform.forward);
#else
        float forwardSpeed = Vector3.Dot(rb.velocity, transform.forward);
#endif

        float yawAmount = yawInputSmooth;

        float targetPitch = 1f + forwardSpeed / maxForwardSpeed * pitchSensitivity + Mathf.Abs(yawAmount) * 0.1f;
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);
        engineAudio.pitch = Mathf.Lerp(engineAudio.pitch, targetPitch, Time.fixedDeltaTime * 3f);

        float targetVolume = 0.5f + Mathf.Abs(forwardSpeed) / maxForwardSpeed * volumeSensitivity;
        engineAudio.volume = Mathf.Lerp(engineAudio.volume, targetVolume, Time.fixedDeltaTime * 3f);
    }

    bool GetAverageGround(out Vector3 avgPoint, out Vector3 avgNormal, out float avgDist)
    {
        int hitCount = 0;
        avgPoint = Vector3.zero;
        avgNormal = Vector3.zero;
        avgDist = 0f;

        if (hoverPoints == null || hoverPoints.Length == 0)
            return false;

        foreach (var p in hoverPoints)
        {
            if (p == null) continue;

            if (Physics.Raycast(p.position, -p.up, out RaycastHit hit, hoverHeight * 2f, groundLayer))
            {
                avgPoint += hit.point;
                avgNormal += hit.normal;
                avgDist += hit.distance;
                hitCount++;
            }
        }

        if (hitCount == 0)
            return false;

        avgPoint /= hitCount;
        avgNormal.Normalize();
        avgDist /= hitCount;
        return true;
    }

    bool GetAverageGroundNormal(out Vector3 avgNormal)
    {
        avgNormal = Vector3.up;
        return GetAverageGround(out _, out avgNormal, out _);
    }

    Vector3 GetHoverOrigin()
    {
        return hoverOrigin ? hoverOrigin.position : transform.position;
    }

    [Header("Recul Canon")]
    public float cannonLinearForce = 500f;      // Force qui pousse doucement le vaisseau
    public float cannonTorqueForce = 5f;        // Secousse très légère

    public float cannonVerticalImpulse = 800f;  // puissance du saut
    public float hoverOverrideTime = 0.15f;     // durée pendant laquelle le hover est ignoré
    private float hoverDisabledUntil = 0f;

    public void ApplyCannonImpulse(Transform firePointTransform)
    {
        if (rb == null || firePointTransform == null) return;

        // Direction opposée au tir
        Vector3 recoilDir = -firePointTransform.forward;

        // Composante verticale pour le saut
        float downwardFactor = Mathf.Clamp01(-firePointTransform.forward.y);
        Vector3 upwardImpulse = Vector3.up * downwardFactor * cannonVerticalImpulse;

        // Recul horizontal classique
        Vector3 linearImpulse = recoilDir * cannonLinearForce;

        rb.AddForce(linearImpulse + upwardImpulse, ForceMode.Impulse);

        rb.AddTorque(transform.right * cannonTorqueForce, ForceMode.Impulse);

        // Désactive temporairement le hover vertical
        hoverDisabledUntil = Time.time + hoverOverrideTime;
    }








#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (hoverPoints == null) return;

        foreach (var p in hoverPoints)
        {
            if (p == null) continue;
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(p.position, p.position - p.up * hoverHeight * 2f);
            Gizmos.DrawSphere(p.position, 0.1f);
        }

        Gizmos.color = Color.red;
        Gizmos.DrawSphere(GetHoverOrigin(), 0.2f);
    }
#endif
}
