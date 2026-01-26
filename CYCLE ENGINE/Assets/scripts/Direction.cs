using UnityEngine;

public class IndicatorMouseClickFast : MonoBehaviour
{
    public enum CubeType { Vitesse, Direction }

    [Header("Type de Cube")]
    public CubeType cubeType = CubeType.Vitesse;

    [Header("Rail")]
    public Transform railStart;
    public Transform railEnd;

    [Header("Contrôles")]
    public float sensitivity = 0.05f;
    [Range(0f, 1f)]
    public float positionNormalized = 0.5f;
    public float snapThreshold = 0.05f;
    public float maxGrabDistance = 3f;
    public float returnSpeed = 2f;

    [Header("Surbrillance")]
    public Color highlightColor = Color.yellow;
    private Color baseColor;
    private Renderer rend;

    [Header("Référence Joueur & ModuleManager")]
    public SimpleFPSController playerController;
    public ModuleResourceManage moduleManager;

    private bool isGrabbed = false;
    private bool isHovered = false;

    private float lastValidHoverTime = -1f;
    private float hoverMemoryDuration = 0.1f;

    void Start()
    {
        // La caméra du joueur
        if (playerController != null)
        {
            baseColor = GetComponent<Renderer>()?.material.color ?? Color.white;
        }
    }

    void Update()
    {
        HandleHover();

        if (Input.GetMouseButtonDown(0) && isHovered && !isGrabbed)
            StartGrab();
        if (Input.GetMouseButtonUp(0) && isGrabbed)
            StopGrab();

        // Déplacement du levier
        if (isGrabbed)
        {
            float mouseX = Input.GetAxis("Mouse X");
            positionNormalized += mouseX * sensitivity;
            positionNormalized = Mathf.Clamp01(positionNormalized);
        }
        else
        {
            // Retour automatique au centre ou aux crans
            if (cubeType == CubeType.Direction)
            {
                positionNormalized = Mathf.Lerp(positionNormalized, 0.5f, Time.deltaTime * returnSpeed);
                if (Mathf.Abs(positionNormalized - 0.5f) < snapThreshold)
                    positionNormalized = 0.5f;
            }
            else
            {
                if (Mathf.Abs(positionNormalized - 0f) < snapThreshold) positionNormalized = 0f;
                else if (Mathf.Abs(positionNormalized - 0.5f) < snapThreshold) positionNormalized = 0.5f;
                else if (Mathf.Abs(positionNormalized - 1f) < snapThreshold) positionNormalized = 1f;
            }
        }

        // Appliquer la position sur le rail
        if (railStart != null && railEnd != null)
            transform.position = Vector3.Lerp(railStart.position, railEnd.position, positionNormalized);
    }

    void HandleHover()
    {
        if (playerController == null || playerController.fpsCamera == null) return;

        Camera playerCamera = playerController.fpsCamera;
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        bool hitThisFrame = false;

        if (Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                hitThisFrame = true;
                lastValidHoverTime = Time.time;
            }
        }

        bool shouldBeHovered = hitThisFrame || (Time.time - lastValidHoverTime < hoverMemoryDuration);

        if (!shouldBeHovered)
        {
            Vector3 screenPoint = playerCamera.WorldToScreenPoint(transform.position);
            float distToMouse = Vector2.Distance(Input.mousePosition, screenPoint);
            if (distToMouse < 40f)
            {
                shouldBeHovered = true;
                lastValidHoverTime = Time.time;
            }
        }

        if (shouldBeHovered != isHovered)
        {
            isHovered = shouldBeHovered;
            if (GetComponent<Renderer>() != null)
                GetComponent<Renderer>().material.color = isHovered ? highlightColor : baseColor;
        }
    }

    void StartGrab()
    {
        isGrabbed = true;
        // La caméra reste libre → ne rien bloquer
    }

    void StopGrab()
    {
        isGrabbed = false;
        // La caméra reste libre → ne rien bloquer
    }

    // ---------------- PUBLIC API ----------------

    public float GetLimitedNormalized()
    {
        float limited = positionNormalized;

        if (cubeType == CubeType.Direction && moduleManager != null)
        {
            float factor = Mathf.Lerp(0.2f, 1f, moduleManager.DirectionEfficiency);
            limited = 0.5f + (positionNormalized - 0.5f) * factor;
        }

        return limited;
    }

    public int GetGear()
    {
        if (cubeType == CubeType.Vitesse)
        {
            if (positionNormalized < 0.25f) return 0;
            if (positionNormalized < 0.75f) return 1;
            return 2;
        }
        return 0;
    }
}
