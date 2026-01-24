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

    [Header("Camera Follow")]
    public Camera playerCamera;
    public float cameraSmoothSpeed = 5f;

    private bool isGrabbed = false;
    private bool isHovered = false;

    private float lastValidHoverTime = -1f;
    private float hoverMemoryDuration = 0.1f;

    void Start()
    {
        if (playerCamera == null)
            playerCamera = playerController?.fpsCamera ?? Camera.main;

        rend = GetComponent<Renderer>();
        if (rend != null)
            baseColor = rend.material.color;
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

            // Smooth aim caméra
            SmoothAimAtLever();
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
        transform.position = Vector3.Lerp(railStart.position, railEnd.position, positionNormalized);
    }

    void HandleHover()
    {
        if (!playerCamera) return;

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
            if (rend != null)
                rend.material.color = isHovered ? highlightColor : baseColor;
        }
    }

    void StartGrab()
    {
        isGrabbed = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        if (playerController != null)
            playerController.mouseLookEnabled = false;
    }

    void StopGrab()
    {
        isGrabbed = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        if (playerController != null)
            playerController.mouseLookEnabled = true;

        // Ici la caméra reste où elle est, pas de snap
    }

    void SmoothAimAtLever()
    {
        if (!playerCamera) return;

        Vector3 direction = (transform.position - playerCamera.transform.position).normalized;
        Quaternion targetRotation = Quaternion.LookRotation(direction);

        playerCamera.transform.rotation = Quaternion.Slerp(
            playerCamera.transform.rotation,
            targetRotation,
            Time.deltaTime * cameraSmoothSpeed
        );
    }

    // ---------------- PUBLIC API ----------------

    // Valeur normalisée de direction limitée par les bouteilles
    public float GetLimitedNormalized()
    {
        float limited = positionNormalized;

        if (cubeType == CubeType.Direction && moduleManager != null)
        {
            float factor = Mathf.Lerp(0.2f, 1f, moduleManager.DirectionEfficiency); // 0.2 si aucune bouteille
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
