using UnityEngine;

public class SeatPoint : MonoBehaviour
{
    [Header("Références")]
    public Transform vehicle;        // Véhicule parent
    public Transform seatPosition;   // Position du siège
    public Transform exitPoint;      // Sortie
    public KeyCode exitKey = KeyCode.E;

    private GameObject playerObject;
    private SimpleFPSController playerController;
    private Rigidbody playerRb;
    private bool isSeated = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isSeated) return;
        if (!other.CompareTag("Player")) return;

        playerObject = other.gameObject;
        playerController = playerObject.GetComponent<SimpleFPSController>();
        playerRb = playerObject.GetComponent<Rigidbody>();

        if (playerController == null || playerRb == null) return;

        // --- Désactive le mouvement mais garde la caméra active ---
        playerController.canMove = false;
        playerRb.isKinematic = true;

        // --- Parent au véhicule ---
        playerObject.transform.SetParent(vehicle);

        // --- Teleporte le joueur sur le siège ---
        playerObject.transform.position = seatPosition.position;
        playerObject.transform.rotation = seatPosition.rotation;

        isSeated = true;
    }

    private void Update()
    {
        if (isSeated && Input.GetKeyDown(exitKey))
        {
            ExitSeat();
        }
    }

    private void ExitSeat()
    {
        if (!isSeated || playerObject == null) return;

        // --- Détache du véhicule ---
        playerObject.transform.SetParent(null);

        // --- Replace à l’exit point ---
        playerObject.transform.position = exitPoint != null 
            ? exitPoint.position 
            : seatPosition.position + seatPosition.right * 2f;
        playerObject.transform.rotation = seatPosition.rotation;

        // --- Réactive le mouvement ---
        playerRb.isKinematic = false;
        playerController.canMove = true;

        isSeated = false;
    }
}
