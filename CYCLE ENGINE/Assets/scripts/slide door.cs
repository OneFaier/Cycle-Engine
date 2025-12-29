using UnityEngine;

public class SlidingDoorWithAudio : MonoBehaviour
{
    public enum DoorSide
    {
        Exterior,
        Interior
    }

    [Header("Positions")]
    public Transform closedPosition;
    public Transform openPosition;
    public float slideSpeed = 2f;

    [Header("SAS")]
    public DoorSide doorSide;
    public SasController sasController;

    private bool isOpen = false;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    void Start()
    {
        transform.position = closedPosition.position;
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        Vector3 target = isOpen ? openPosition.position : closedPosition.position;
        transform.position = Vector3.MoveTowards(transform.position, target, slideSpeed * Time.deltaTime);
    }

    // 🔘 Appelé par le bouton
    public void ToggleDoor()
    {
        if (isOpen)
        {
            CloseDoor();
            return;
        }

        if (!CanOpenDoor())
            return;

        OpenDoor();
    }

    public void OpenDoor()
    {
        if (!CanOpenDoor())
            return;

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

    // ✅ Fermeture forcée par le SAS
    public void ForceClose()
    {
        if (isOpen)
            CloseDoor();
    }

    private bool CanOpenDoor()
    {
        if (sasController == null)
            return true;

        return sasController.CanOpen(
            doorSide == DoorSide.Exterior
                ? SasController.SasSide.Exterior
                : SasController.SasSide.Interior
        );
    }

    public bool IsFullyClosed()
    {
        return Vector3.Distance(transform.position, closedPosition.position) < 0.01f;
    }

    public bool IsFullyOpen()
    {
        return Vector3.Distance(transform.position, openPosition.position) < 0.01f;
    }
}
