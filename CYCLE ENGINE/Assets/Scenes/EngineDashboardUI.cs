using UnityEngine;
using UnityEngine.UI;

public class EngineDashboardUI : MonoBehaviour
{
    [Header("Sliders sur le tableau de bord")]
    public Slider speedSlider;      // vitesse
    public Slider directionSlider;  // direction

    [Header("Module Resource Manager")]
    public ModuleResourceManage moduleManager;

    void Update()
    {
        if (moduleManager == null) return;

        if (speedSlider != null)
            speedSlider.value = moduleManager.SpeedEfficiency;      // 0 = vide, 1 = full

        if (directionSlider != null)
            directionSlider.value = moduleManager.DirectionEfficiency; // 0 = vide, 1 = full
    }
}