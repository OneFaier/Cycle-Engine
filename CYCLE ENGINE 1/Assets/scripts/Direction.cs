using UnityEngine;

public class IndicatorMouseClickFast : MonoBehaviour
{
    [Header("Rail")]
    public Transform railStart;
    public Transform railEnd;

    [Header("Contrôles")]
    public float sensitivity = 0.05f;
    [Range(0f, 1f)] public float positionNormalized = 0.5f;
    public float snapThreshold = 0.05f;
    public float maxGrabDistance = 3f;

    [Header("Surbrillance")]
    public Color highlightColor = Color.yellow;
    private Color baseColor;
    private Renderer rend;

    private Camera playerCamera;
    private bool isGrabbed = false;
    private bool isHovered = false;

    // 🔧 Anti-clignotement
    private float lastValidHoverTime = -1f;
    private float hoverMemoryDuration = 0.1f; // temps de "mémoire" du survol

    void Start()
    {
        playerCamera = Camera.main;
        rend = GetComponent<Renderer>();
        if (rend != null)
            baseColor = rend.material.color;
    }

    void Update()
    {
        HandleHover();

        // Commencer le grab
        if (Input.GetMouseButtonDown(0) && isHovered)
            isGrabbed = true;

        // Relâcher
        if (Input.GetMouseButtonUp(0))
            isGrabbed = false;

        // Déplacement si attrapé
        if (isGrabbed)
        {
            float mouseX = Input.GetAxis("Mouse X");
            positionNormalized += mouseX * sensitivity;
            positionNormalized = Mathf.Clamp01(positionNormalized);
        }

        // Snap auto
        if (!isGrabbed)
        {
            if (Mathf.Abs(positionNormalized - 0f) < snapThreshold) positionNormalized = 0f;
            else if (Mathf.Abs(positionNormalized - 0.5f) < snapThreshold) positionNormalized = 0.5f;
            else if (Mathf.Abs(positionNormalized - 1f) < snapThreshold) positionNormalized = 1f;
        }

        // Position sur le rail
        transform.position = Vector3.Lerp(railStart.position, railEnd.position, positionNormalized);
    }

    void HandleHover()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        bool hitThisFrame = false;

        // Raycast classique
        if (Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                hitThisFrame = true;
                lastValidHoverTime = Time.time;
            }
        }

        // 🔄 Hover “persistant” pendant un court instant
        bool shouldBeHovered = hitThisFrame || (Time.time - lastValidHoverTime < hoverMemoryDuration);

        // 🔍 Si pas de hit, vérifie la distance à la ligne de visée
        if (!shouldBeHovered)
        {
            Vector3 screenPoint = playerCamera.WorldToScreenPoint(transform.position);
            float distToMouse = Vector2.Distance(Input.mousePosition, screenPoint);
            if (distToMouse < 40f) // pixels tolérés
            {
                shouldBeHovered = true;
                lastValidHoverTime = Time.time;
            }
        }

        // ✅ Changement d’état stable
        if (shouldBeHovered != isHovered)
        {
            isHovered = shouldBeHovered;
            SetHighlight(isHovered);
        }
    }

    void SetHighlight(bool active)
    {
        if (rend == null) return;
        rend.material.color = active ? highlightColor : baseColor;
    }
}
