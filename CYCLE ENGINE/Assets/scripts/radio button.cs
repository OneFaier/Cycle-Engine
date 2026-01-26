using UnityEngine;

public class RadioButton3D : MonoBehaviour
{
    public enum ButtonType { OnOff, VolumeUp, VolumeDown, NextTrack }
    public ButtonType buttonType;

    [Header("Références")]
    public RadioController3D radio;
    public SimpleFPSController playerController; // ← à forcer
    public Camera playerCamera;                  // ← à forcer

    [Header("Interaction")]
    public float maxClickDistance = 100f;
    public float hoverMemoryDuration = 0.1f;
    public float screenFallbackRadius = 40f;

    [Header("Visual")]
    public Color baseColor = Color.white;
    public Color highlightColor = Color.yellow;

    private Renderer rend;
    private bool hovered = false;
    private float lastHoverTime = -1f;

    void Start()
    {
        rend = GetComponent<Renderer>();

        if (!playerCamera && playerController)
            playerCamera = playerController.fpsCamera;

        if (rend)
            baseColor = rend.material.color;
    }

    void Update()
    {
        if (!playerCamera || !radio) return;

        HandleHover();
        HandleInput();
    }

    void HandleHover()
    {
        bool hitThisFrame = false;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance))
        {
            if (hit.collider.gameObject == gameObject)
            {
                hitThisFrame = true;
                lastHoverTime = Time.time;
            }
        }

        bool shouldBeHovered =
            hitThisFrame ||
            (Time.time - lastHoverTime < hoverMemoryDuration);

        // Fallback écran (comme SeatButton)
        if (!shouldBeHovered)
        {
            Vector3 screenPoint = playerCamera.WorldToScreenPoint(transform.position);
            float dist = Vector2.Distance(Input.mousePosition, screenPoint);
            if (dist < screenFallbackRadius)
            {
                shouldBeHovered = true;
                lastHoverTime = Time.time;
            }
        }

        if (shouldBeHovered != hovered)
        {
            hovered = shouldBeHovered;
            if (rend)
                rend.material.color = hovered ? highlightColor : baseColor;
        }
    }

    void HandleInput()
    {
        if (!hovered) return;

        if (Input.GetMouseButtonDown(0))
        {
            switch (buttonType)
            {
                case ButtonType.OnOff:
                    radio.ToggleRadio();
                    break;

                case ButtonType.VolumeUp:
                    radio.VolumeUp();
                    break;

                case ButtonType.VolumeDown:
                    radio.VolumeDown();
                    break;

                case ButtonType.NextTrack:
                    radio.NextTrack();
                    break;
            }
        }
    }
}
