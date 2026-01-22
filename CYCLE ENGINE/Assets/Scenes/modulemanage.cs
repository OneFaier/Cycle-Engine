using UnityEngine;
using UnityEngine.UI;

public class ModuleResourceManage : MonoBehaviour
{
    [Header("Slots")]
    public GasBottleSlot[] speedSlots;
    public GasBottleSlot[] directionSlots;

    [Header("Settings")]
    public int maxBottles = 3;
    public float speedUseMultiplier = 10f;      // gas consommé par gear
    public float directionUseMultiplier = 10f;  // gas consommé par déviation

    [Header("References")]
    public IndicatorGearMouse gearLever;
    public IndicatorMouseClickFast directionCube;

    [Header("UI")]
    public Slider speedSlider;      // slider tableau de bord vitesse
    public Slider directionSlider;  // slider tableau de bord direction

    void Update()
    {
        // Appliquer la consommation selon usage
        ApplySpeedUsage();
        ApplyDirectionUsage();

        // Mettre à jour les sliders
        if (speedSlider != null)
            speedSlider.value = SpeedEfficiency; // 0 = vide, 1 = full

        if (directionSlider != null)
            directionSlider.value = DirectionEfficiency; // 0 = vide, 1 = full
    }

    public int SpeedCount => Count(speedSlots);
    public int DirectionCount => Count(directionSlots);

    public float SpeedEfficiency =>
        Mathf.Clamp01(GetTotalGas(speedSlots) / maxBottles);

    public float DirectionEfficiency =>
        Mathf.Clamp01(GetTotalGas(directionSlots) / maxBottles);

    public int MaxGearAllowed => Mathf.Clamp(SpeedCount + 1, 1, maxBottles + 1);

    void ApplySpeedUsage()
    {
        if (gearLever == null) return;
        int gear = gearLever.currentGear;

        float amount = gear * speedUseMultiplier * Time.deltaTime;

        foreach (var s in speedSlots)
        {
            if (s.currentBottle && !s.currentBottle.IsEmpty())
            {
                s.currentBottle.UseGas(amount);
                s.currentBottle.isActive = true;

                // détruire si vide
                if (s.currentBottle.IsEmpty())
                {
                    Destroy(s.currentBottle.gameObject);
                    s.currentBottle = null;
                }

                break; // une seule bouteille active à la fois
            }
        }
    }

    void ApplyDirectionUsage()
    {
        if (directionCube == null) return;

        float deviation = Mathf.Abs(directionCube.positionNormalized - 0.5f);
        float amount = deviation * directionUseMultiplier * Time.deltaTime;

        foreach (var s in directionSlots)
        {
            if (s.currentBottle && !s.currentBottle.IsEmpty())
            {
                s.currentBottle.UseGas(amount);
                s.currentBottle.isActive = true;

                // détruire si vide
                if (s.currentBottle.IsEmpty())
                {
                    Destroy(s.currentBottle.gameObject);
                    s.currentBottle = null;
                }

                break; // une seule bouteille active à la fois
            }
        }
    }

    float GetTotalGas(GasBottleSlot[] slots)
    {
        float total = 0f;

        foreach (var s in slots)
        {
            if (s.currentBottle != null)
                total += s.currentBottle.GetRatio(); // 0 → 1
        }

        return total;
    }

    int Count(GasBottleSlot[] slots)
    {
        int c = 0;
        foreach (var s in slots)
            if (s.currentBottle != null && !s.currentBottle.IsEmpty())
                c++;
        return c;
    }
}
