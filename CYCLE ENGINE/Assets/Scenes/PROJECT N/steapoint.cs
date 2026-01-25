using UnityEngine;
using TMPro;

public class SeatButton : MonoBehaviour
{
    [Header("Éjection")]
    public float backwardForceFactor = 0.6f;
    public float upwardForceFactor = 0.3f;
    public float exitImpulseForce = 8f;

    [Header("Références")]
    public Transform seatTransform;
    public Transform exitPoint;
    public GameObject player;

    [Header("UI")]
    public TextMeshProUGUI enterTextUI;
    public GameObject exitButton;

    [Header("Couleurs")]
    public Color baseColor = Color.white;
    public Color highlightColor = Color.cyan;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip sitClip;
    public AudioClip exitClip;

    // ---------------- PRIVATE ----------------
    private SimpleFPSController playerController;
    private Rigidbody playerRb;
    private Collider playerCollider;

    private bool isSeated = false;
    private Quaternion savedWorldRotation;
    private Transform savedParent;
    private Renderer exitRend;
    private Camera cam;

    // Hover memory
    private bool exitHovered = false;
    private bool isHovered = false;
    private float lastExitHoverTime = -1f;
    private float hoverMemoryDuration = 0.1f;
    private float maxClickDistance = 3f; // distance pour raycast

    private Vector3 originalScale = Vector3.one;

    private void Start()
    {
        cam = Camera.main;

        if (player)
        {
            playerController = player.GetComponent<SimpleFPSController>();
            playerCollider = player.GetComponent<Collider>();
            playerRb = player.GetComponent<Rigidbody>();
            originalScale = player.transform.localScale; // mémoriser l’échelle originale
        }

        if (exitButton)
        {
            exitRend = exitButton.GetComponent<Renderer>();
            exitButton.SetActive(false);
        }

        if (enterTextUI)
            enterTextUI.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!player || !cam) return;

        HandleExitHover();

        // Sortie via bouton si hover + clic
        if (isSeated && isHovered && Input.GetMouseButtonDown(0))
        {
            ExitSeat();
        }

        // Affichage du bouton uniquement si assis
        if (exitButton)
            exitButton.SetActive(isSeated);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isSeated || !playerController) return;

        if (other.gameObject == player)
        {
            EnterSeat();
        }
    }

    private void HandleExitHover()
    {
        if (!exitButton || !cam) return;

        bool hitThisFrame = false;

        // 1️⃣ Raycast physique
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance))
        {
            if (hit.collider.gameObject == exitButton)
            {
                hitThisFrame = true;
                lastExitHoverTime = Time.time;
            }
        }

        // 2️⃣ Hover memory
        bool shouldBeHovered = hitThisFrame || (Time.time - lastExitHoverTime < hoverMemoryDuration);

        // 3️⃣ Distance écran
        if (!shouldBeHovered)
        {
            Vector3 screenPoint = cam.WorldToScreenPoint(exitButton.transform.position);
            float distToMouse = Vector2.Distance(Input.mousePosition, screenPoint);
            if (distToMouse < 40f)
            {
                shouldBeHovered = true;
                lastExitHoverTime = Time.time;
            }
        }

        // 4️⃣ Appliquer couleur
        if (shouldBeHovered != exitHovered)
        {
            exitHovered = shouldBeHovered;
            if (exitRend)
                exitRend.material.color = exitHovered ? highlightColor : baseColor;
        }

        isHovered = exitHovered;
    }

    private void EnterSeat()
    {
        isSeated = true;
        savedWorldRotation = playerController.transform.rotation;
        savedParent = playerController.transform.parent;

        playerController.transform.SetParent(seatTransform);
        playerController.transform.localPosition = Vector3.zero;
        playerController.transform.localRotation = Quaternion.identity;

        playerController.canMove = false;
        playerController.mouseLookEnabled = true;

        if (playerCollider) playerCollider.enabled = false;
        if (playerRb) playerRb.isKinematic = true;

        // 🔹 Changer l’échelle à 0.4
        playerController.transform.localScale = Vector3.one * 0.4f;

        if (playerController.footstepSource != null)
            playerController.footstepSource.enabled = false;

        playerController.seatTransform = seatTransform;

        if (enterTextUI)
            enterTextUI.gameObject.SetActive(false);

        if (audioSource != null && sitClip != null)
            audioSource.PlayOneShot(sitClip, 1f);
    }

    public void ExitSeat()
    {
        isSeated = false;

        playerController.transform.SetParent(savedParent);

        if (exitPoint)
            playerController.transform.position = exitPoint.position;
        else
            playerController.transform.position = seatTransform.position + seatTransform.forward * 2f;

        Vector3 flatRotation = new Vector3(0f, savedWorldRotation.eulerAngles.y, 0f);
        playerController.transform.rotation = Quaternion.Euler(flatRotation);

        playerController.canMove = true;
        playerController.mouseLookEnabled = true;

        if (playerCollider) playerCollider.enabled = true;

        if (playerRb)
        {
            playerRb.isKinematic = false;
#if UNITY_6000_0_OR_NEWER
            playerRb.linearVelocity = Vector3.zero;
#else
            playerRb.velocity = Vector3.zero;
#endif

            Vector3 backward = -seatTransform.forward * backwardForceFactor;
            Vector3 upward = Vector3.up * upwardForceFactor;
            Vector3 ejectDir = (backward + upward).normalized;

            playerRb.AddForce(ejectDir * exitImpulseForce, ForceMode.Impulse);
        }

        // 🔹 Restaurer l’échelle originale
        playerController.transform.localScale = originalScale;

        if (playerController.footstepSource != null)
            playerController.footstepSource.enabled = true;

        playerController.seatTransform = null;

        if (audioSource != null && exitClip != null)
            audioSource.PlayOneShot(exitClip, 1f);
    }
}
