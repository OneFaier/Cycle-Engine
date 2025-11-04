using UnityEngine;

public class ElevationCubeHealth : MonoBehaviour
{
    [Header("Références")]
    public IndicatorMouseClickFast elevationCube;
    public Renderer cubeRenderer;

    [Header("Paramètres")]
    public float maxHealth = 100f;
    public float recoveryRate = 5f;
    public float depletionRate = 10f;
    public float passiveDepletionRate = 2f;

    private float currentHealth;
    private Color baseColor;

    void Start()
    {
        currentHealth = maxHealth;

        if (cubeRenderer == null)
            cubeRenderer = GetComponent<Renderer>();

        if (cubeRenderer != null)
            baseColor = cubeRenderer.material.color;
    }

    void Update()
    {
        if (elevationCube == null) return;
        if (transform.parent == null) return;

        float intensity = Mathf.Clamp01(elevationCube.positionNormalized);

        currentHealth -= (depletionRate * intensity + passiveDepletionRate) * Time.deltaTime;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        if (cubeRenderer != null)
        {
            float ratio = currentHealth / maxHealth;
            cubeRenderer.material.color = Color.Lerp(Color.black, baseColor, ratio);
        }
    }

    public bool IsDead() => currentHealth <= 0f;
    public float GetHealthRatio() => currentHealth / maxHealth;

    public void Repair()
    {
        currentHealth = maxHealth;
        if (cubeRenderer != null)
            cubeRenderer.material.color = baseColor;
    }
}