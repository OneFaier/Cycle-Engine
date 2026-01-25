using UnityEngine;

public class GasBottleResource : MonoBehaviour
{
    [Header("Gas")]
    public float maxGas = 100f;
    private float currentGas;

    [Header("State")]
    public bool isInstalled = false;
    public bool isActive = false;
    public bool isGrabbed = false;
    public Vector3 originalScale = Vector3.one;

    [HideInInspector] public GasBottleSlot previewSlot;

    [Header("Visual")]
    public Renderer bottleRenderer;
    public Color fullColor = Color.green;
    public Color midColor = Color.yellow;
    public Color emptyColor = Color.red;

    public bool IsEmptyTriggered { get; private set; } = false;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    void Start()
    {
        currentGas = maxGas;
        UpdateColor();
    }

    public void UseGas(float amount)
    {
        if (!isInstalled || !isActive || IsEmptyTriggered) return;

        currentGas -= amount;
        currentGas = Mathf.Clamp(currentGas, 0f, maxGas);
        UpdateColor();

        if (currentGas <= 0f)
            IsEmptyTriggered = true; // 🔔 signal au manager
    }

    void UpdateColor()
    {
        if (!bottleRenderer) return;

        float ratio = GetRatio();
        Color c = ratio > 0.5f
            ? Color.Lerp(midColor, fullColor, (ratio - 0.5f) * 2f)
            : Color.Lerp(emptyColor, midColor, ratio * 2f);

        bottleRenderer.material.color = c;
    }

    public bool IsEmpty() => currentGas <= 0f;
    public float GetRatio() => currentGas / maxGas;
}