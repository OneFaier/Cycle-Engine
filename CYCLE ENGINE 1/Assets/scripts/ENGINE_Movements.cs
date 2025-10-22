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
    public float maxHoverDistance = 8f;
    public float fallGravity = 15f;
    public LayerMask groundLayer;

    [Header("Sliders externes (optionnels)")]
    public Slider externalSpeedSlider;
    public Slider externalDirectionSlider;

    [Header("Points de raycast (coins du véhicule)")]
    public Transform frontLeft;
    public Transform frontRight;
    public Transform rearLeft;
    public Transform rearRight;

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

        //  Bloque la vitesse avec les slider externe
        if (externalSpeedSlider != null && externalSpeedSlider.value <= 0f)
        {
            speedNormalized = 0f;
            speedCube.positionNormalized = 0f;
        }

        // Bloquer la direction
        if (externalDirectionSlider != null && externalDirectionSlider.value <= 0f)
        {
            directionNormalized = 0.5f; // recentre le cube
            directionCube.positionNormalized = 0.5f;
        }

        // Direction 
        float targetSteer = Mathf.Lerp(-1f, 1f, directionNormalized);
        currentSteer = Mathf.Lerp(currentSteer, targetSteer, Time.deltaTime * steerSmoothing);

        // Vitesse 
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

        //bloque cube a 0
        if (currentSpeed <= 0.01f)
        {
            currentSpeed = 0f;
            speedCube.positionNormalized = 0f;
        }

        // move avant arriere
        transform.position += transform.forward * currentSpeed * Time.deltaTime;

        // Rotation Y (direction)
        transform.Rotate(Vector3.up, currentSteer * turnSpeed * Time.deltaTime);

        // survol avec les 4 ray 
        Vector3[] positions = new Vector3[4];
        Vector3[] normals = new Vector3[4];
        Transform[] points = { frontLeft, frontRight, rearLeft, rearRight };
        bool[] hits = new bool[4];

        int hitCount = 0;
        for (int i = 0; i < 4; i++)
        {
            if (points[i] == null) continue;

            if (Physics.Raycast(points[i].position, Vector3.down, out RaycastHit hit, maxHoverDistance, groundLayer))
            {
                positions[i] = hit.point;
                normals[i] = hit.normal;
                hits[i] = true;
                hitCount++;
            }
        }

        if (hitCount > 0)
        {
            // Moyenne des positions touchées
            Vector3 avgPos = Vector3.zero;
            Vector3 avgNormal = Vector3.zero;

            for (int i = 0; i < 4; i++)
            {
                if (!hits[i]) continue;
                avgPos += positions[i];
                avgNormal += normals[i];
            }

            avgPos /= hitCount;
            avgNormal.Normalize();

            float targetY = avgPos.y + hoverHeight;
            float newY = Mathf.Lerp(transform.position.y, targetY, Time.deltaTime * hoverFollowSpeed);

            //position MAJ
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
            verticalVelocity = 0f;

            // Rotation selon la pentee
            Quaternion slopeRotation = Quaternion.FromToRotation(Vector3.up, avgNormal);
            Vector3 euler = slopeRotation.eulerAngles;
            euler.y = transform.eulerAngles.y;
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(euler), Time.deltaTime * 3f);
        }
        else
        {
            //chutelibre
            verticalVelocity -= fallGravity * Time.deltaTime;
            transform.position += Vector3.up * verticalVelocity * Time.deltaTime;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!Application.isPlaying)
            return;

        Gizmos.color = Color.cyan;
        Transform[] points = { frontLeft, frontRight, rearLeft, rearRight };
        foreach (var p in points)
        {
            if (p == null) continue;
            Gizmos.DrawLine(p.position, p.position + Vector3.down * maxHoverDistance);
        }
    }
}
