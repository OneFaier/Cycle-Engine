using UnityEngine;

public class IndicatorGearMouse : MonoBehaviour
{
    [Header("Rails / Crans")]
    public Transform[] gearCrans;   // Crans pour le levier
    public Transform[] snapPoints;  // Empty à utiliser pour chaque cran
    public int currentGear = 0;

    [Header("Grab / Mouse")]
    public float grabDistance = 3f;
    public float snapThreshold = 0.1f; // distance verticale pour snap au cran

    [Header("Highlight")]
    public Color highlightColor = Color.yellow;

    private Color baseColor;
    private Renderer rend;

    public ModuleResourceManage moduleManager;

    private Camera playerCamera;
    private bool isGrabbed = false;
    private bool isHovered = false;

    void Start()
    {
        playerCamera = Camera.main;
        rend = GetComponent<Renderer>();

        if (rend != null)
            baseColor = rend.material.color;

        if (gearCrans.Length > 0 && currentGear < gearCrans.Length)
            transform.position = gearCrans[currentGear].position;
    }

    void Update()
    {
        HandleHover();

        if (isHovered && Input.GetMouseButtonDown(0) && !isGrabbed)
            isGrabbed = true;

        if (isGrabbed && Input.GetMouseButtonUp(0))
            isGrabbed = false;

        if (isGrabbed)
        {
            HandleGrab();
        }
        else if (gearCrans.Length > 0)
        {
            // Snap fluide vers le cran actuel
            transform.position = Vector3.Lerp(
                transform.position,
                gearCrans[currentGear].position,
                Time.deltaTime * 10f
            );
        }
    }

    void HandleHover()
    {
        Vector3 screenPoint = playerCamera.WorldToScreenPoint(transform.position);
        isHovered = Vector2.Distance(Input.mousePosition, screenPoint) < 40f;

        if (rend != null)
            rend.material.color = isHovered ? highlightColor : baseColor;
    }

    void HandleGrab()
    {
        if (gearCrans.Length == 0 || snapPoints.Length != gearCrans.Length)
            return;

        Plane railPlane = new Plane(Vector3.right, transform.position);
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (!railPlane.Raycast(ray, out float enter))
            return;

        Vector3 hitPoint = ray.GetPoint(enter);

        // Calcul vertical
        float minY = gearCrans[0].position.y;
        float maxY = gearCrans[gearCrans.Length - 1].position.y;

        float t = Mathf.InverseLerp(minY, maxY, hitPoint.y);
        t = Mathf.Clamp01(t);

        float targetY = Mathf.Lerp(minY, maxY, t);

        // Recherche du cran le plus proche
        float closestDist = float.MaxValue;
        int closestIndex = currentGear;

        for (int i = 0; i < gearCrans.Length; i++)
        {
            float dist = Mathf.Abs(targetY - gearCrans[i].position.y);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        Vector3 targetPos;

        // Snap si assez proche
        if (closestDist <= snapThreshold)
        {
            targetPos = snapPoints[closestIndex].position;
            currentGear = closestIndex;
        }
        else
        {
            targetPos = transform.position;
            targetPos.y = targetY;
        }

        transform.position = targetPos;
    }

    // Limitation du cran selon ModuleResourceManage
    public int GetLimitedGear()
    {
        if (moduleManager != null)
        {
            int maxAllowed = Mathf.Clamp(
                moduleManager.SpeedCount + 1,
                1,
                moduleManager.maxBottles + 1
            );

            return Mathf.Min(currentGear, maxAllowed);
        }

        return currentGear;
    }
}
