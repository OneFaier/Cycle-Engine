using UnityEngine;

public class DirectionCubeHealth : MonoBehaviour
{
    [Header("Références")]
    public IndicatorMouseClickFast directionCube; // Cube contrôlé par le joueur
    public Renderer healthIndicatorCube;          // Cube voyant de santé

    [Header("Paramètres")]
    public float maxHealth = 100f;             // Vie max
    public float depletionRate = 50f;          // Perte de vie maximale quand utilisé
    public float passiveDepletionRate = 5f;    // Dégradation passive même si pas utilisé

    private float currentHealth;

    [HideInInspector] public bool isInMachine = false; // ⚡️ Flag pour contrôler l’usure

    void Start()
    {
        currentHealth = maxHealth;

        if (healthIndicatorCube != null)
            healthIndicatorCube.material.color = Color.green;
    }

    void Update()
    {
        if (!isInMachine || directionCube == null) return; // ⬅️ N’use le cube que s’il est dans la machine

        // Intensité basée sur l'écart par rapport au centre (0.5 = centre)
        float intensity = Mathf.Abs(directionCube.positionNormalized - 0.5f) * 2f; // 0 = centre, 1 = extrême

        // Décrémente la vie selon l'utilisation + dégradation passive
        currentHealth -= (depletionRate * intensity + passiveDepletionRate) * Time.deltaTime;
        currentHealth = Mathf.Clamp(currentHealth, 0f, maxHealth);

        // Update cube voyant
        if (healthIndicatorCube != null)
        {
            float ratio = currentHealth / maxHealth;
            healthIndicatorCube.material.color = Color.Lerp(Color.red, Color.green, ratio);
        }

        // Détruire le cube si mort
        if (currentHealth <= 0f)
            Destroy(gameObject);
    }

    public bool IsDead() => currentHealth <= 0f;
    public float GetHealthRatio() => currentHealth / maxHealth;

    public void Repair()
    {
        currentHealth = maxHealth;
        if (healthIndicatorCube != null)
            healthIndicatorCube.material.color = Color.green;
    }
}