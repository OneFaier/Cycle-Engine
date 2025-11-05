using UnityEngine;
using TMPro;

public class SeatPoint : MonoBehaviour
{
    [Header("Références")]
    public Transform seatTransform;
    public Transform exitPoint;

    [Header("Tourelle / Canon")]
    public bool isTurretSeat = false;
    public TurretControllerStable turretController;

    [Header("Touches")]
    public KeyCode enterKey = KeyCode.F;
    public KeyCode exitKey = KeyCode.E;
    public KeyCode switchSeatKey = KeyCode.B;

    [Header("UI")]
    public TextMeshProUGUI enterTextUI;

    [Header("Tous les SeatPoints du véhicule")]
    public SeatPoint[] allSeatPoints;

    private SimpleFPSController playerController;
    private bool isSeated = false;
    private bool canEnter = false;
    private int currentSeatIndex = 0;

    private void Start()
    {
        if (allSeatPoints.Length == 0)
            allSeatPoints = new SeatPoint[] { this };

        // Désactive le canon par défaut
        if (turretController != null)
            turretController.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerController = other.GetComponent<SimpleFPSController>();
        if (!playerController) return;

        canEnter = true;
        if (enterTextUI) enterTextUI.gameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        canEnter = false;
        if (enterTextUI) enterTextUI.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (canEnter && !isSeated && Input.GetKeyDown(enterKey))
            EnterSeat();

        if (isSeated)
        {
            if (Input.GetKeyDown(exitKey))
                ExitSeat();

            if (Input.GetKeyDown(switchSeatKey))
                SwitchSeat();
        }
    }

    private void EnterSeat()
    {
        isSeated = true;
        canEnter = false;

        playerController.transform.SetParent(seatTransform);
        playerController.transform.localPosition = Vector3.zero;
        playerController.transform.localRotation = Quaternion.identity;
        playerController.canMove = false;

        Rigidbody rb = playerController.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        if (enterTextUI) enterTextUI.gameObject.SetActive(false);

        // 🔹 Active le script du canon uniquement si c'est un seat canon
        if (isTurretSeat && turretController != null)
            turretController.enabled = true;
    }

    private void ExitSeat()
    {
        isSeated = false;

        playerController.transform.SetParent(null);
        playerController.canMove = true;

        Rigidbody rb = playerController.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;

        Vector3 exitPos = exitPoint ? exitPoint.position : seatTransform.position + seatTransform.right * 2f;
        playerController.transform.position = exitPos;

        // 🔹 Désactive toujours le script canon quand il sort
        if (turretController != null)
            turretController.enabled = false;
    }

    private void SwitchSeat()
    {
        if (allSeatPoints.Length <= 1) return;

        // 🔹 Désactive le canon du seat actuel si c'en est un
        if (isTurretSeat && turretController != null)
            turretController.enabled = false;

        // 🔹 Marque ce seat comme "non occupé"
        isSeated = false;

        // 🔹 Passe au seat suivant
        currentSeatIndex = (currentSeatIndex + 1) % allSeatPoints.Length;
        SeatPoint nextSeat = allSeatPoints[currentSeatIndex];

        // 🔹 Marque le nouveau seat comme "occupé"
        nextSeat.isSeated = true;

        // 🔹 Déplace le joueur
        playerController.transform.SetParent(nextSeat.seatTransform);
        playerController.transform.localPosition = Vector3.zero;
        playerController.transform.localRotation = Quaternion.identity;
        playerController.canMove = false;

        Rigidbody rb = playerController.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        // 🔹 Active le canon du nouveau seat seulement si c'est un seat canon
        if (nextSeat.isTurretSeat && nextSeat.turretController != null)
            nextSeat.turretController.enabled = true;
    }

}
