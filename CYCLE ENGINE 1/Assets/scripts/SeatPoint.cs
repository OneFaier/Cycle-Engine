using UnityEngine;
using TMPro;

public class VehicleSeatPoint : MonoBehaviour
{
    [Header("Références véhicule")]
    public Transform vehicleRoot;
    public Transform seatTransform;
    public Transform exitPoint;

    [Header("Touches")]
    public KeyCode enterKey = KeyCode.F;
    public KeyCode exitKey = KeyCode.E;

    [Header("UI")]
    public TextMeshProUGUI enterTextUI;

    private GameObject playerObject;
    private SimpleFPSController playerController;
    private Rigidbody playerRb;

    private Collider[] playerColliders;
    private Collider[] vehicleColliders;

    private bool isSeated = false;
    private bool canEnter = false;

    // Référence dynamique (s’adapte à n’importe quel script de véhicule)
    private MonoBehaviour vehicleController;
    private System.Reflection.FieldInfo controlField;

    private void Start()
    {
        // Recherche automatique d’un script de véhicule sur la racine
        if (vehicleRoot != null)
        {
            vehicleController = vehicleRoot.GetComponent<MonoBehaviour>();
            if (vehicleController == null)
            {
                // Cherche dans les enfants au cas où
                vehicleController = vehicleRoot.GetComponentInChildren<MonoBehaviour>();
            }

            if (vehicleController != null)
            {
                // Cherche un champ commun : isPiloting ou canControl
                controlField = vehicleController.GetType().GetField("isPiloting") ??
                               vehicleController.GetType().GetField("canControl");
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerObject = other.gameObject;
        playerController = playerObject.GetComponent<SimpleFPSController>();
        playerRb = playerObject.GetComponent<Rigidbody>();

        if (playerController && playerRb && vehicleController)
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
        if (!vehicleController) return;

        if (!isSeated && canEnter && Input.GetKeyDown(enterKey))
            EnterVehicle();

        if (isSeated && Input.GetKeyDown(exitKey))
            ExitVehicle();

        if (enterTextUI)
            enterTextUI.gameObject.SetActive(canEnter && !isSeated);
    }

    private void EnterVehicle()
    {
        if (!playerObject || !vehicleController) return;

        // Bloque les contrôles du joueur
        playerController.canMove = false;
        playerRb.isKinematic = true;

        // Place le joueur sur le siège
        playerObject.transform.SetParent(vehicleRoot);
        playerObject.transform.SetPositionAndRotation(seatTransform.position, seatTransform.rotation);

        // Active le contrôle du véhicule
        SetVehicleControl(true);

        // Ignore collisions joueur ↔ véhicule
        playerColliders = playerObject.GetComponentsInChildren<Collider>();
        vehicleColliders = vehicleRoot.GetComponentsInChildren<Collider>();
        foreach (var pCol in playerColliders)
            foreach (var vCol in vehicleColliders)
                Physics.IgnoreCollision(pCol, vCol, true);

        isSeated = true;
        canEnter = false;
        if (enterTextUI) enterTextUI.gameObject.SetActive(false);
    }

    private void ExitVehicle()
    {
        if (!playerObject || !vehicleController) return;

        // Réactive collisions
        if (playerColliders != null && vehicleColliders != null)
            foreach (var pCol in playerColliders)
                foreach (var vCol in vehicleColliders)
                    Physics.IgnoreCollision(pCol, vCol, false);

        // Détache le joueur et replace au point de sortie
        playerObject.transform.SetParent(null);
        Vector3 exitPos = exitPoint ? exitPoint.position : seatTransform.position + seatTransform.right * 2f;
        playerObject.transform.SetPositionAndRotation(exitPos, seatTransform.rotation);

        // Réactive le joueur
        playerRb.isKinematic = false;
        playerController.canMove = true;

        // Désactive le contrôle véhicule
        SetVehicleControl(false);

        isSeated = false;
        canEnter = false;
        if (enterTextUI) enterTextUI.gameObject.SetActive(false);
    }

    private void SetVehicleControl(bool state)
    {
        if (vehicleController == null || controlField == null) return;

        // Affecte le champ trouvé (isPiloting ou canControl)
        controlField.SetValue(vehicleController, state);
    }
}
