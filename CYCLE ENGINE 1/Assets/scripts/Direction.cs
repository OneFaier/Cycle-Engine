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
    public float maxGrabDistance = 3f; // distance max pour attraper ou survoler

    [Header("Surbrillance")]
    public Color highlightColor = Color.yellow; // couleur au survol
    private Color baseColor;                    // couleur d'origine
    private Renderer rend;                      // renderer du cube

    private Camera playerCamera;
    private bool isGrabbed = false;
    private bool isHovered = false;

    void Start()
    {
        playerCamera = Camera.main;
        rend = GetComponent<Renderer>();
        if (rend != null)
            baseColor = rend.material.color;
    }

    void Update()
    {
        // Vérifie le survol avec raycast
        HandleHover();

        // Commence à attraper le cube
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance))
            {
                if (hit.collider.gameObject == gameObject)
                    isGrabbed = true;
            }
        }

        // Relâche
        if (Input.GetMouseButtonUp(0))
            isGrabbed = false;

        // Déplacer si attrapé
        if (isGrabbed)
        {
            float mouseX = Input.GetAxis("Mouse X");
            positionNormalized += mouseX * sensitivity;
            positionNormalized = Mathf.Clamp01(positionNormalized);
        }

        // Snap automatique si proche des points clés
        if (!isGrabbed)
        {
            if (Mathf.Abs(positionNormalized - 0f) < snapThreshold) positionNormalized = 0f;
            else if (Mathf.Abs(positionNormalized - 0.5f) < snapThreshold) positionNormalized = 0.5f;
            else if (Mathf.Abs(positionNormalized - 1f) < snapThreshold) positionNormalized = 1f;
        }

        // Appliquer la position sur le rail
        transform.position = Vector3.Lerp(railStart.position, railEnd.position, positionNormalized);
    }

    void HandleHover()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance) && hit.collider.gameObject == gameObject)
        {
            if (!isHovered)
            {
                isHovered = true;
                SetHighlight(true);
            }
        }
        else if (isHovered)
        {
            isHovered = false;
            SetHighlight(false);
        }
    }

    void SetHighlight(bool active)
    {
        if (rend == null) return;
        rend.material.color = active ? highlightColor : baseColor;
    }
}
