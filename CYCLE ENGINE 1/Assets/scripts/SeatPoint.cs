using UnityEngine;
using TMPro;

public class VehicleSeatPoint : MonoBehaviour
{
    [Header("Références véhicule")]
    public Transform vehicleRoot;
    public Transform seatTransform;
    public Transform exitPoint;

    [Header("Tourelle / Canon")]
    public bool isTurretSeat = false;
    public TurretController turretController; // 👈 Référence vers le script TurretController

    [Header("Touches")]
    public KeyCode enterKey = KeyCode.F;
    public KeyCode exitKey = KeyCode.E;
    public KeyCode switchSeatKey = KeyCode.B;

    [Header("UI")]
    public TextMeshProUGUI enterTextUI;

    private GameObject playerObject;
    private Camera playerCamera;
    private SimpleFPSController playerController;
    private Rigidbody playerRb;

    private Collider[] playerColliders;
    private Collider[] vehicleColliders;

    private bool isSeated = false;
    private bool canEnter = false;

    private VehicleSeatPoint[] allSeatPoints;
    private int currentSeatIndex = -1; // 👈 On initialise à -1 pour gérer le premier "EnterVehicle()"

    private void Start()
    {
        if (vehicleRoot)
            allSeatPoints = vehicleRoot.GetComponentsInChildren<VehicleSeatPoint>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerObject = other.gameObject;
        playerController = playerObject.GetComponent<SimpleFPSController>();
        playerRb = playerObject.GetComponent<Rigidbody>();
        playerCamera = playerObject.GetComponentInChildren<Camera>();

        if (playerController && playerRb)
            canEnter = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        canEnter = false;
        if (enterTextUI) enterTextUI.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (!isSeated && canEnter && Input.GetKeyDown(enterKey))
            EnterVehicle();

        if (isSeated && Input.GetKeyDown(exitKey))
            ExitVehicle();

        if (isSeated && Input.GetKeyDown(switchSeatKey))
            SwitchSeat();

        if (enterTextUI)
            enterTextUI.gameObject.SetActive(canEnter && !isSeated);
    }

    private void EnterVehicle()
    {
        playerController.canMove = false;
        playerRb.isKinematic = true;

        playerObject.transform.SetParent(vehicleRoot);
        playerObject.transform.SetPositionAndRotation(seatTransform.position, seatTransform.rotation);

        playerColliders = playerObject.GetComponentsInChildren<Collider>();
        vehicleColliders = vehicleRoot.GetComponentsInChildren<Collider>();

        foreach (var pCol in playerColliders)
            foreach (var vCol in vehicleColliders)
                Physics.IgnoreCollision(pCol, vCol, true);

        // 🔹 Détermine quel siège est celui-ci
        if (allSeatPoints != null)
        {
            for (int i = 0; i < allSeatPoints.Length; i++)
            {
                if (allSeatPoints[i] == this)
                {
                    currentSeatIndex = i;
                    break;
                }
            }
        }

        // 🔹 Si c’est une tourelle, on active le contrôle
        if (isTurretSeat && turretController)
            turretController.ActivateTurret(playerCamera, true);

        isSeated = true;
        canEnter = false;
        if (enterTextUI) enterTextUI.gameObject.SetActive(false);
    }

    private void ExitVehicle()
    {
        foreach (var pCol in playerColliders)
            foreach (var vCol in vehicleColliders)
                Physics.IgnoreCollision(pCol, vCol, false);

        playerObject.transform.SetParent(null);

        Vector3 exitPos = exitPoint ? exitPoint.position : seatTransform.position + vehicleRoot.right * 2f;
        if (Physics.Raycast(exitPos + Vector3.up * 2f, Vector3.down, out RaycastHit hit, 5f))
            exitPos = hit.point;

        playerObject.transform.SetPositionAndRotation(exitPos, Quaternion.Euler(0, vehicleRoot.eulerAngles.y, 0));

        playerRb.isKinematic = false;
        playerController.canMove = true;

        if (isTurretSeat && turretController)
            turretController.ActivateTurret(null, false);

        isSeated = false;
        canEnter = false;
        if (enterTextUI) enterTextUI.gameObject.SetActive(false);
    }

    private void SwitchSeat()
    {
        if (allSeatPoints == null || allSeatPoints.Length <= 1) return;

        // 🔹 Désactive la tourelle actuelle si active
        if (isTurretSeat && turretController)
            turretController.ActivateTurret(null, false);

        // 🔹 Passe au siège suivant
        currentSeatIndex = (currentSeatIndex + 1) % allSeatPoints.Length;
        VehicleSeatPoint nextSeat = allSeatPoints[currentSeatIndex];

        // 🔹 Change la position du joueur
        playerObject.transform.SetParent(vehicleRoot);
        playerObject.transform.SetPositionAndRotation(nextSeat.seatTransform.position, nextSeat.seatTransform.rotation);

        // 🔹 Si le prochain siège est une tourelle, on l’active
        if (nextSeat.isTurretSeat && nextSeat.turretController)
            nextSeat.turretController.ActivateTurret(playerCamera, true);
    }
}
