using UnityEngine;

public class SeatButtonExit : MonoBehaviour
{
    [Header("Références")]
    public Transform exitPoint;
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

        if (isHovered && Input.GetMouseButtonDown(0))
        {
            ExitSeat();
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

    void ExitSeat()
    {
        if (!playerController) return;

        playerController.transform.SetParent(null);

        // Position de sortie
        Vector3 exitPos = exitPoint ? exitPoint.position : playerController.transform.position + Vector3.right * 2f;
        playerController.transform.position = exitPos;

        // Réactive mouvements + souris
        playerController.canMove = true;
        playerController.mouseLookEnabled = true;

        if (playerCollider) playerCollider.enabled = true;
        if (playerRb) playerRb.isKinematic = false;
    }
}
