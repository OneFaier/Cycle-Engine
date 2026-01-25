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

    void Update()
    {
        ApplySpeedUsage();
        ApplyDirectionUsage();

        if (speedSlider)
            speedSlider.value = SpeedEfficiency;

        if (directionSlider)
            directionSlider.value = DirectionEfficiency;
    }

    public int SpeedCount => Count(speedSlots);
    public int DirectionCount => Count(directionSlots);

    public float SpeedEfficiency =>
        Mathf.Clamp01(GetTotalGas(speedSlots) / maxBottles);

    public float DirectionEfficiency =>
        Mathf.Clamp01(GetTotalGas(directionSlots) / maxBottles);

    public int MaxGearAllowed =>
        Mathf.Clamp(SpeedCount + 1, 1, maxBottles + 1);

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

            break;
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

            break;
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
