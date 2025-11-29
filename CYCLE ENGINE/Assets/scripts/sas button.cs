using UnityEngine;

public class SasButton : MonoBehaviour
{
    public SasController sasAndSubmarine;
    public float maxInteractDistance = 3f;
    public Color highlightColor = Color.yellow;

    private Renderer rend;
    private Color baseColor;
    private Camera cam;
    private bool hovered = false;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null) baseColor = rend.material.color;

        cam = Camera.main;
    }

    void Update()
    {
        HandleHover();
        HandleInput();
    }

    void HandleHover()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        bool hitThis = Physics.Raycast(ray, out RaycastHit hit, maxInteractDistance)
                       && hit.collider.gameObject == gameObject;

        if (hitThis != hovered)
        {
            hovered = hitThis;
            if (rend != null)
                rend.material.color = hovered ? highlightColor : baseColor;
        }
    }

    void HandleInput()
    {
        if (hovered && Input.GetMouseButtonDown(0) && sasAndSubmarine != null)
            sasAndSubmarine.PressSasButton();  // ← nouvelle méthode
    }
}