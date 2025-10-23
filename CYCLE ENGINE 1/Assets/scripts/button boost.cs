using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BoostButton3D : MonoBehaviour
{
    [Header("Références")]
    public Camera playerCamera;
    public float maxClickDistance = 5f;

    [HideInInspector] public bool isClicked = false;
    private Renderer rend;
    private Color baseColor = Color.black;
    private Color hoverColor = Color.yellow;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (rend != null) rend.material.color = baseColor;
    }

    void Update()
    {
        isClicked = false;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                if (rend != null) rend.material.color = hoverColor;

                if (Input.GetMouseButton(0))
                {
                    isClicked = true;
                }

                return;
            }
        }

        if (rend != null) rend.material.color = baseColor;
    }
}
