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

    [Header("Audio")]
    public AudioClip modulePickupClip;      // Son à jouer quand un module est récupéré
    public AudioSource audioSource;         // AudioSource pour jouer le son de pickup
    public AudioSource alertAudioSource;    // AudioSource pour la boucle d'alerte
    public AudioClip alertClip;             // Son de la boucle d'alerte

    private CubeHealth speedModule;
    private DirectionCubeHealth directionModule;

    private void Update()
    {
        UpdateSliders();
        EnforceCubeLimits();
        HandleAlertAudio();
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
        if (slotModule != null)
            Destroy(slotModule.gameObject);

        slotModule = newModule;

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

        CubeHealth cubeHealth = moduleObj.GetComponent<CubeHealth>();
        if (cubeHealth != null) cubeHealth.isInMachine = true;

        DirectionCubeHealth dirHealth = moduleObj.GetComponent<DirectionCubeHealth>();
        if (dirHealth != null) dirHealth.isInMachine = true;

        if (audioSource != null && modulePickupClip != null)
        {
            audioSource.PlayOneShot(modulePickupClip);
        }
    }

    private void EnforceCubeLimits()
    {
        if (speedCube != null)
            speedCube.positionNormalized = (speedModule != null && !speedModule.IsDead()) ? speedCube.positionNormalized : 0f;

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

    private void HandleAlertAudio()
    {
        // Vérifie si les modules sont absents ou morts
        bool speedModuleDead = speedModule == null || speedModule.IsDead();
        bool directionModuleDead = directionModule == null || directionModule.IsDead();

        // Si au moins un module est mort ou absent, on joue l'alerte
        bool shouldPlayAlert = speedModuleDead || directionModuleDead;

        if (alertAudioSource != null && alertClip != null)
        {
            if (shouldPlayAlert && !alertAudioSource.isPlaying)
            {
                alertAudioSource.clip = alertClip;
                alertAudioSource.loop = true;
                alertAudioSource.Play();
            }
            else if (!shouldPlayAlert && alertAudioSource.isPlaying)
            {
                alertAudioSource.Stop();
            }
        }
    }

    public void AssignSpeedModule(CubeHealth module) => ReplaceModule(ref speedModule, module, speedSlot);
    public void AssignDirectionModule(DirectionCubeHealth module) => ReplaceModule(ref directionModule, module, directionSlot);
}
