using UnityEngine;

public class ShowEmptyOnHover : MonoBehaviour
{
    public GameObject outlineEmpty;

    [Header("Hover écran")]
    public float hoverRadius = 40f;

    [Header("Distance max monde")]
    public float maxDistance = 2f; // 2 mètres max

    Camera playerCamera;
    bool isHovered = false;

    void Start()
    {
        playerCamera = Camera.main;

        if (outlineEmpty != null)
            outlineEmpty.SetActive(false);
    }

    void Update()
    {
        if (!playerCamera) return;

        // Distance monde caméra -> objet
        float worldDistance = Vector3.Distance(
            playerCamera.transform.position,
            transform.position
        );

        // Trop loin => jamais affiché
        if (worldDistance > maxDistance)
        {
            SetHover(false);
            return;
        }

        // Hover écran (même logique que ton levier)
        Vector3 screenPoint = playerCamera.WorldToScreenPoint(transform.position);

        bool hoverNow =
            screenPoint.z > 0 && // devant la caméra
            Vector2.Distance(Input.mousePosition, screenPoint) < hoverRadius;

        SetHover(hoverNow);
    }

    void SetHover(bool value)
    {
        if (value == isHovered) return;

        isHovered = value;

        if (outlineEmpty != null)
            outlineEmpty.SetActive(isHovered);
    }
}