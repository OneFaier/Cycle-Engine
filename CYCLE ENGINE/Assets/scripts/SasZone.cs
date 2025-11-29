using UnityEngine;

public class SasController : MonoBehaviour
{
    [Header("Controllers")]
    public SimpleFPSController fpsController;
    public SwimController swimController;

    [Header("Audio & Effects")]
    public AudioSource underwaterAudio;
    public GameObject bubblesParticleSystem;
    public AudioSource waterSound;
    public AudioSource sasSound;
    public AudioClip soundDrain;
    public AudioClip soundFill;
    public WaterDrainEffect waterEffect;
    public ParticleSystem sasBubbles;

    [HideInInspector] public bool sasInProgress = false;

    [Header("Volumes")]
    public float airVolume = 0.15f;
    public float waterVolume = 1f;
    [Range(0f, 1f)]
    public float volumeInsideSubmarine = 0.2f;

    private enum SasMode { In, Out }
    private SasMode currentMode = SasMode.In; // In = plein d'eau → Swim, Out = vide → FPS

    private void Start()
    {
        EnableSwimController();

        if (bubblesParticleSystem != null)
            bubblesParticleSystem.SetActive(true);
    }

    public void PressSasButton()
    {
        currentMode = (currentMode == SasMode.In) ? SasMode.Out : SasMode.In;

        if (waterEffect != null)
        {
            if (currentMode == SasMode.In)
                waterEffect.StartWaterDrain();
            else
                waterEffect.StartWaterRise();
        }

        if (sasBubbles != null)
        {
            if (currentMode == SasMode.Out)
                sasBubbles.Play();
            else
                sasBubbles.Stop();
        }

        if (waterSound != null)
        {
            waterSound.volume = (currentMode == SasMode.In) ? waterVolume : airVolume;
            if (!waterSound.isPlaying) waterSound.Play();
        }

        if (sasSound != null)
        {
            sasSound.clip = (currentMode == SasMode.In) ? soundDrain : soundFill;
            sasSound.Play();
        }

        if (currentMode == SasMode.In)
            EnableSwimController();
        else
            EnableFPSController();

        // 🔄 Inversion du son global
        if (underwaterAudio != null)
            underwaterAudio.volume = (currentMode == SasMode.In) ? 1f : volumeInsideSubmarine;

        if (bubblesParticleSystem != null)
            bubblesParticleSystem.SetActive(currentMode == SasMode.In);
    }

    private void EnableSwimController()
    {
        if (fpsController != null) fpsController.enabled = false;
        if (swimController != null) swimController.enabled = true;
    }

    private void EnableFPSController()
    {
        if (fpsController != null) fpsController.enabled = true;
        if (swimController != null) swimController.enabled = false;
    }
}
