using UnityEngine;
using TMPro;

public class SeatButton : MonoBehaviour
{
    [Header("Références")]
    public Transform seatTransform;
    public Transform exitPoint;
    public GameObject player;

    [Header("UI")]
    public TextMeshProUGUI enterTextUI;

    [Header("Boutons 3D")]
    public GameObject enterButton;  
    public GameObject exitButton;   
    public float maxClickDistance = 3f;
    public Color baseColor = Color.white;
    public Color highlightColor = Color.cyan;

    private SimpleFPSController playerController;
    private Rigidbody playerRb;
    private Collider playerCollider;

    private bool isSeated = false;
    private Quaternion savedWorldRotation;
    private Transform savedParent;

    private Camera cam;
    private Renderer enterRend;
    private Renderer exitRend;

    // ---------------- hover memory
    private bool enterHovered = false;
    private bool exitHovered = false;
    private float lastEnterHoverTime = -1f;
    private float lastExitHoverTime = -1f;
    private float hoverMemoryDuration = 0.1f;

    private void Start()
    {
        cam = Camera.main;

        if (player)
        {
            playerController = player.GetComponent<SimpleFPSController>();
            playerCollider = player.GetComponent<Collider>();
            playerRb = player.GetComponent<Rigidbody>();
        }

        if (enterButton) enterRend = enterButton.GetComponent<Renderer>();
        if (exitButton) exitRend = exitButton.GetComponent<Renderer>();

        if (enterTextUI) enterTextUI.gameObject.SetActive(true);
    }

    private void Update()
    {
        if (!player) return;

        HandleHover();

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance))
            {
                if (hit.collider.gameObject == enterButton && !isSeated)
                    EnterSeat();
                else if (hit.collider.gameObject == exitButton && isSeated)
                    ExitSeat();
            }
        }

        if (isSeated && Input.GetKeyDown(KeyCode.B))
            SwitchSeat();
    }

    private void HandleHover()
    {
        if (!cam) return;

        enterHovered = false;
        exitHovered = false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance))
        {
            if (hit.collider.gameObject == enterButton)
            {
                enterHovered = true;
                lastEnterHoverTime = Time.time;
            }

            if (hit.collider.gameObject == exitButton)
            {
                exitHovered = true;
                lastExitHoverTime = Time.time;
            }
        }

        // mémoire de hover (comme pour la vitesse)
        if (!enterHovered && Time.time - lastEnterHoverTime < hoverMemoryDuration)
            enterHovered = true;

        if (!exitHovered && Time.time - lastExitHoverTime < hoverMemoryDuration)
            exitHovered = true;

        if (enterRend) enterRend.material.color = enterHovered ? highlightColor : baseColor;
        if (exitRend) exitRend.material.color = exitHovered ? highlightColor : baseColor;
    }

    private void EnterSeat()
    {
        if (!playerController) return;

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

        if (enterTextUI) enterTextUI.gameObject.SetActive(false);
    }

    private void ExitSeat()
    {
        if (!playerController) return;

        isSeated = false;

        playerController.transform.SetParent(savedParent);

        Vector3 flatRotation = new Vector3(0f, savedWorldRotation.eulerAngles.y, 0f);
        playerController.transform.rotation = Quaternion.Euler(flatRotation);

        playerController.canMove = true;
        playerController.mouseLookEnabled = true;

        if (playerCollider) playerCollider.enabled = true;
        if (playerRb) playerRb.isKinematic = false;

        Vector3 exitPos = exitPoint ? exitPoint.position : seatTransform.position + seatTransform.right * 2f;
        playerController.transform.position = exitPos;

        if (enterTextUI) enterTextUI.gameObject.SetActive(true);
    }

    private void SwitchSeat()
    {
        // Optionnel si plusieurs sièges
    }
}
