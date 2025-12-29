using UnityEngine;
using System.Collections;

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

    // 🔑 Toujours un côté actif
    public enum SasSide
    {
        Exterior,
        Interior
    }

    private enum SasState
    {
        Filled,   // eau → extérieur
        Empty     // air → intérieur
    }

    [Header("SAS State")]
    public SasSide authorizedSide = SasSide.Exterior; // on commence dehors
    private SasState state = SasState.Filled;

    [Header("Timing")]
    public float cycleDuration = 2f;

    [Header("Toutes les portes SAS")]
    public SlidingDoorWithAudio[] allDoors; // assigner dans l’inspecteur

    public bool SasInProgress { get; private set; } = false;

    void Start()
    {
        authorizedSide = SasSide.Exterior;
        state = SasState.Filled;
        ApplyState(state);
    }

    // 🔘 Appelé par le bouton SAS
    public void ToggleSas()
    {
        if (SasInProgress)
            return;

        StartCoroutine(SasCycle());
    }

    private IEnumerator SasCycle()
    {
        SasInProgress = true;

        // 🔒 Ferme automatiquement toutes les portes du côté opposé
        if (allDoors != null)
        {
            foreach (var door in allDoors)
            {
                var sideAfterCycle = state == SasState.Filled ? SasSide.Interior : SasSide.Exterior;
                if ((door.doorSide == SlidingDoorWithAudio.DoorSide.Exterior && sideAfterCycle == SasSide.Interior) ||
                    (door.doorSide == SlidingDoorWithAudio.DoorSide.Interior && sideAfterCycle == SasSide.Exterior))
                {
                    door.ForceClose();
                }
            }
        }

        // 🔄 Inversion du SAS
        if (state == SasState.Filled)
        {
            state = SasState.Empty;
            authorizedSide = SasSide.Interior;
        }
        else
        {
            state = SasState.Filled;
            authorizedSide = SasSide.Exterior;
        }

        ApplyState(state);

        yield return new WaitForSeconds(cycleDuration);

        SasInProgress = false;
    }

    // 🔑 Utilisé par les portes
    public bool CanOpen(SasSide side)
    {
        return !SasInProgress && authorizedSide == side;
    }

    // 🌊 Application visuelle / audio
    private void ApplyState(SasState newState)
    {
        bool isFilled = (newState == SasState.Filled);

        if (fpsController) fpsController.enabled = !isFilled;
        if (swimController) swimController.enabled = isFilled;

        if (waterEffect)
        {
            if (isFilled) waterEffect.StartWaterRise();
            else waterEffect.StartWaterDrain();
        }

        if (bubbleFX)
        {
            if (isFilled) bubbleFX.Play();
            else bubbleFX.Stop();
        }

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

