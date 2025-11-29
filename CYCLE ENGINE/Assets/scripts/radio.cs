using UnityEngine;

public class RadioController3D : MonoBehaviour
{
    [Header("Audio")]
    public AudioSource radioAudio;        // Son de la radio
    [Range(0f,1f)]
    public float volumeStep = 0.1f;       // incrément de volume

    [Header("Playlist")]
    public AudioClip[] musicTracks;       // Liste des musiques
    private int currentTrackIndex = 0;    // Musique actuelle

    private bool isOn = false;

    void Start()
    {
        if (radioAudio == null)
            radioAudio = GetComponent<AudioSource>();

        radioAudio.volume = 0f;
        radioAudio.Stop();

        if (musicTracks.Length > 0)
            radioAudio.clip = musicTracks[currentTrackIndex];
    }

    /// <summary>
    /// Bouton ON/OFF
    /// </summary>
    public void ToggleRadio()
    {
        isOn = !isOn;

        if (isOn)
        {
            radioAudio.Play();
            radioAudio.volume = 0.5f; // volume par défaut
        }
        else
        {
            radioAudio.Stop();
        }
    }

    /// <summary>
    /// Volume +
    /// </summary>
    public void VolumeUp()
    {
        if (!isOn) return;
        radioAudio.volume = Mathf.Clamp(radioAudio.volume + volumeStep, 0f, 1f);
    }

    /// <summary>
    /// Volume -
    /// </summary>
    public void VolumeDown()
    {
        if (!isOn) return;
        radioAudio.volume = Mathf.Clamp(radioAudio.volume - volumeStep, 0f, 1f);
    }

    /// <summary>
    /// Musique suivante
    /// </summary>
    public void NextTrack()
    {
        if (musicTracks.Length == 0 || !isOn) return;

        currentTrackIndex = (currentTrackIndex + 1) % musicTracks.Length;
        radioAudio.clip = musicTracks[currentTrackIndex];
        radioAudio.Play();
    }

    /// <summary>
    /// Musique précédente
    /// </summary>
    public void PreviousTrack()
    {
        if (musicTracks.Length == 0 || !isOn) return;

        currentTrackIndex--;
        if (currentTrackIndex < 0) currentTrackIndex = musicTracks.Length - 1;

        radioAudio.clip = musicTracks[currentTrackIndex];
        radioAudio.Play();
    }
}
