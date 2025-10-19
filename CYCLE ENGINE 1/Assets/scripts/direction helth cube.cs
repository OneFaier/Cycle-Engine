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

    void Start()
    {
        currentHealth = maxHealth;

        if (healthIndicatorCube != null)
            healthIndicatorCube.material.color = Color.green;
    }

    void Update()
    {
        if (directionCube == null) return;

        // Ne gérer la santé que si le cube est snapé (a un parent)
        if (transform.parent == null) return;

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
