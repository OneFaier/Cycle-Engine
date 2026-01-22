using UnityEngine;
using TMPro;

public class SeatButton : MonoBehaviour
{
    [Header("Éjection")]
    public float backwardForceFactor = 0.6f;
    public float upwardForceFactor = 0.3f;

    [Header("Références")]
    public Transform seatTransform;
    public Transform exitPoint;
    public GameObject player;

    [Header("UI")]
    public TextMeshProUGUI enterTextUI;

    [Header("Bouton d'éjection")]
    public GameObject exitButton;
    public float maxClickDistance = 3f;
    public Color baseColor = Color.white;
    public Color highlightColor = Color.cyan;

    [Header("Éjection")]
    public float exitImpulseForce = 8f;

    private SimpleFPSController playerController;
    private Rigidbody playerRb;
    private Collider playerCollider;

    private bool isSeated = false;

    private Quaternion savedWorldRotation;
    private Transform savedParent;

    private Camera cam;
    private Renderer exitRend;

    // ----- hover memory (exit button)
    private bool exitHovered = false;
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

        if (exitButton)
            exitRend = exitButton.GetComponent<Renderer>();

        if (enterTextUI)
            enterTextUI.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!player || !cam) return;

        HandleExitHover();

        // -------- SORTIE VIA BOUTON (RAYCAST) --------
        if (isSeated && Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance))
            {
                if (hit.collider.gameObject == exitButton)
                {
                    ExitSeat();
                }
            }
        }
    }

    // -------- ENTRÉE AUTO VIA TRIGGER --------
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
        if (!exitButton) return;

        exitHovered = false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance))
        {
            if (hit.collider.gameObject == exitButton)
            {
                exitHovered = true;
                lastExitHoverTime = Time.time;
            }
        }

        if (!exitHovered && Time.time - lastExitHoverTime < hoverMemoryDuration)
            exitHovered = true;

        if (exitRend)
            exitRend.material.color = exitHovered ? highlightColor : baseColor;
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

        if (enterTextUI)
            enterTextUI.gameObject.SetActive(false);
    }

   private void ExitSeat()
   {
       isSeated = false;
   
       playerController.transform.SetParent(savedParent);
   
       // 🔒 TP SÉCURITÉ (SORTIE DU TRIGGER)
       if (exitPoint)
           playerController.transform.position = exitPoint.position;
       else
           playerController.transform.position = seatTransform.position + seatTransform.forward * 2f;
   
       Vector3 flatRotation = new Vector3(0f, savedWorldRotation.eulerAngles.y, 0f);
       playerController.transform.rotation = Quaternion.Euler(flatRotation);
   
       playerController.canMove = true;
       playerController.mouseLookEnabled = true;
   
       if (playerCollider)
           playerCollider.enabled = true;
   
       if (playerRb)
       {
           playerRb.isKinematic = false;
   
           // Reset vitesse pour éviter les restes
           playerRb.linearVelocity = Vector3.zero;
   
           // 🔥 COMBINAISON DES FORCES
           Vector3 backward = -seatTransform.forward * backwardForceFactor;
           Vector3 upward = Vector3.up * upwardForceFactor;
   
           Vector3 ejectDir = (backward + upward).normalized;
   
           playerRb.AddForce(ejectDir * exitImpulseForce, ForceMode.Impulse);
       }
   }



}
