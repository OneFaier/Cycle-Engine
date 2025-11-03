using UnityEngine;
using TMPro;

public class SeatPoint : MonoBehaviour
{
    [Header("Références")]
    public Transform vehicle;              // Racine du véhicule
    public Transform seatPosition;         // Position d'assise
    public Transform exitPoint;            // Point de sortie
    public KeyCode enterKey = KeyCode.F;   // Touche pour entrer
    public KeyCode exitKey = KeyCode.E;    // Touche pour sortir

    [Header("Contrôle véhicule")]
    public ENGINEMovements vehicleController; // Script de conduite

    [Header("UI")]
    public TextMeshProUGUI enterTextUI; // TMP qui s'affiche quand le joueur peut entrer

    private GameObject playerObject;
    private SimpleFPSController playerController;
    private Rigidbody playerRb;
    private bool isSeated = false;
    private bool canEnter = false;

    // Pour gérer l'ignoreCollision
    private Collider[] playerColliders;
    private Collider[] vehicleColliders;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerObject = other.gameObject;
        playerController = playerObject.GetComponent<SimpleFPSController>();
        playerRb = playerObject.GetComponent<Rigidbody>();

        if (playerController != null && playerRb != null)
            canEnter = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        canEnter = false;
    }

    private void Update()
    {
        // --- Entrer dans le véhicule ---
        if (!isSeated && canEnter && Input.GetKeyDown(enterKey))
            SitPlayer();

        // --- Sortir du véhicule ---
        if (isSeated && Input.GetKeyDown(exitKey))
            ExitSeat();

        // --- TMP visibilité ---
        if (enterTextUI != null)
            enterTextUI.gameObject.SetActive(canEnter && !isSeated);
    }

    private void SitPlayer()
    {
        if (playerObject == null || vehicleController == null) return;

        // Désactive mouvements FPS
        playerController.canMove = false;
        playerRb.isKinematic = true;

        // Parent et positionnement
        playerObject.transform.SetParent(vehicle);
        playerObject.transform.SetPositionAndRotation(seatPosition.position, seatPosition.rotation);

        // Active le contrôle et la physique de la voiture
        vehicleController.canControl = true;
        vehicleController.rb.isKinematic = false;

        // Désactive collisions joueur ↔ véhicule
        playerColliders = playerObject.GetComponentsInChildren<Collider>();
        vehicleColliders = vehicle.GetComponentsInChildren<Collider>();
        foreach (Collider pCol in playerColliders)
        {
            foreach (Collider vCol in vehicleColliders)
            {
                Physics.IgnoreCollision(pCol, vCol, true);
            }
        }

        isSeated = true;
        canEnter = false;
    }

    private void ExitSeat()
    {
        if (playerObject == null) return;

        // Réactive collisions joueur ↔ véhicule
        if (playerColliders != null && vehicleColliders != null)
        {
            foreach (Collider pCol in playerColliders)
            {
                foreach (Collider vCol in vehicleColliders)
                {
                    Physics.IgnoreCollision(pCol, vCol, false);
                }
            }
        }

        // Détache du véhicule
        playerObject.transform.SetParent(null);

        // Position de sortie
        Vector3 exitPos = exitPoint ? exitPoint.position : seatPosition.position + seatPosition.right * 2f;
        playerObject.transform.SetPositionAndRotation(exitPos, seatPosition.rotation);

        // Réactive FPS
        playerRb.isKinematic = false;
        playerController.canMove = true;

        // Désactive contrôle véhicule et bloque la physique
        vehicleController.canControl = false;
        vehicleController.rb.isKinematic = true;

        isSeated = false;
        canEnter = false;
    }
}
