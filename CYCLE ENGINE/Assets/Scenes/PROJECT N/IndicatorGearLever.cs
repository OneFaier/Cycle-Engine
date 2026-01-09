using UnityEngine;

public class IndicatorGearMouse : MonoBehaviour
{
    [Header("Rails / Crans")]
    public Transform[] gearCrans;
    public int currentGear = 0;

    [Header("Grab / Mouse")]
    public float grabDistance = 3f;

    [Header("Highlight")]
    public Color highlightColor = Color.yellow;
    private Color baseColor;
    private Renderer rend;

    public ModuleResourceManage moduleManager;

    private Camera playerCamera;
    private bool isGrabbed = false;
    private bool isHovered = false;
    private float lastValidHoverTime = -1f;

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

        if (Input.GetMouseButtonDown(0) && isHovered && !isGrabbed)
            isGrabbed = true;
        if (Input.GetMouseButtonUp(0) && isGrabbed)
            isGrabbed = false;

        if (isGrabbed)
            HandleGrab();

        if (!isGrabbed && gearCrans.Length > 0)
            transform.position = Vector3.Lerp(transform.position, gearCrans[currentGear].position, Time.deltaTime * 10f);
    }

    void HandleHover()
    {
        bool hitThisFrame = false;
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, grabDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                hitThisFrame = true;
                lastValidHoverTime = Time.time;
            }
        }

        bool shouldBeHovered = hitThisFrame || (Time.time - lastValidHoverTime < 0.1f);

        if (!shouldBeHovered)
        {
            Vector3 screenPoint = playerCamera.WorldToScreenPoint(transform.position);
            if (Vector2.Distance(Input.mousePosition, screenPoint) < 40f)
            {
                shouldBeHovered = true;
                lastValidHoverTime = Time.time;
            }
        }

        if (shouldBeHovered != isHovered)
        {
            isHovered = shouldBeHovered;
            if (rend != null)
                rend.material.color = isHovered ? highlightColor : baseColor;
        }
    }

    void HandleGrab()
    {
        Plane railPlane = new Plane(Vector3.right, transform.position);
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (railPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter);
            Vector3 minPos = gearCrans[0].position;
            Vector3 maxPos = gearCrans[gearCrans.Length - 1].position;

            float t = Mathf.InverseLerp(minPos.y, maxPos.y, hitPoint.y);
            t = Mathf.Clamp01(t);

            transform.position = Vector3.Lerp(minPos, maxPos, t);

            // Snap au cran le plus proche
            float closestDist = float.MaxValue;
            for (int i = 0; i < gearCrans.Length; i++)
            {
                float dist = Mathf.Abs(transform.position.y - gearCrans[i].position.y);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    currentGear = i;
                }
            }
        }
    }

    public int GetLimitedGear()
    {
        if (moduleManager != null)
        {
            int maxAllowed = Mathf.Clamp(moduleManager.SpeedCount + 1, 1, moduleManager.maxBottles + 1);
            return Mathf.Min(currentGear, maxAllowed);
        }
        return currentGear;
    }
}
