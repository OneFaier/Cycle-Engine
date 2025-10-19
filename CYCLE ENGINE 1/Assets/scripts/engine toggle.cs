using UnityEngine;

public class EngineToggle3D : MonoBehaviour
{
    [Header("Références")]
    public ENGINEMovements engineScript;
    public Renderer cubeRenderer;

    [Header("Couleurs")]
    public Color onColor = Color.green;
    public Color offColor = Color.gray;

    private bool isOn = true;

    void Start()
    {
        if (cubeRenderer == null)
            cubeRenderer = GetComponent<Renderer>();

        UpdateVisual();
    }

    void OnMouseDown()
    {
        if (engineScript == null) return;

        // Inverse l'état
        isOn = !isOn;
        engineScript.isEngineOn = isOn; // active ou désactive le moteur

        UpdateVisual();
    }

    void UpdateVisual()
    {
        if (cubeRenderer != null)
            cubeRenderer.material.color = isOn ? onColor : offColor;
    }
}
