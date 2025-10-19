using UnityEngine;
using UnityEngine.UI;

public class DirectionCubeHealth : MonoBehaviour
{
    [Header("Références")]
    public IndicatorMouseClickFast directionCube; // Cube de direction à surveiller
    public Renderer cubeRenderer;                 // Pour changer la couleur
    public Slider healthSlider;                   // Slider représentant la vie du cube

    [Header("Paramètres")]
    public float maxHealth = 100f;       // Vie maximale du cube
    public float recoveryRate = 5f;      // Récupération quand pas utilisé
    public float depletionRate = 10f;    // Perte de vie selon l’intensité
    public float minPositionNormalized = 0.5f; // Valeur minimale pour le cube direction

    private float currentHealth;
    private bool isDead = false;
    private Color baseColor;

    void Start()
    {
        currentHealth = maxHealth;

        if (cubeRenderer == null)
            cubeRenderer = GetComponent<Renderer>();

        if (cubeRenderer != null)
            baseColor = cubeRenderer.material.color;

        if (healthSlider != null)
        {
            healthSlider.minValue = 0f;
            healthSlider.maxValue = maxHealth;
            healthSlider.value = maxHealth;
        }
    }

    void Update()
    {
        if (directionCube == null) return;

        // Ne gère la vie que si le cube est enfant du SnapPoint
        if (transform.parent == null) return;

        float intensity = Mathf.Clamp01(directionCube.positionNormalized); // 0 à 1

        if (isDead)
        {
            if (cubeRenderer != null)
                cubeRenderer.material.color = Color.black;

            if (healthSlider != null)
                healthSlider.value = 0f;

            // Maintenir la position minimale du cube direction
            directionCube.positionNormalized = minPositionNormalized;
            return;
        }

        // Décrémente la vie selon l’intensité
        if (intensity > 0.05f)
            currentHealth -= depletionRate * intensity * Time.deltaTime;
        else
            currentHealth += recoveryRate * Time.deltaTime; // régénération

        // Clamp
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // Met à jour le slider
        if (healthSlider != null)
            healthSlider.value = currentHealth;

        // Change la couleur selon la vie
        if (cubeRenderer != null)
        {
            float ratio = currentHealth / maxHealth;
            cubeRenderer.material.color = Color.Lerp(Color.black, baseColor, ratio);
        }

        // Si mort
        if (currentHealth <= 0f)
        {
            isDead = true;

            // On ne met pas le cube à 0 mais au minimum défini
            directionCube.positionNormalized = minPositionNormalized;

            if (cubeRenderer != null)
                cubeRenderer.material.color = Color.black;
            if (healthSlider != null)
                healthSlider.value = 0f;
        }
    }

    public bool IsDead() => isDead;
    public float GetHealthRatio() => currentHealth / maxHealth;

    // Permet de “réparer” le cube si besoin
    public void Repair()
    {
        isDead = false;
        currentHealth = maxHealth;
        if (cubeRenderer != null)
            cubeRenderer.material.color = baseColor;
        if (healthSlider != null)
            healthSlider.value = currentHealth;
    }
}
