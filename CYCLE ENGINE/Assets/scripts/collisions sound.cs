using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpaceshipAdvanced : MonoBehaviour
{
    [Header("Movement Settings")]
    public float[] gearSpeeds = { 0f, 20f, 40f, 65f, 90f }; // vitesses par gear
    public float acceleration = 80f;
    public float turnSpeed = 40f;
    public float turnSmooth = 5f;

    [Header("Hover Settings")]
    public float hoverHeight = 2f;
    public float hoverForce = 250f;
    public float hoverDamping = 3f;
    public LayerMask groundLayer;
    public Transform[] hoverPoints;
    public Transform hoverOrigin;

    [Header("Controls")]
    public IndicatorGearMouse gearLever;
    public IndicatorMouseClickFast directionCube;

    [Header("Pilotage")]
    public bool isPiloting = true;

    [Header("Gear Boost / Propulsion")]
    public float gearImpulse = 120f;
    public ParticleSystem propulsionEffect;
    public AudioSource propulsionAudio;
    public float propulsionPitchMultiplier = 0.05f;

    [Header("Camera Effects")]
    public Camera shipCamera;
    public float cameraAccelFov = 5f;
    public float cameraMaxFov = 70f;
    public float cameraMinFov = 60f;
    public float cameraFovSmooth = 2f;
    public float maxRollAngle = 20f; // inclinaison max en Z
    public float maxPitchTilt = 10f; // inclinaison avant/arrière

    [Header("Engine Sound")]
    public AudioSource engineAudio;
    public float minPitch = 0.8f;
    public float maxPitch = 1.2f;
    public float pitchSensitivity = 1f;
    public float volumeSensitivity = 0.5f;

    Rigidbody rb;
    float yawInputSmooth;
    int lastGear = 0;
    float targetRoll = 0f;
    float targetPitchTilt = 0f;
    float hoverDisabledUntil = 0f;
    float currentFov;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.mass = 200f;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

#if UNITY_6000_0_OR_NEWER
        rb.linearDamping = 0.2f;  // moins de drift
        rb.angularDamping = 2f;
#else
        rb.drag = 0.2f;
        rb.angularDrag = 2f;
#endif

        if (shipCamera != null)
            currentFov = shipCamera.fieldOfView;
    }

    void FixedUpdate()
    {
        if (isPiloting)
            MoveAndTurn();

        ApplyHoverAndGravity();
        UpdateEngineSound();
        UpdateCameraEffects();
    }

    void MoveAndTurn()
    {
        int gear = gearLever ? gearLever.GetGear() : 0;
        float targetSpeed = gearSpeeds[Mathf.Clamp(gear, 0, gearSpeeds.Length - 1)];

        // orientation et forward
        Vector3 forward = transform.forward;
        if (GetAverageGroundNormal(out Vector3 avgNormal))
            forward = Vector3.ProjectOnPlane(forward, avgNormal).normalized;

#if UNITY_6000_0_OR_NEWER
        float currentSpeed = Vector3.Dot(rb.linearVelocity, forward);
#else
        float currentSpeed = Vector3.Dot(rb.velocity, forward);
#endif

        // accélération
        float speedError = targetSpeed - currentSpeed;
        float accel = Mathf.Clamp(speedError, -acceleration, acceleration);
        rb.AddForce(forward * accel, ForceMode.Acceleration);

        // propulsion visuelle/audio
        if (gear > lastGear)
        {
            rb.AddForce(forward * gearImpulse, ForceMode.Impulse);
            if (propulsionEffect != null) propulsionEffect.Play();
            if (propulsionAudio != null) propulsionAudio.pitch = 1f + gear * propulsionPitchMultiplier;
        }

        lastGear = gear;

        // ---------- TURN ----------
        float dirVal = directionCube ? directionCube.positionNormalized : 0.5f;
        float yawInput = Mathf.Lerp(-1f, 1f, dirVal);
        yawInputSmooth = Mathf.Lerp(yawInputSmooth, yawInput, Time.fixedDeltaTime * turnSmooth);
        rb.AddTorque(Vector3.up * yawInputSmooth * turnSpeed, ForceMode.Acceleration);

        // ---------- ROLL et PITCH ----------
        targetRoll = -yawInput * maxRollAngle; // inclinaison selon virage
        targetPitchTilt = Mathf.Clamp(speedError / acceleration, -maxPitchTilt, maxPitchTilt);

        Quaternion targetRotation = Quaternion.Euler(targetPitchTilt, transform.eulerAngles.y, targetRoll);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.fixedDeltaTime * 3f);
    }

    void ApplyHoverAndGravity()
    {
        if (Time.time < hoverDisabledUntil) return;

        if (GetAverageGround(out _, out Vector3 avgNormal, out float avgDist))
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

        if (Physics.Raycast(GetHoverOrigin(), Vector3.down, out RaycastHit hit, 20f, groundLayer))
        {
            float t = Mathf.InverseLerp(0, 20f, hit.distance);
            rb.AddForce(Vector3.down * 30f * Mathf.Lerp(0.5f, 2f, t), ForceMode.Acceleration);
        }
        else
        {
            rb.AddForce(Vector3.down * 30f, ForceMode.Acceleration);
        }
    }

    void UpdateEngineSound()
    {
        if (!engineAudio) return;
#if UNITY_6000_0_OR_NEWER
        float speed = Vector3.Dot(rb.linearVelocity, transform.forward);
#else
        float speed = Vector3.Dot(rb.velocity, transform.forward);
#endif
        float pitch = 1f + speed / gearSpeeds[gearSpeeds.Length - 1] * pitchSensitivity;
        engineAudio.pitch = Mathf.Lerp(engineAudio.pitch, Mathf.Clamp(pitch, minPitch, maxPitch), Time.fixedDeltaTime * 3f);
        engineAudio.volume = Mathf.Lerp(engineAudio.volume, 0.5f + Mathf.Abs(speed) / gearSpeeds[gearSpeeds.Length - 1] * volumeSensitivity, Time.fixedDeltaTime * 3f);
    }

    void UpdateCameraEffects()
    {
        if (!shipCamera) return;

#if UNITY_6000_0_OR_NEWER
        float speed = Vector3.Dot(rb.linearVelocity, transform.forward);
#else
        float speed = Vector3.Dot(rb.velocity, transform.forward);
#endif
        float targetFov = cameraMinFov + cameraAccelFov * Mathf.Clamp01(speed / gearSpeeds[gearSpeeds.Length - 1]);
        currentFov = Mathf.Lerp(currentFov, targetFov, Time.fixedDeltaTime * cameraFovSmooth);
        shipCamera.fieldOfView = currentFov;

        // léger tilt caméra selon roulis
        if (rb.linearVelocity.magnitude > 0.5f)
        {
            shipCamera.transform.localRotation = Quaternion.Lerp(
                shipCamera.transform.localRotation,
                Quaternion.Euler(targetPitchTilt, 0f, targetRoll * 0.5f),
                Time.fixedDeltaTime * 3f
            );
        }
    }

    bool GetAverageGround(out Vector3 avgPoint, out Vector3 avgNormal, out float avgDist)
    {
        int count = 0;
        avgPoint = Vector3.zero;
        avgNormal = Vector3.zero;
        avgDist = 0f;

        foreach (var p in hoverPoints)
        {
            if (!p) continue;
            if (Physics.Raycast(p.position, -p.up, out RaycastHit hit, hoverHeight * 2f, groundLayer))
            {
                avgPoint += hit.point;
                avgNormal += hit.normal;
                avgDist += hit.distance;
                count++;
            }
        }

        if (count == 0) return false;
        avgPoint /= count;
        avgNormal.Normalize();
        avgDist /= count;
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

    // CANON / RECOIL
    public float cannonLinearForce = 500f;
    public float cannonTorqueForce = 5f;
    public float cannonVerticalImpulse = 800f;
    public float hoverOverrideTime = 0.15f;
    public void ApplyCannonImpulse(Transform firePoint)
    {
        Vector3 recoilDir = -firePoint.forward;
        float downwardFactor = Mathf.Clamp01(-firePoint.forward.y);
        rb.AddForce(recoilDir * cannonLinearForce + Vector3.up * downwardFactor * cannonVerticalImpulse, ForceMode.Impulse);
        rb.AddTorque(transform.right * cannonTorqueForce, ForceMode.Impulse);
        hoverDisabledUntil = Time.time + hoverOverrideTime;
    }
}
