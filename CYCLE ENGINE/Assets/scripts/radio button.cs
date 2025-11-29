using UnityEngine;

public class RadioButton3D : MonoBehaviour
{
    public enum ButtonType { OnOff, VolumeUp, VolumeDown, NextTrack } // ← ajouté NextTrack

    public ButtonType buttonType;

    public RadioController3D radio;
    public float maxInteractDistance = 3f;
    private bool hovered = false;
    private Renderer rend;
    private Color baseColor;
    public Color highlightColor = Color.yellow;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
        rend = GetComponent<Renderer>();
        if (rend != null)
            baseColor = rend.material.color;
    }

    void Update()
    {
        HandleHover();
        HandleInput();
    }

    void HandleHover()
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        bool hitThis = Physics.Raycast(ray, out RaycastHit hit, maxInteractDistance) &&
                       hit.collider == GetComponent<Collider>();

        if (hitThis != hovered)
        {
            hovered = hitThis;
            if (rend != null)
                rend.material.color = hovered ? highlightColor : baseColor;
        }
    }

    void HandleInput()
    {
        if (hovered && Input.GetMouseButtonDown(0) && radio != null)
        {
            switch (buttonType)
            {
                case ButtonType.OnOff: radio.ToggleRadio(); break;
                case ButtonType.VolumeUp: radio.VolumeUp(); break;
                case ButtonType.VolumeDown: radio.VolumeDown(); break;
                case ButtonType.NextTrack: radio.NextTrack(); break; // ← nouveau cas
            }
        }
    }
}