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
    public AudioClip modulePickupClip;
    public AudioSource audioSource;
    public AudioSource alertAudioSource;
    public AudioClip alertClip;

    private CubeHealth speedModule;
    private DirectionCubeHealth directionModule;

    private void Update()
    {
        UpdateBlinkers();
        UpdateSliders();
        EnforceCubeLimits();
        HandleAlertAudio();
    }

    private void UpdateBlinkers()
    {
        // --- Module VITESSE ---
        if (speedModule != null)
        {
            CubeBlinker blink = speedModule.GetComponent<CubeBlinker>();
            if (blink != null)
            {
                float intensity = speedCube != null ? speedCube.positionNormalized : 0f;
                blink.SetIntensity(intensity);
            }
        }

        // --- Module DIRECTION ---
        if (directionModule != null)
        {
            CubeBlinker blink = directionModule.GetComponent<CubeBlinker>();
            if (blink != null)
            {
                float pos = directionCube != null ? directionCube.positionNormalized : 0.5f;

                // Intensité en fonction de la distance au centre (0.5)
                float intensity = Mathf.Abs(pos - 0.5f) * 2f;

                blink.SetIntensity(intensity);
            }
        }
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

                var blink = speedModule.GetComponent<CubeBlinker>();
                if (blink != null)
                    blink.SetIntensity(speedCube != null ? speedCube.positionNormalized : 0f);
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

                var blink = directionModule.GetComponent<CubeBlinker>();
                if (blink != null)
                    blink.SetIntensity(directionCube != null ? directionCube.positionNormalized : 0f);
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
            audioSource.PlayOneShot(modulePickupClip);
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
        bool speedModuleDead = speedModule == null || speedModule.IsDead();
        bool directionModuleDead = directionModule == null || directionModule.IsDead();

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
