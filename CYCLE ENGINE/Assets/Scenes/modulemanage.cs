using UnityEngine;
using UnityEngine.UI;

public class ModuleResourceManage : MonoBehaviour
{
    [Header("Slots")]
    public GasBottleSlot[] speedSlots;
    public GasBottleSlot[] directionSlots;

    [Header("Settings")]
    public int maxBottles = 3;
    public float speedUseMultiplier = 10f;
    public float directionUseMultiplier = 10f;

    [Header("References")]
    public IndicatorGearMouse gearLever;
    public IndicatorMouseClickFast directionCube;

    [Header("UI")]
    public Slider speedSlider;
    public Slider directionSlider;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip bottleEmptyClip;

    [Header("Visual Slots - Speed")]
    public SpriteRenderer[] speedIndicators;

    [Header("Visual Slots - Direction")]
    public SpriteRenderer[] directionIndicators;

    [Header("Colors")]
    public Color emptyColor = Color.black;
    public Color filledColor = new Color(0.5f, 0.8f, 1f); // bleu ciel

    void Update()
    {
        ApplySpeedUsage();
        ApplyDirectionUsage();

        if (speedSlider)
            speedSlider.value = SpeedEfficiency;

        if (directionSlider)
            directionSlider.value = DirectionEfficiency;

        UpdateVisuals();
    }

    // ================= PROPERTIES =================
    public int SpeedCount => Count(speedSlots);
    public int DirectionCount => Count(directionSlots);

    public float SpeedEfficiency =>
        Mathf.Clamp01(GetTotalGas(speedSlots) / maxBottles);

    public float DirectionEfficiency =>
        Mathf.Clamp01(GetTotalGas(directionSlots) / maxBottles);

    public int MaxGearAllowed
    {
        get
        {
            int speedBottles = SpeedCount; // nombre de bouteilles actives (max 3)
            int maxGear = 1; // Gear 0 et 1 par défaut

            // Mapping spécifique
            switch (speedBottles)
            {
                case 0:
                    maxGear = 2; // Gear 0-1
                    break;
                case 1:
                    maxGear = 3; // Gear 1
                    break;
                case 2:
                    maxGear = 5; // Gear 3-4
                    break;
                case 3:
                    maxGear = 6; // Gear 4-5
                    break;
            }

            // Ne jamais dépasser le nombre total de gears du vaisseau
            if (gearLever && gearLever.GetComponentInParent<SpaceshipAdvanced>())
            {
                int totalGears = gearLever.GetComponentInParent<SpaceshipAdvanced>().gearSpeeds.Length;
                maxGear = Mathf.Min(maxGear, totalGears);
            }

            return maxGear;
        }
    }




    // ================= SPEED =================
    void ApplySpeedUsage()
    {
        if (gearLever == null) return;

        int gear = gearLever.currentGear;
        float amount = gear * speedUseMultiplier * Time.deltaTime;

        foreach (var s in speedSlots)
        {
            if (s.currentBottle == null) continue;

            var bottle = s.currentBottle;

            if (!bottle.IsEmptyTriggered)
            {
                bottle.isActive = true;
                bottle.UseGas(amount);
            }

            if (bottle.IsEmptyTriggered)
            {
                PlayBottleEmptySound();
                Destroy(bottle.gameObject);
                s.currentBottle = null;
            }

            break; // seulement une bouteille consommée par frame
        }
    }

    // ================= DIRECTION =================
    void ApplyDirectionUsage()
    {
        if (directionCube == null) return;

        float deviation = Mathf.Abs(directionCube.positionNormalized - 0.5f);
        float amount = deviation * directionUseMultiplier * Time.deltaTime;

        foreach (var s in directionSlots)
        {
            if (s.currentBottle == null) continue;

            var bottle = s.currentBottle;

            if (!bottle.IsEmptyTriggered)
            {
                bottle.isActive = true;
                bottle.UseGas(amount);
            }

            if (bottle.IsEmptyTriggered)
            {
                PlayBottleEmptySound();
                Destroy(bottle.gameObject);
                s.currentBottle = null;
            }

            break; // seulement une bouteille consommée par frame
        }
    }

    // ================= VISUAL =================
    void UpdateVisuals()
    {
        UpdateIndicatorGroup(speedIndicators, SpeedCount);
        UpdateIndicatorGroup(directionIndicators, DirectionCount);
    }

    void UpdateIndicatorGroup(SpriteRenderer[] indicators, int filledCount)
    {
        if (indicators == null) return;

        for (int i = 0; i < indicators.Length; i++)
        {
            if (indicators[i] == null) continue;
            indicators[i].color = (i < filledCount) ? filledColor : emptyColor;
        }
    }

    // ================= UTILS =================
    void PlayBottleEmptySound()
    {
        if (audioSource && bottleEmptyClip)
            audioSource.PlayOneShot(bottleEmptyClip);
    }

    float GetTotalGas(GasBottleSlot[] slots)
    {
        float total = 0f;
        foreach (var s in slots)
            if (s.currentBottle != null)
                total += s.currentBottle.GetRatio();
        return total;
    }

    int Count(GasBottleSlot[] slots)
    {
        int c = 0;
        foreach (var s in slots)
            if (s.currentBottle != null && !s.currentBottle.IsEmptyTriggered)
                c++;
        return c;
    }
}
