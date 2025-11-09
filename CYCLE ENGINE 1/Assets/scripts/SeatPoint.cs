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

    [Header("Son")]
    public AudioClip enterSeatSound;
    public AudioSource audioSource;

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

        // Désactive le canon par défaut
        if (turretController != null)
            turretController.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerController = other.GetComponent<SimpleFPSController>();
        if (!playerController) return;

        playerRb = other.GetComponent<Rigidbody>();
        playerCollider = other.GetComponent<Collider>();

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
        if (playerController == null) return;

        isSeated = true;
        canEnter = false;

        // 🔹 Son
        if (audioSource && enterSeatSound)
            audioSource.PlayOneShot(enterSeatSound);

        // 🔹 Parentage
        playerController.transform.SetParent(seatTransform);
        playerController.transform.localPosition = Vector3.zero;
        playerController.transform.localRotation = Quaternion.identity;

        // 🔹 Désactivation du contrôle et physique du joueur
        playerController.canMove = false;
        if (playerRb)
        {
            playerRb.isKinematic = true;
            playerRb.detectCollisions = false;
        }
        if (playerCollider)
            playerCollider.enabled = false;

        // 🔹 Cache le texte d'entrée
        if (enterTextUI) enterTextUI.gameObject.SetActive(false);

        // 🔹 Active le script du canon uniquement si c'est un siège canon
        if (isTurretSeat && turretController != null)
            turretController.enabled = true;
    }

    private void ExitSeat()
    {
        if (playerController == null) return;

        isSeated = false;

        // 🔹 Détache le joueur du véhicule
        playerController.transform.SetParent(null);

        // 🔹 Réactive la physique et le mouvement
        playerController.canMove = true;
        if (playerRb)
        {
            playerRb.isKinematic = false;
            playerRb.detectCollisions = true;
        }
        if (playerCollider)
            playerCollider.enabled = true;

        // 🔹 Téléporte à la sortie
        Vector3 exitPos = exitPoint ? exitPoint.position : seatTransform.position + seatTransform.right * 2f;
        playerController.transform.position = exitPos;

        // 🔹 Désactive le canon si présent
        if (turretController != null)
            turretController.enabled = false;
    }

    private void SwitchSeat()
    {
        if (allSeatPoints.Length <= 1 || playerController == null) return;

        // 🔹 Désactive le canon du siège actuel si c'en est un
        if (isTurretSeat && turretController != null)
            turretController.enabled = false;

        // 🔹 Marque ce siège comme libre
        isSeated = false;

        // 🔹 Passe au suivant
        currentSeatIndex = (currentSeatIndex + 1) % allSeatPoints.Length;
        SeatPoint nextSeat = allSeatPoints[currentSeatIndex];

        // 🔹 Déplace le joueur
        playerController.transform.SetParent(nextSeat.seatTransform);
        playerController.transform.localPosition = Vector3.zero;
        playerController.transform.localRotation = Quaternion.identity;

        // 🔹 Son
        if (nextSeat.audioSource && nextSeat.enterSeatSound)
            nextSeat.audioSource.PlayOneShot(nextSeat.enterSeatSound);

        // 🔹 Active le canon si le nouveau siège est une tourelle
        if (nextSeat.isTurretSeat && nextSeat.turretController != null)
            nextSeat.turretController.enabled = true;

        // 🔹 Marque le nouveau siège comme occupé
        nextSeat.isSeated = true;

        // 🔹 Met à jour le joueur dans le nouveau siège
        playerController.canMove = false;
        if (playerRb)
        {
            playerRb.isKinematic = true;
            playerRb.detectCollisions = false;
        }
        if (playerCollider)
            playerCollider.enabled = false;
    }
}
