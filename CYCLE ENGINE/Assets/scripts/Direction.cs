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
    [Range(0f, 1f)] public float positionNormalized = 0.5f;
    public float snapThreshold = 0.05f;
    public float maxGrabDistance = 3f;
    public float returnSpeed = 2f;

    [Header("Surbrillance")]
    public Color highlightColor = Color.yellow;
    private Color baseColor;
    private Renderer rend;

    [Header("Référence Joueur")]
    public SimpleFPSController playerController;

    private Camera playerCamera;
    private bool isGrabbed = false;
    private bool isHovered = false;

    // Anti-clignotement
    private float lastValidHoverTime = -1f;
    private float hoverMemoryDuration = 0.1f;

    // 🔒 Gestion du curseur
    private bool cursorWasLockedBefore = false;

    [Header("Aimbot Smooth Settings")]
    public float aimSmoothSpeed = 5f; // plus grand = plus rapide vers le cube

    void Start()
    {
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

        // Déplacement cube
        if (isGrabbed)
        {
            float mouseX = Input.GetAxis("Mouse X");
            positionNormalized += mouseX * sensitivity;
            positionNormalized = Mathf.Clamp01(positionNormalized);

            // 🟢 AIMBOT SMOOTH
            SmoothAimCamera();
        }
        else
        {
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
            SetHighlight(isHovered);
        }
    }

    void SetHighlight(bool active)
    {
        if (rend == null) return;
        rend.material.color = active ? highlightColor : baseColor;
    }

    void StartGrab()
    {
        isGrabbed = true;
        cursorWasLockedBefore = Cursor.lockState == CursorLockMode.Locked;

        // On bloque la souris pour empêcher le look normal
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        if (playerController != null)
            playerController.mouseLookEnabled = false; // désactive le look normal
    }

    void StopGrab()
    {
        isGrabbed = false;
        Cursor.lockState = cursorWasLockedBefore ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = true;

        if (playerController != null)
            playerController.mouseLookEnabled = true; // récupère le look normal
    }

    void SmoothAimCamera()
    {
        if (playerController == null || playerCamera == null) return;

        // Direction vers le cube
        Vector3 targetDir = transform.position - playerCamera.transform.position;

        // Calcul de la rotation cible en angles euler
        Vector3 targetEuler = Quaternion.LookRotation(targetDir).eulerAngles;

        // Convertir en -180..180 pour clamp vertical correctement
        float targetX = targetEuler.x;
        if (targetX > 180f) targetX -= 360f;

        // Clamp vertical selon le maxLookAngle du joueur
        targetX = Mathf.Clamp(targetX, -playerController.maxLookAngle, playerController.maxLookAngle);

        // Récupération rotation actuelle en euler
        Vector3 currentEuler = playerCamera.transform.rotation.eulerAngles;
        float currentX = currentEuler.x;
        if (currentX > 180f) currentX -= 360f;

        // Interpolation X et Y seulement (pas Z)
        float smoothX = Mathf.Lerp(currentX, targetX, Time.deltaTime * aimSmoothSpeed);
        float smoothY = Mathf.LerpAngle(currentEuler.y, targetEuler.y, Time.deltaTime * aimSmoothSpeed);

        playerCamera.transform.rotation = Quaternion.Euler(smoothX, smoothY, 0f);
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
