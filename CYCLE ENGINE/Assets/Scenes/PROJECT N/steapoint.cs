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
    public SimpleFPSController playerController; // ← à forcer dans l’inspector
    public Camera playerCamera; // ← à forcer dans l’inspector

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

    private Rigidbody playerRb;
    private Collider playerCollider;
    private Renderer exitRend;

    private bool isSeated = false;
    private Quaternion savedWorldRotation;
    private Transform savedParent;
    private Vector3 originalScale = Vector3.one;

    // Hover memory
    private bool exitHovered = false;
    private float lastHoverTime = -1f;
    private float hoverMemoryDuration = 0.1f;
    private float maxClickDistance = 100f;

    private void Start()
    {
        if (!playerController && player != null)
            playerController = player.GetComponent<SimpleFPSController>();

        if (playerController)
        {
            playerCollider = player.GetComponent<Collider>();
            playerRb = player.GetComponent<Rigidbody>();
            originalScale = player.transform.localScale;
            if (!playerCamera) playerCamera = playerController.fpsCamera;
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
        if (!playerController || !playerCamera) return;

        HandleExitHover();

        // Cliquer pour sortir
        if (isSeated && exitHovered && Input.GetMouseButtonDown(0))
        {
            ExitSeat();
        }

        if (exitButton)
            exitButton.SetActive(isSeated);
    }

    private void HandleExitHover()
    {
        if (!exitButton || !playerCamera) return;

        bool hitThisFrame = false;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance))
        {
            if (hit.collider.gameObject == exitButton)
            {
                hitThisFrame = true;
                lastHoverTime = Time.time;
            }
        }

        bool shouldBeHovered = hitThisFrame || (Time.time - lastHoverTime < hoverMemoryDuration);

        if (!shouldBeHovered)
        {
            Vector3 screenPoint = playerCamera.WorldToScreenPoint(exitButton.transform.position);
            float distToMouse = Vector2.Distance(Input.mousePosition, screenPoint);
            if (distToMouse < 40f)
            {
                shouldBeHovered = true;
                lastHoverTime = Time.time;
            }
        }

        if (shouldBeHovered != exitHovered)
        {
            exitHovered = shouldBeHovered;
            if (exitRend)
                exitRend.material.color = exitHovered ? highlightColor : baseColor;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isSeated || !playerController) return;

        if (other.gameObject == player)
            EnterSeat();
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

        playerController.transform.localScale = originalScale;

        if (playerController.footstepSource != null)
            playerController.footstepSource.enabled = true;

        playerController.seatTransform = null;

        if (audioSource != null && exitClip != null)
            audioSource.PlayOneShot(exitClip, 1f);
    }
}
