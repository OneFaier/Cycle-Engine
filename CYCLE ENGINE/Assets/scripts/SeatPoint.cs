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

        // Change de siège avec B
        if (allSeatPoints.Length > 1 && Input.GetKeyDown(KeyCode.B))
            SwitchSeat();
    }

    private void EnterSeat()
    {
        if (!playerController) return;

        isSeated = true;

        // Place le joueur dans le siège
        playerController.transform.SetParent(seatTransform);
        playerController.transform.localPosition = Vector3.zero;
        playerController.transform.localRotation = Quaternion.identity;

        // Désactive mouvement
        playerController.canMove = false;

        // 🔥 Désactive collisions du joueur
        if (playerCollider) playerCollider.enabled = false;

        // 🔥 Physique désactivée
        if (playerRb) playerRb.isKinematic = true;

        // Cache le texte
        if (enterTextUI)
            enterTextUI.gameObject.SetActive(false);

        // Définit le siège actuel
        currentSeatIndex = System.Array.IndexOf(allSeatPoints, this);
    }

    private void SwitchSeat()
    {
        // Quitte l'ancien siège
        allSeatPoints[currentSeatIndex].isSeated = false;

        // Choisis le prochain siège
        currentSeatIndex = (currentSeatIndex + 1) % allSeatPoints.Length;
        SeatPoint newSeat = allSeatPoints[currentSeatIndex];

        // Assure que le joueur est toujours en "mode assis"
        isSeated = true;
        newSeat.isSeated = true;

        // Replace le joueur sur le nouveau siège
        playerController.transform.SetParent(newSeat.seatTransform);
        playerController.transform.localPosition = Vector3.zero;
        playerController.transform.localRotation = Quaternion.identity;

        // Toujours pas de mouvements
        playerController.canMove = false;

        // 🔥 Continue de désactiver collisions
        if (playerCollider) playerCollider.enabled = false;

        // 🔥 Continue de bloquer la physique
        if (playerRb) playerRb.isKinematic = true;

        if (enterTextUI)
            enterTextUI.gameObject.SetActive(false);
    }

    private void ExitSeat()
    {
        if (!playerController) return;

        isSeated = false;

        // Libère le joueur
        playerController.transform.SetParent(null);
        playerController.canMove = true;

        // 🔥 Réactive collisions
        if (playerCollider) playerCollider.enabled = true;

        // 🔥 Réactive physique
        if (playerRb) playerRb.isKinematic = false;

        // Position de sortie
        Vector3 exitPos = exitPoint ?
            exitPoint.position :
            seatTransform.position + seatTransform.right * 2f;

        playerController.transform.position = exitPos;

        if (enterTextUI)
            enterTextUI.gameObject.SetActive(true);
    }
}
