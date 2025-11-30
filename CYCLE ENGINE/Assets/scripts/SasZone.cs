using UnityEngine;

public class SasController : MonoBehaviour
{
    public SimpleFPSController fpsController;
    public SwimController swimController;

    [Header("Audio & Effects")]
    public AudioSource ambientWaterAudio; 
    public AudioSource sasSound;
    public AudioSource waterLoop;
    public AudioClip fillClip;
    public AudioClip drainClip;
    public ParticleSystem bubbleFX;
    public WaterDrainEffect waterEffect;

    [Header("Volumes")]
    public float volumeWater = 1f;
    public float volumeAir = 0.15f;
    public float volumeInsideSubmarine = 0.2f;

    private enum SasState { Filled, Empty }
    private SasState state = SasState.Filled;   // plein d’eau → mode natation

    public bool SasInProgress { get; private set; } = false;

    void Start()
    {
        ApplyState(state);
    }

    public void ToggleSas()
    {
        if (SasInProgress) return;

        SasInProgress = true;

        state = state == SasState.Filled ? SasState.Empty : SasState.Filled;

        ApplyState(state);

        Invoke(nameof(UnlockSas), 2f); // durée approximative d’un cycle
    }

    private void UnlockSas()
    {
        SasInProgress = false;
    }

    private void ApplyState(SasState newState)
    {
        bool isFilled = (newState == SasState.Filled);

        // Contrôleurs
        if (fpsController) fpsController.enabled = !isFilled;
        if (swimController) swimController.enabled = isFilled;

        // Effet visuel eau
        if (waterEffect)
        {
            if (isFilled) waterEffect.StartWaterRise();
            else waterEffect.StartWaterDrain();
        }

        // Particules
        if (bubbleFX)
        {
            if (isFilled) bubbleFX.Play();
            else bubbleFX.Stop();
        }

        // Sons
        if (sasSound)
        {
            sasSound.clip = isFilled ? fillClip : drainClip;
            sasSound.Play();
        }

        if (waterLoop)
        {
            waterLoop.volume = isFilled ? volumeWater : volumeAir;
            if (!waterLoop.isPlaying) waterLoop.Play();
        }

        if (ambientWaterAudio)
        {
            ambientWaterAudio.volume = isFilled ? 1f : volumeInsideSubmarine;
        }
    }
}
