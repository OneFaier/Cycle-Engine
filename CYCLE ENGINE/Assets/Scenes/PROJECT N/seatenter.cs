using UnityEngine;

public class SeatButtonEnter : MonoBehaviour
{
    [Header("Références")]
    public Transform seatTransform;
    public GameObject player;
    private SimpleFPSController playerController;

    [Header("Bouton 3D")]
    public Color baseColor = Color.white;
    public Color highlightColor = Color.cyan;
    public float maxClickDistance = 3f;

    private Renderer rend;
    private Camera cam;
    private bool isHovered = false;

    private Collider playerCollider;
    private Rigidbody playerRb;

    void Start()
    {
        cam = Camera.main;
        rend = GetComponent<Renderer>();
        if (rend) rend.material.color = baseColor;

        if (!playerController && player)
            playerController = player.GetComponent<SimpleFPSController>();

        if (player)
        {
            playerCollider = player.GetComponent<Collider>();
            playerRb = player.GetComponent<Rigidbody>();
        }
    }

    void Update()
    {
        HandleHover();

        if (isHovered && Input.GetMouseButtonDown(0)) // clic gauche
        {
            EnterSeat();
        }
    }

    void HandleHover()
    {
        isHovered = false;
        if (!cam) return;

        Ray ray = new Ray(cam.transform.position, cam.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance))
        {
            if (hit.collider == GetComponent<Collider>())
                isHovered = true;
        }

        if (rend)
            rend.material.color = isHovered ? highlightColor : baseColor;
    }

    void EnterSeat()
    {
        if (!playerController) return;

        // Parent + position sur le siège
        playerController.transform.SetParent(seatTransform);
        playerController.transform.localPosition = Vector3.zero;
        playerController.transform.localRotation = Quaternion.identity;

        // Désactive le mouvement et la rotation
        playerController.canMove = false;
        playerController.mouseLookEnabled = false;

        if (playerCollider) playerCollider.enabled = false;
        if (playerRb) playerRb.isKinematic = true;
    }
}
