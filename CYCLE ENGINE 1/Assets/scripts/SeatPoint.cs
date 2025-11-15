using UnityEngine;
using TMPro;

public class SeatPoint : MonoBehaviour
{
    [Header("Références")]
    public Transform seatTransform;    // Point où le joueur s'assoit
    public Transform exitPoint;        // Point où le joueur sort

    [Header("Touches")]
    public KeyCode enterKey = KeyCode.F;
    public KeyCode exitKey = KeyCode.E;
    public KeyCode switchSeatKey = KeyCode.B;

    [Header("UI")]
    public TextMeshProUGUI enterTextUI;

    [Header("Tous les sièges du véhicule")]
    public SeatPoint[] allSeatPoints;

    private SimpleFPSController playerController;
    private Rigidbody playerRb;
    private Collider playerCollider;
    private bool isSeated = false;
    private bool canEnter = false;
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

        canEnter = true;

        // Affiche le texte uniquement si le joueur n'est pas déjà assis
        if (enterTextUI && !isSeated)
            enterTextUI.gameObject.SetActive(true);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        canEnter = false;

        if (enterTextUI)
            enterTextUI.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (canEnter && !isSeated && Input.GetKeyDown(enterKey))
            EnterSeat();

        if (isSeated)
        {
            if (Input.GetKeyDown(exitKey))
                ExitSeat();

            if (allSeatPoints.Length > 1 && Input.GetKeyDown(switchSeatKey))
                SwitchSeat();
        }
    }

    private void EnterSeat()
    {
        if (!playerController) return;

        isSeated = true;
        canEnter = false;

        // Bloque le joueur dans le siège
        playerController.transform.SetParent(seatTransform);
        playerController.transform.localPosition = Vector3.zero;
        playerController.transform.localRotation = Quaternion.identity;

        playerController.canMove = false;

        if (playerRb) playerRb.isKinematic = true;
        if (playerCollider) playerCollider.enabled = true; // collider actif pour les triggers

        // 🔹 Désactive le texte dès qu'on est assis
        if (enterTextUI) enterTextUI.gameObject.SetActive(false);

        currentSeatIndex = System.Array.IndexOf(allSeatPoints, this);
    }

    private void ExitSeat()
    {
        if (!playerController) return;

        isSeated = false;

        playerController.transform.SetParent(null);
        playerController.canMove = true;

        if (playerRb) playerRb.isKinematic = false;
        if (playerCollider) playerCollider.enabled = true;

        Vector3 exitPos = exitPoint ? exitPoint.position : seatTransform.position + seatTransform.right * 2f;
        playerController.transform.position = exitPos;

        // 🔹 Réactive le texte si le joueur sort du siège
        if (enterTextUI) enterTextUI.gameObject.SetActive(true);
    }

    private void SwitchSeat()
    {
        if (allSeatPoints.Length <= 1 || playerController == null) return;

        // Désactive siège actuel
        SeatPoint currentSeat = allSeatPoints[currentSeatIndex];
        currentSeat.isSeated = false;

        // Passe au siège suivant
        currentSeatIndex = (currentSeatIndex + 1) % allSeatPoints.Length;
        SeatPoint nextSeat = allSeatPoints[currentSeatIndex];

        playerController.transform.SetParent(nextSeat.seatTransform);
        playerController.transform.localPosition = Vector3.zero;
        playerController.transform.localRotation = Quaternion.identity;

        nextSeat.isSeated = true;
        isSeated = true;

        playerController.canMove = false;
        if (playerRb) playerRb.isKinematic = true;
        if (playerCollider) playerCollider.enabled = true;

        // 🔹 Texte reste désactivé tant qu'on est assis
        if (enterTextUI) enterTextUI.gameObject.SetActive(false);
    }
}
