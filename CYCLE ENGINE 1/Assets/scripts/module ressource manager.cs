using UnityEngine;
using UnityEngine.UI;

public class ModuleResourceManager : MonoBehaviour
{
    [Header("Cubes de contrôle")]
    public IndicatorMouseClickFast speedCube;
    public IndicatorMouseClickFast directionCube;

    [Header("Slots fixes (Empty)")]
    public Transform speedSlot;
    public Transform directionSlot;

    [Header("UI Sliders")]
    public Slider speedSlider;
    public Slider directionSlider;

    private CubeHealth speedModule;
    private DirectionCubeHealth directionModule;

    private void Update()
    {
        UpdateSliders();
        EnforceCubeLimits();
    }

    private void OnTriggerEnter(Collider other)
    {
        // --- Gestion Cube Vitesse ---
        CubeHealth cubeHealth = other.GetComponent<CubeHealth>();
        if (cubeHealth != null)
        {
            if (speedModule == null || cubeHealth.GetHealthRatio() > speedModule.GetHealthRatio())
            {
                ReplaceModule(ref speedModule, cubeHealth, speedSlot);
            }
            return;
        }

        // --- Gestion Cube Direction ---
        DirectionCubeHealth dirHealth = other.GetComponent<DirectionCubeHealth>();
        if (dirHealth != null)
        {
            if (directionModule == null || dirHealth.GetHealthRatio() > directionModule.GetHealthRatio())
            {
                ReplaceModule(ref directionModule, dirHealth, directionSlot);
            }
        }
    }

    private void ReplaceModule<T>(ref T slotModule, T newModule, Transform slot) where T : MonoBehaviour
    {
        // Détruire l'ancien module si présent
        if (slotModule != null)
            Destroy(slotModule.gameObject);

        slotModule = newModule;

        // Snap dans le slot et activer l’usure
        SnapToSlot(newModule.gameObject, slot);
    }

    private void SnapToSlot(GameObject moduleObj, Transform slot)
    {
        if (moduleObj == null || slot == null) return;

        moduleObj.transform.position = slot.position;
        moduleObj.transform.rotation = slot.rotation;
        moduleObj.transform.SetParent(slot);

        Rigidbody rb = moduleObj.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        // Activer l'usure uniquement lorsqu'il est dans la machine
        CubeHealth cubeHealth = moduleObj.GetComponent<CubeHealth>();
        if (cubeHealth != null) cubeHealth.isInMachine = true;

        DirectionCubeHealth dirHealth = moduleObj.GetComponent<DirectionCubeHealth>();
        if (dirHealth != null) dirHealth.isInMachine = true;
    }

    private void EnforceCubeLimits()
    {
        // Si pas de module vitesse ou mort, bloquer le cube à 0
        if (speedCube != null)
            speedCube.positionNormalized = (speedModule != null && !speedModule.IsDead()) ? speedCube.positionNormalized : 0f;

        // Si pas de module direction ou mort, bloquer le cube à 0.5
        if (directionCube != null)
            directionCube.positionNormalized = (directionModule != null && !directionModule.IsDead()) ? directionCube.positionNormalized : 0.5f;
    }

    private void UpdateSliders()
    {
        if (speedSlider != null)
            speedSlider.value = speedModule != null ? speedModule.GetHealthRatio() : 0f;

        if (directionSlider != null)
            directionSlider.value = directionModule != null ? directionModule.GetHealthRatio() : 0f;
    }

    // Méthodes publiques pour assigner directement un module
    public void AssignSpeedModule(CubeHealth module) => ReplaceModule(ref speedModule, module, speedSlot);
    public void AssignDirectionModule(DirectionCubeHealth module) => ReplaceModule(ref directionModule, module, directionSlot);
}
