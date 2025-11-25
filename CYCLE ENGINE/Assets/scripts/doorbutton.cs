using UnityEngine;

public class DoorButton : MonoBehaviour
{
    [Header("Interaction")]
    public BackDoor targetDoor;          // La porte que ce bouton contrôle
    public float maxInteractDistance = 3f;
    public Color highlightColor = Color.yellow;

    private Renderer rend;
    private Color baseColor;
    private Camera playerCamera;
    private bool isHovered = false;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null) baseColor = rend.material.color;
        playerCamera = Camera.main;
    }

    void Update()
    {
        HandleHover();
        HandleInput();
    }

    void HandleHover()
    {
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        bool hitThisFrame = Physics.Raycast(ray, out RaycastHit hit, maxInteractDistance) && hit.collider.gameObject == gameObject;

        if (hitThisFrame != isHovered)
        {
            isHovered = hitThisFrame;
            if (rend != null) rend.material.color = isHovered ? highlightColor : baseColor;
        }
    }

    void HandleInput()
    {
        if (isHovered && Input.GetMouseButtonDown(0) && targetDoor != null)
        {
            targetDoor.ToggleDoor();
        }
    }
}