using UnityEngine;
using UnityEngine.UI;

public class ENGINEMovements : MonoBehaviour
{
    [Header("Références des cubes")]
    public IndicatorMouseClickFast directionCube;
    public IndicatorMouseClickFast speedCube;

    [Header("Paramètres véhicule")]
    public float CurrentSpeed => currentSpeed;
    public float maxSpeed = 15f;
    public float acceleration = 8f;
    public float deceleration = 6f;
    public float turnSpeed = 60f;
    public float steerSmoothing = 5f;

    [Header("Paramètres de survol")]
    public float hoverHeight = 2f;
    public float hoverFollowSpeed = 8f;
    public float hoverForwardOffset = 2f;
    public float maxHoverDistance = 8f;
    public float fallGravity = 15f;
    public LayerMask groundLayer;

    [Header("Sliders externes (optionnels)")]
    public Slider externalSpeedSlider;     // Vitesse
    public Slider externalDirectionSlider; // Direction

    private float currentSpeed = 0f;
    private float currentSteer = 0f;
    private float verticalVelocity = 0f;

    [HideInInspector] public bool isEngineOn = true;

    void Update()
    {
        if (directionCube == null || speedCube == null)
            return;

        float speedNormalized;
        float directionNormalized;

        // ---- Gestion des cubes selon moteur ----
        if (isEngineOn)
        {
            speedNormalized = Mathf.Clamp01(speedCube.positionNormalized);
            directionNormalized = Mathf.Clamp01(directionCube.positionNormalized);
        }
        else
        {
            speedNormalized = 0f;
            directionNormalized = 0.5f; // centre
        }

        // ---- Bloquer la vitesse si slider externe à 0 ----
        if (externalSpeedSlider != null && externalSpeedSlider.value <= 0f)
        {
            speedNormalized = 0f;
            speedCube.positionNormalized = 0f;
        }

        // ---- Bloquer la direction si slider externe direction à 0 ----
        if (externalDirectionSlider != null && externalDirectionSlider.value <= 0f)
        {
            directionNormalized = 0.5f; // recentre le cube
            directionCube.positionNormalized = 0.5f;
        }

        // ---- Direction ----
        float targetSteer = Mathf.Lerp(-1f, 1f, directionNormalized);
        currentSteer = Mathf.Lerp(currentSteer, targetSteer, Time.deltaTime * steerSmoothing);

        // ---- Vitesse ----
        float targetSpeed = speedNormalized * maxSpeed;

        if (isEngineOn)
        {
            if (targetSpeed > currentSpeed)
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, acceleration * Time.deltaTime);
            else
                currentSpeed = Mathf.MoveTowards(currentSpeed, targetSpeed, deceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, deceleration * Time.deltaTime);
        }

        // ---- Si la vitesse atteint 0, bloque le cube à 0 ----
        if (currentSpeed <= 0.01f)
        {
            currentSpeed = 0f;
            speedCube.positionNormalized = 0f;
        }

        // ---- Déplacement horizontal (avant/arrière) ----
        transform.position += transform.forward * currentSpeed * Time.deltaTime;

        // ---- Rotation Y (joueur) ----
        transform.Rotate(Vector3.up, currentSteer * turnSpeed * Time.deltaTime);

        // ---- Gestion de la hauteur + inclinaison ----
        Vector3 rayOrigin = transform.position + transform.forward * hoverForwardOffset + Vector3.up;
        if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, maxHoverDistance, groundLayer))
        {
            // Ajuste la hauteur
            float targetY = hit.point.y + hoverHeight;
            float newY = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * hoverFollowSpeed);
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            verticalVelocity = 0f;

            // Rotation X/Z : suit la pente mais Y reste joueur
            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            Vector3 euler = slopeRotation.eulerAngles;
            euler.y = transform.eulerAngles.y; // conserve la rotation Y
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(euler), Time.deltaTime * 3f);
        }
        else
        {
            // Pas de sol → chute libre
            verticalVelocity -= fallGravity * Time.deltaTime;
            transform.position += Vector3.up * verticalVelocity * Time.deltaTime;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        Gizmos.color = Color.cyan;
        Vector3 rayOrigin = transform.position + transform.forward * hoverForwardOffset + Vector3.up;
        Gizmos.DrawLine(rayOrigin, rayOrigin + Vector3.down * maxHoverDistance);
    }
}
