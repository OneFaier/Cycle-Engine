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

    // Référence dynamique (isPiloting ou canControl)
    private MonoBehaviour vehicleController;
    private System.Reflection.FieldInfo controlField;

    private void Start()
    {
        // Recherche automatique d’un script de véhicule sur la racine
        if (vehicleRoot != null)
        {
            vehicleController = vehicleRoot.GetComponent<MonoBehaviour>();
            if (vehicleController == null)
                vehicleController = vehicleRoot.GetComponentInChildren<MonoBehaviour>();

            if (vehicleController != null)
            {
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

        // Réactive les collisions
        if (playerColliders != null && vehicleColliders != null)
        {
            foreach (var pCol in playerColliders)
                foreach (var vCol in vehicleColliders)
                    Physics.IgnoreCollision(pCol, vCol, false);
        }

        // Détache le joueur
        playerObject.transform.SetParent(null);

        // --- Nouveau système de sortie ---
        Vector3 exitPos;

        if (exitPoint != null)
        {
            exitPos = exitPoint.position;
        }
        else
        {
            // Essaie de le faire sortir à droite du véhicule
            exitPos = seatTransform.position + vehicleRoot.right * 2f;

            // Si bloqué à droite, tente à gauche
            if (Physics.Raycast(seatTransform.position, vehicleRoot.right, out RaycastHit hitRight, 2f))
                exitPos = seatTransform.position - vehicleRoot.right * 2f;
        }

        // Raycast vers le bas pour poser le joueur sur le sol
        if (Physics.Raycast(exitPos + Vector3.up * 2f, Vector3.down, out RaycastHit groundHit, 5f))
            exitPos = groundHit.point;

        // Replace et oriente correctement le joueur
        playerObject.transform.SetPositionAndRotation(exitPos, Quaternion.Euler(0, vehicleRoot.eulerAngles.y, 0));

        // Réactive le joueur
        playerRb.isKinematic = false;
        playerController.canMove = true;

        // Désactive le contrôle du véhicule
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

    // ✅ Optionnel : affichage du point de sortie dans la scène
    private void OnDrawGizmosSelected()
    {
        if (exitPoint)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(exitPoint.position, 0.2f);
        }
    }
}
