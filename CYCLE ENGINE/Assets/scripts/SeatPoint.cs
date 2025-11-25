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
        playerRb = other.GetComponent<Rigidbody>();
        playerCollider = other.GetComponent<Collider>();

        EnterSeat();
    }

    private void Update()
    {
        if (!isSeated) return;

        // Quitte le siège
        if (Input.GetKeyDown(KeyCode.E))
            ExitSeat();

        // 🔥 Change de siège avec B
        if (allSeatPoints.Length > 1 && Input.GetKeyDown(KeyCode.B))
            SwitchSeat();
    }

    private void EnterSeat()
    {
        if (!playerController) return;

        isSeated = true;

        playerController.transform.SetParent(seatTransform);
        playerController.transform.localPosition = Vector3.zero;
        playerController.transform.localRotation = Quaternion.identity;

        playerController.canMove = false;

        if (playerRb) playerRb.isKinematic = true;
        if (playerCollider) playerCollider.enabled = true;

        if (enterTextUI)
            enterTextUI.gameObject.SetActive(false);

        currentSeatIndex = System.Array.IndexOf(allSeatPoints, this);
    }

    private void SwitchSeat()
    {
        // Désactive l'ancien siège
        SeatPoint oldSeat = allSeatPoints[currentSeatIndex];
        oldSeat.isSeated = false;

        // Passe au suivant
        currentSeatIndex = (currentSeatIndex + 1) % allSeatPoints.Length;
        SeatPoint newSeat = allSeatPoints[currentSeatIndex];

        // Revient au même joueur
        isSeated = true;
        newSeat.isSeated = true;

        playerController.transform.SetParent(newSeat.seatTransform);
        playerController.transform.localPosition = Vector3.zero;
        playerController.transform.localRotation = Quaternion.identity;

        playerController.canMove = false;
        playerRb.isKinematic = true;

        if (enterTextUI)
            enterTextUI.gameObject.SetActive(false);
    }

    private void ExitSeat()
    {
        if (!playerController) return;

        isSeated = false;

        playerController.transform.SetParent(null);
        playerController.canMove = true;

        if (playerRb) playerRb.isKinematic = false;
        if (playerCollider) playerCollider.enabled = true;

        Vector3 exitPos = exitPoint ?
            exitPoint.position :
            seatTransform.position + seatTransform.right * 2f;

        playerController.transform.position = exitPos;

        if (enterTextUI)
            enterTextUI.gameObject.SetActive(true);
    }
}
