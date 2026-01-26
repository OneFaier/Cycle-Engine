using UnityEngine;

public class IndicatorGearMouse : MonoBehaviour
{
    [Header("Rails / Crans")]
    public Transform[] gearCrans;
    public Transform[] snapPoints;
    public int currentGear = 0;

    [Header("Grab / Mouse")]
    public float grabDistance = 3f;
    public float snapZone = 0.2f;
    public float snapCooldown = 0.1f;
    public int maxSnapsPerGrab = 2;
    public float grabMoveThreshold = 5f;

    [Header("Highlight")]
    public Color highlightColor = Color.yellow;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip gearChangeClip;

    [Header("Snap Indicators")]
    public Renderer[] snapIndicators;
    public Color passedColor = Color.green;
    public Color notPassedColor = Color.red;

    [Header("Référence Joueur / Caméra")]
    public SimpleFPSController playerController;
    public Camera targetCamera;
    [Range(0f, 1f)] public float smoothAimFactor = 0.1f;

    [Header("Module Manager")]
    public ModuleResourceManage moduleManager;

    [Header("Aim Zone")]
    public float maxYawOffset = 30f;
    public float maxPitchOffset = 20f;

    private Renderer rend;
    private Color baseColor;

    private bool isGrabbed = false;
    private bool isHovered = false;

    private int lastGear = -1;
    private int lastSnapIndex = -1;
    private int grabStartIndex = 0;
    private bool grabActivated = false;
    private Vector3 grabStartMousePos;
    private float lastSnapTime = -1f;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend) baseColor = rend.material.color;

        if (gearCrans.Length > 0 && currentGear < gearCrans.Length)
            transform.position = gearCrans[currentGear].position;

        lastGear = currentGear;
        lastSnapIndex = currentGear;

        if (!targetCamera && playerController) targetCamera = playerController.fpsCamera;
    }

    void Update()
    {
        if (!targetCamera) return;

        HandleHover();

        if (isHovered && Input.GetMouseButtonDown(0) && !isGrabbed)
            StartGrab();
        if (isGrabbed && Input.GetMouseButtonUp(0))
            StopGrab();

        if (isGrabbed)
        {
            if (!grabActivated)
            {
                float dist = Vector2.Distance(Input.mousePosition, grabStartMousePos);
                if (dist >= grabMoveThreshold) grabActivated = true;
            }

            if (grabActivated)
            {
                HandleGrab();
                SmoothAimAtLever();
            }
        }
        else if (gearCrans.Length > 0)
        {
            transform.position = Vector3.Lerp(transform.position, gearCrans[currentGear].position, Time.deltaTime * 10f);
        }

        if (currentGear != lastGear)
        {
            PlayGearSound();
            lastGear = currentGear;
        }

        UpdateSnapIndicators();
    }

    void HandleHover()
    {
        Vector3 screenPoint = targetCamera.WorldToScreenPoint(transform.position);
        isHovered = Vector2.Distance(Input.mousePosition, screenPoint) < 40f;
        if (rend) rend.material.color = isHovered ? highlightColor : baseColor;
    }

    void StartGrab()
    {
        isGrabbed = true;
        grabActivated = false;
        grabStartMousePos = Input.mousePosition;
        grabStartIndex = currentGear;

        if (snapPoints.Length > 0 && currentGear < snapPoints.Length)
        {
            transform.position = snapPoints[currentGear].position;
            lastSnapIndex = currentGear;
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerController) playerController.mouseLookEnabled = true;
    }

    void StopGrab()
    {
        isGrabbed = false;
        grabActivated = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController) playerController.mouseLookEnabled = true;
    }

    void HandleGrab()
    {
        if (gearCrans.Length == 0 || snapPoints.Length != gearCrans.Length) return;

        Plane railPlane = new Plane(Vector3.right, transform.position);
        Ray ray = targetCamera.ScreenPointToRay(Input.mousePosition);
        if (!railPlane.Raycast(ray, out float enter)) return;

        Vector3 hitPoint = ray.GetPoint(enter);
        int minIndex = Mathf.Max(grabStartIndex - maxSnapsPerGrab, 0);
        int maxIndex = Mathf.Min(grabStartIndex + maxSnapsPerGrab, snapPoints.Length - 1);

        int closestIndex = lastSnapIndex;
        float closestDist = float.MaxValue;

        for (int i = minIndex; i <= maxIndex; i++)
        {
            float dist = Mathf.Abs(hitPoint.y - snapPoints[i].position.y);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        float time = Time.time;
        float distToCurrent = Mathf.Abs(hitPoint.y - snapPoints[lastSnapIndex].position.y);

        if (closestIndex != lastSnapIndex && distToCurrent > snapZone && time - lastSnapTime > snapCooldown)
        {
            transform.position = snapPoints[closestIndex].position;
            lastSnapIndex = closestIndex;
            currentGear = closestIndex;
            PlayGearSound();
            lastSnapTime = time;
        }
        else
        {
            transform.position = snapPoints[lastSnapIndex].position;
        }
    }

    void UpdateSnapIndicators()
    {
        if (snapIndicators == null || snapIndicators.Length == 0) return;

        int maxUsefulGear = snapIndicators.Length - 1;
        if (moduleManager != null)
            maxUsefulGear = moduleManager.MaxGearAllowed - 1;

        for (int i = 0; i < snapIndicators.Length; i++)
        {
            if (!snapIndicators[i]) continue;
            snapIndicators[i].material.color = i <= maxUsefulGear ? passedColor : notPassedColor;
        }
    }

    void SmoothAimAtLever()
    {
        if (!targetCamera) return;

        Vector3 dir = (transform.position - targetCamera.transform.position).normalized;
        Quaternion targetRot = Quaternion.LookRotation(dir);

        Vector3 cur = targetCamera.transform.rotation.eulerAngles;
        Vector3 tgt = targetRot.eulerAngles;

        float yaw = cur.y + Mathf.Clamp(Mathf.DeltaAngle(cur.y, tgt.y), -maxYawOffset, maxYawOffset) * smoothAimFactor;
        float pitch = cur.x + Mathf.Clamp(Mathf.DeltaAngle(cur.x, tgt.x), -maxPitchOffset, maxPitchOffset) * smoothAimFactor;

        targetCamera.transform.rotation = Quaternion.Euler(pitch, yaw, 0f);
    }

    void PlayGearSound()
    {
        if (audioSource && gearChangeClip)
            audioSource.PlayOneShot(gearChangeClip);
    }

    public int GetLimitedGear()
    {
        if (moduleManager != null)
        {
            int maxAllowed = moduleManager.MaxGearAllowed;
            return Mathf.Min(currentGear, maxAllowed - 1); // indices 0-based
        }
        return currentGear;
    }
}
