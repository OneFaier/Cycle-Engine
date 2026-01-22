using UnityEngine;

public class GasBottleResource : MonoBehaviour
{
    [Header("Gas")]
    public float maxGas = 100f;
    private float currentGas;

    [Header("State")]
    public bool isInstalled = false;
    public bool isActive = false;
    public Vector3 originalScale = Vector3.one;

    [Header("Visual")]
    public Renderer bottleRenderer;
    public Color fullColor = Color.green;
    public Color midColor = Color.yellow;
    public Color emptyColor = Color.red;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    void Start()
    {
        currentGas = maxGas;
        UpdateColor();
    }

    public void Install()
    {
        isInstalled = true;
        isActive = false;
    }

    public void Uninstall()
    {
        isInstalled = false;
        isActive = false;
    }

    /// <summary>
    /// Vide la bouteille d'une quantité donnée
    /// </summary>
    public void UseGas(float amount)
    {
        if (!isInstalled || !isActive) return;

        currentGas -= amount;
        currentGas = Mathf.Clamp(currentGas, 0f, maxGas);

        UpdateColor();

        if (IsEmpty())
        {
            Destroy(gameObject);
        }
    }

    void UpdateColor()
    {
        if (!bottleRenderer) return;

        float ratio = GetRatio();
        Color c;

        if (ratio > 0.5f)
            c = Color.Lerp(midColor, fullColor, (ratio - 0.5f) * 2f);
        else
            c = Color.Lerp(emptyColor, midColor, ratio * 2f);

        bottleRenderer.material.color = c;
    }

    public bool IsEmpty() => currentGas <= 0f;
    public float GetRatio() => currentGas / maxGas;
}