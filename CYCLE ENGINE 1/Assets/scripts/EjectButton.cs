using UnityEngine;

public class EjectButtonFPS : MonoBehaviour
{
    [Header("Références")]
    public Camera playerCamera;
    public SeatSnapWithIndicator seatSnap; // le SeatSnap avec voyant
    public float maxClickDistance = 5f;   // distance max pour cliquer

    private Renderer cubeRenderer;
    private Color baseColor = Color.black;   // bouton noir
    private Color hoverColor = Color.yellow; // survol

    void Start()
    {
        cubeRenderer = GetComponent<Renderer>();
        if (cubeRenderer != null)
            cubeRenderer.material.color = baseColor;
    }

    void Update()
    {
        // Raycast depuis la caméra
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                // Hover color
                if (cubeRenderer != null)
                    cubeRenderer.material.color = hoverColor;

                // Click
                if (Input.GetMouseButtonDown(0) && seatSnap != null)
                {
                    seatSnap.DestroySnappedObject();
                }

                return;
            }
        }

        // Pas sur le bouton → couleur noire
        if (cubeRenderer != null)
            cubeRenderer.material.color = baseColor;
    }
}
