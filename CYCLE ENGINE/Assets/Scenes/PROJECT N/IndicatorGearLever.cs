using UnityEngine;

public class IndicatorGearMouse : MonoBehaviour
{
    [Header("Rails / Crans")]
    public Transform[] gearCrans; // positions des crans
    public int currentGear = 0;   // cran actuel

    [Header("Grab / Mouse")]
    public float grabDistance = 3f; // distance max du raycast pour grab
    public float sensitivity = 5f;  // non utilisé ici car grab est direct

    [Header("Highlight")]
    public Color highlightColor = Color.yellow;
    private Color baseColor;
    private Renderer rend;

    private Camera playerCamera;
    private bool isGrabbed = false;
    private bool isHovered = false;

    private float lastValidHoverTime = -1f;
    private float hoverMemoryDuration = 0.1f;

    private Vector3 grabOffset;

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
            StartGrab();

        if (Input.GetMouseButtonUp(0) && isGrabbed)
            StopGrab();

        if (isGrabbed)
            HandleGrab();

        // Snap sur le cran le plus proche
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

        bool shouldBeHovered = hitThisFrame || (Time.time - lastValidHoverTime < hoverMemoryDuration);

        // zone autour du cube pour le hover même si le raycast rate
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
            if (rend != null)
                rend.material.color = isHovered ? highlightColor : baseColor;
        }
    }

    void StartGrab()
    {
        isGrabbed = true;

        // calcule l'offset entre le point hit par le raycast et le cube
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
            grabOffset = transform.position - hit.point;
        else
            grabOffset = Vector3.zero;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void StopGrab()
    {
        isGrabbed = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void HandleGrab()
    {
        // plan vertical pour le rail
        Plane railPlane = new Plane(Vector3.right, transform.position);
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);

        if (railPlane.Raycast(ray, out float enter))
        {
            Vector3 hitPoint = ray.GetPoint(enter) + grabOffset;

            // clamp entre premier et dernier cran
            Vector3 minPos = gearCrans[0].position;
            Vector3 maxPos = gearCrans[gearCrans.Length - 1].position;

            float t = Mathf.InverseLerp(minPos.y, maxPos.y, hitPoint.y);
            t = Mathf.Clamp01(t);

            transform.position = Vector3.Lerp(minPos, maxPos, t);

            // détecte le cran le plus proche
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

    public int GetGear() => currentGear;

    public float GetNormalizedGear()
    {
        return gearCrans.Length > 1 ? currentGear / (float)(gearCrans.Length - 1) : 0f;
    }
}
