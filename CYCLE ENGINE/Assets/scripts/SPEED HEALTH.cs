using UnityEngine;

public class CubeHealth : MonoBehaviour
{
    [Header("Références")]
    public IndicatorMouseClickFast cube; // Cube à surveiller
    public Renderer cubeRenderer;        // Pour changer la couleur

    [Header("Paramètres")]
    public float maxHealth = 100f;           // Vie maximale du cube
    public float depletionRate = 10f;        // Perte de vie selon l’intensité
    public float passiveDepletionRate = 2f;  // Dégradation passive même si pas utilisé

    private float currentHealth;
    private Color baseColor;

    [HideInInspector] public bool isInMachine = false; // ✅ Flag pour contrôler l’usure

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
        // N’use le cube que s’il est dans la machine
        if (!isInMachine || cube == null)
            return;

        float intensity = Mathf.Clamp01(cube.positionNormalized); // 0 à 1

        // Décrémente la vie selon l’intensité + dégradation passive
        currentHealth -= (depletionRate * intensity + passiveDepletionRate) * Time.deltaTime;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // Mise à jour de la couleur
        if (cubeRenderer != null)
        {
            float ratio = currentHealth / maxHealth;
            cubeRenderer.material.color = Color.Lerp(Color.black, baseColor, ratio);
        }

        // Détruire le cube si mort
        if (currentHealth <= 0f)
        {
            Destroy(gameObject);
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