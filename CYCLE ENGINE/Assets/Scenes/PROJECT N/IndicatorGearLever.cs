using UnityEngine;

public class IndicatorGearMouse : MonoBehaviour
{
    [Header("Rails / Crans")]
    public Transform[] gearCrans;   // Crans pour le levier (visuel)
    public Transform[] snapPoints;  // Empties pour chaque cran
    public int currentGear = 0;     // Cran officiel

    [Header("Grab / Mouse")]
    public float grabDistance = 3f;
    public float snapThreshold = 0.1f; // distance verticale pour snap au cran

    [Header("Highlight")]
    public Color highlightColor = Color.yellow;

    [Header("Audio")]
    public AudioSource audioSource;     // source pour le son
    public AudioClip gearChangeClip;    // son à chaque empty ou cran

    private Color baseColor;
    private Renderer rend;

    public ModuleResourceManage moduleManager;

    private Camera playerCamera;
    private bool isGrabbed = false;
    private bool isHovered = false;

    private int lastGear = -1;       // pour le cran officiel
    private int lastSnapIndex = -1;  // pour le son à chaque empty
    private int blockedSnapIndex = -1; // snap point où le levier est bloqué

    void Start()
    {
        playerCamera = Camera.main;
        rend = GetComponent<Renderer>();

        if (rend != null)
            baseColor = rend.material.color;

        if (gearCrans.Length > 0 && currentGear < gearCrans.Length)
            transform.position = gearCrans[currentGear].position;

        lastGear = currentGear;
        lastSnapIndex = currentGear;
        blockedSnapIndex = -1;
    }

    void Update()
    {
        HandleHover();

        // Début du grab
        if (isHovered && Input.GetMouseButtonDown(0) && !isGrabbed)
        {
            isGrabbed = true;

            // Relâchement précédent du snap
            if (blockedSnapIndex != -1)
                blockedSnapIndex = -1;
        }

        // Fin du grab manuel
        if (isGrabbed && Input.GetMouseButtonUp(0))
        {
            isGrabbed = false;

            // Si on était bloqué sur un snap, appliquer le cran officiel maintenant
            if (blockedSnapIndex != -1)
            {
                currentGear = blockedSnapIndex;
                blockedSnapIndex = -1;
            }
        }

        if (isGrabbed)
            HandleGrab();
        else if (gearCrans.Length > 0)
        {
            // Lerp fluide vers le cran officiel
            Vector3 targetPos = gearCrans[currentGear].position;
            transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);
        }

        // Jouer son si le cran officiel change
        if (currentGear != lastGear)
        {
            PlayGearSound();
            lastGear = currentGear;
        }
    }

    void HandleHover()
    {
        if (!playerCamera) return;

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

        float minY = gearCrans[0].position.y;
        float maxY = gearCrans[gearCrans.Length - 1].position.y;
        float t = Mathf.InverseLerp(minY, maxY, hitPoint.y);
        t = Mathf.Clamp01(t);
        float targetY = Mathf.Lerp(minY, maxY, t);

        // Calcul du snap point le plus proche
        float closestDist = float.MaxValue;
        int closestIndex = currentGear;
        for (int i = 0; i < snapPoints.Length; i++)
        {
            float dist = Mathf.Abs(targetY - snapPoints[i].position.y);
            if (dist < closestDist)
            {
                closestDist = dist;
                closestIndex = i;
            }
        }

        Vector3 targetPos;

        // Si proche du snap point, bloquer dessus
        if (closestDist <= snapThreshold)
        {
            targetPos = snapPoints[closestIndex].position;

            // Jouer le son si passage sur un empty différent
            if (closestIndex != lastSnapIndex)
            {
                lastSnapIndex = closestIndex;
                PlayGearSound();
            }

            // Bloquer le levier ici jusqu'à relâchement
            blockedSnapIndex = closestIndex;
        }
        else
        {
            // Suivi libre tant qu’on n’est pas proche
            targetPos = transform.position;
            targetPos.y = targetY;
        }

        transform.position = targetPos;
    }

    void PlayGearSound()
    {
        if (audioSource != null && gearChangeClip != null)
            audioSource.PlayOneShot(gearChangeClip);
    }

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
