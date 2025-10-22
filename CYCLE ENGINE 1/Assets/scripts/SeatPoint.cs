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
    public GameObject overH;
    private ENGINEMovements overH_script;
    
    void Start()
    {
        overH_script = overH.GetComponent<ENGINEMovements>(); 
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isSeated) return;
        {
            overH_script.hoverHeight = 4f;
        }
        if (!other.CompareTag("Player")) return;

        playerObject = other.gameObject;
        playerController = playerObject.GetComponent<SimpleFPSController>();
        playerRb = playerObject.GetComponent<Rigidbody>();

        if (playerController == null || playerRb == null) return;

        
        playerController.canMove = false;
        playerRb.isKinematic = true;
        

        //Parent au véhicule 
        playerObject.transform.SetParent(vehicle);

        //Teleporte le joueur sur le siège 
        playerObject.transform.position = seatPosition.position;
        playerObject.transform.rotation = seatPosition.rotation;

        isSeated = true;
        
        
    }

    private void Update()
    {
        if (isSeated && Input.GetKeyDown(exitKey))
        {
            overH_script.hoverHeight = 2f;
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
