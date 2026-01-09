using UnityEngine;

public class GasBottleResource : MonoBehaviour
{
    [Header("Gas")]
    public float maxGas = 100f;
    private float currentGas;

    [Header("State")]
    public bool isInstalled = false;
    public bool isActive = false;   // 🔥 seule celle-ci se vide
    public Vector3 originalScale = Vector3.one;

    void Awake()
    {
        originalScale = transform.localScale;
    }

    void Start()
    {
        currentGas = maxGas;
    }

    /// <summary>
    /// Vide la bouteille d'une quantité donnée (appelé par le ModuleManager selon vitesse ou direction)
    /// </summary>
    public void UseGas(float amount)
    {
        if (!isInstalled || !isActive) return;

        currentGas -= amount;
        currentGas = Mathf.Clamp(currentGas, 0f, maxGas);

        if (IsEmpty())
        {
            Destroy(gameObject); // détruit immédiatement quand vide
        }
    }

    public bool IsEmpty() => currentGas <= 0f;
    public float GetRatio() => currentGas / maxGas;
}