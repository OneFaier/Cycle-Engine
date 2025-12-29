using UnityEngine;
using TMPro;

public class SeatPoint : MonoBehaviour
{
    [Header("Références")]
    public Transform seatTransform;
    public Transform exitPoint;

    [Header("UI")]
    public TextMeshProUGUI enterTextUI;

    [Header("Tous les sièges du véhicule")]
    public SeatPoint[] allSeatPoints;

    private SimpleFPSController playerController;
    private Rigidbody playerRb;
    private Collider playerCollider;

    private bool isSeated = false;
    private int currentSeatIndex = 0;

    // 🔥 Sauvegarde de l'état du joueur
    private Quaternion savedWorldRotation;
    private Transform savedParent;

    private void Start()
    {
        if (allSeatPoints.Length == 0)
            allSeatPoints = new SeatPoint[] { this };
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerController = other.GetComponent<SimpleFPSController>();
        playerRb         = other.GetComponent<Rigidbody>();
        playerCollider   = other.GetComponent<Collider>();

        EnterSeat();
    }

    private void Update()
    {
        if (!isSeated) return;

        // Quitte le siège
        if (Input.GetKeyDown(KeyCode.E))
            ExitSeat();

        // Change de siège
        if (allSeatPoints.Length > 1 && Input.GetKeyDown(KeyCode.B))
            SwitchSeat();
    }

    private void EnterSeat()
    {
        if (!playerController) return;

        isSeated = true;

        // 🔥 Sauvegarde rotation + parent du joueur
        savedWorldRotation = playerController.transform.rotation;
        savedParent        = playerController.transform.parent;

        // Place le joueur dans le siège
        playerController.transform.SetParent(seatTransform);
        playerController.transform.localPosition = Vector3.zero;
        playerController.transform.localRotation = Quaternion.identity;

        // Désactive mouvement
        playerController.canMove = false;

        // Désactive collisions
        if (playerCollider) playerCollider.enabled = false;

        // Désactive physique
        if (playerRb) playerRb.isKinematic = true;

        // Cache le texte
        if (enterTextUI)
            enterTextUI.gameObject.SetActive(false);

        currentSeatIndex = System.Array.IndexOf(allSeatPoints, this);
    }

    private void SwitchSeat()
    {
        allSeatPoints[currentSeatIndex].isSeated = false;

        currentSeatIndex = (currentSeatIndex + 1) % allSeatPoints.Length;
        SeatPoint newSeat = allSeatPoints[currentSeatIndex];

        isSeated = true;
        newSeat.isSeated = true;

        playerController.transform.SetParent(newSeat.seatTransform);
        playerController.transform.localPosition = Vector3.zero;
        playerController.transform.localRotation = Quaternion.identity;

        playerController.canMove = false;

        if (playerCollider) playerCollider.enabled = false;
        if (playerRb) playerRb.isKinematic = true;

        if (enterTextUI)
            enterTextUI.gameObject.SetActive(false);
    }

    private void ExitSeat()
    {
        if (!playerController) return;

        isSeated = false;

        // 🔥 Détache du véhicule
        playerController.transform.SetParent(savedParent);

        // 🔥 Restaure la rotation du monde (plus d’inclinaison)
        Vector3 flatRotation = new Vector3(
            0f,
            savedWorldRotation.eulerAngles.y,
            0f
        );
        playerController.transform.rotation = Quaternion.Euler(flatRotation);

        playerController.canMove = true;

        // Réactive collisions
        if (playerCollider) playerCollider.enabled = true;

        // Réactive physique
        if (playerRb) playerRb.isKinematic = false;

        // Position de sortie
        Vector3 exitPos = exitPoint
            ? exitPoint.position
            : seatTransform.position + seatTransform.right * 2f;

        playerController.transform.position = exitPos;

        if (enterTextUI)
            enterTextUI.gameObject.SetActive(true);
    }
}
