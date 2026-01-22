using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpaceshipAdvanced : MonoBehaviour
{
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
    public float propulsionPitchMultiplier = 0.05f;

    [Header("Propulsion Particles")]
    public GameObject propulsionEffectObject; // Empty qui contient le ParticleSystem
    public int gearToActivateParticles = 3;   // À partir de ce gear, les particules s'activent
    #endregion

    // ================= PRIVATE =================
    Rigidbody rb;
    float yawInputSmooth;
    int lastGear = 0;
    float targetRoll = 0f;
    
    
    float currentRoll;
    float rollVelocity;

    const float STOP_THRESHOLD = 0.6f;
    const float LOW_SPEED_THRESHOLD = 2f;
    public float maxRollAngle = 20f;

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

        // Forcer le centre de masse sur le pivot public "hoverOrigin" ou le milieu de l'Empty
        if (hoverOrigin != null)
            rb.centerOfMass = rb.transform.InverseTransformPoint(hoverOrigin.position);
        else
            rb.centerOfMass = Vector3.zero; // fallback : centre de l'Empty

        // Désactiver l'Empty au départ
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
        if (propulsionAudio == null) return;

        // 2D = son identique dans les deux oreilles
        propulsionAudio.spatialBlend = 0f;

        // Volume : faible à l'arrêt, max en vitesse
        float minVolume = 0.2f;
        float maxVolume = 1f;
        float maxSpeed = gearSpeeds[gearSpeeds.Length - 1];
        float speedRatio = Mathf.Clamp01(speed / maxSpeed);
        propulsionAudio.volume = Mathf.Lerp(minVolume, maxVolume, speedRatio);

        // Pitch : augmente avec la vitesse
        float pitchBase = 1f;
        float pitchMultiplier = 0.5f; // ajustable selon ton goût
        propulsionAudio.pitch = pitchBase + speedRatio * pitchMultiplier;
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

    // Mise à jour du son de propulsion
    UpdatePropulsionAudio(speed, gear);

    // Stabilisation à l’arrêt
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

    // Accélération
    float speedError = targetSpeed - speed;
    float accel = Mathf.Clamp(speedError, -acceleration, acceleration);
    rb.AddForce(transform.forward * accel, ForceMode.Acceleration);

    // Boost / propulsion
    if (gear > lastGear)
    {
        rb.AddForce(transform.forward * gearImpulse, ForceMode.Impulse);
    }
    lastGear = gear;

    // Turn + Roll style avion
    float dirVal = directionCube ? directionCube.GetLimitedNormalized() : 0.5f;
    float yawInput = Mathf.Lerp(-1f, 1f, dirVal);
    yawInputSmooth = Mathf.Lerp(yawInputSmooth, yawInput, Time.fixedDeltaTime * turnSmooth);

    rb.AddTorque(Vector3.up * yawInputSmooth * turnSpeed, ForceMode.Acceleration);

    // Re-align velocity avec forward si assez rapide
    if (velocity.magnitude > LOW_SPEED_THRESHOLD)
    {
        Vector3 newDir = Vector3.Slerp(velocity.normalized, transform.forward, Time.fixedDeltaTime * turnSmooth);
#if UNITY_6000_0_OR_NEWER
        rb.linearVelocity = newDir * rb.linearVelocity.magnitude;
#else
        rb.velocity = newDir * rb.velocity.magnitude;
#endif
    }

    // ---- Roll stable autour du pivot ----
    float targetRoll = -yawInput * maxRollAngle;

    // Lissage du roll
    currentRoll = Mathf.SmoothDampAngle(currentRoll, targetRoll, ref rollVelocity, 0.18f);

    // Roll autour de l'axe local Z
    Quaternion rollRotation = Quaternion.AngleAxis(currentRoll, transform.forward);

    // Orientation actuelle du vaisseau (forward + up)
    Quaternion forwardRotation = Quaternion.LookRotation(transform.forward, transform.up);

    // Combiner roll + orientation
    Quaternion targetRotation = rollRotation * forwardRotation;

    // Appliquer rotation lissée
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
