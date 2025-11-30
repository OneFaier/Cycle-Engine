using UnityEngine;

public class SasButton : MonoBehaviour
{
    public SasController sasController;
    public float interactDistance = 3f;
    public Color highlightColor = Color.yellow;

    private Renderer rend;
    private Color baseColor;
    private Camera cam;
    private bool hovered;

    void Start()
    {
        cam = Camera.main;
        rend = GetComponent<Renderer>();

        if (rend != null)
            baseColor = rend.material.color;
    }

    void Update()
    {
        if (cam == null) return;

        CheckHover();
        CheckClick();
    }

    private void CheckHover()
    {
        hovered = false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, interactDistance))
        {
            if (hit.collider != null && hit.collider.gameObject == gameObject)
                hovered = true;
        }

        if (rend != null)
            rend.material.color = hovered ? highlightColor : baseColor;
    }

    private void CheckClick()
    {
        if (!hovered) return;
        if (sasController == null) return;

        if (Input.GetMouseButtonDown(0))
            sasController.ToggleSas();
    }
}