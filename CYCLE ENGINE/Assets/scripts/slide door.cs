using UnityEngine;

public class SlidingDoorWithAudio : MonoBehaviour
{
    [Header("Positions")]
    public Transform closedPosition;
    public Transform openPosition;
    public float slideSpeed = 2f;

    private bool isOpen = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    void Start()
    {
        // Initialise la position
        transform.position = closedPosition.position;

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Déplacement lisse
        Vector3 target = isOpen ? openPosition.position : closedPosition.position;
        transform.position = Vector3.MoveTowards(transform.position, target, slideSpeed * Time.deltaTime);
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;

        // Jouer le son correspondant
        if (audioSource != null)
        {
            if (isOpen && openSound != null)
                audioSource.PlayOneShot(openSound);
            else if (!isOpen && closeSound != null)
                audioSource.PlayOneShot(closeSound);
        }
    }

    public void OpenDoor()
    {
        isOpen = true;
        if (audioSource != null && openSound != null)
            audioSource.PlayOneShot(openSound);
    }

    public void CloseDoor()
    {
        isOpen = false;
        if (audioSource != null && closeSound != null)
            audioSource.PlayOneShot(closeSound);
    }
}