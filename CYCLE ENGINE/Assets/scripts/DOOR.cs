using UnityEngine;

public class BackDoor : MonoBehaviour
{
    public enum DoorSide
    {
        Exterior,
        Interior
    }

    [Header("Porte")]
    public Transform pivot;
    public float openAngle = 90f;
    public float openSpeed = 90f;
    public DoorSide doorSide;

    private bool isOpen = false;
    private float currentAngle = 0f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    [Header("Lampe (Point Light)")]
    public Light lampLight;
    public Color colorClosed = Color.green;
    public Color colorOpen = Color.red;

    [Header("SAS")]
    public SasController sasController;

    private void Start()
    {
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        UpdateLampColor(colorClosed);
    }

    private void Update()
    {
        float targetAngle = isOpen ? openAngle : 0f;
        float step = openSpeed * Time.deltaTime;

        currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, step);
        pivot.localRotation = Quaternion.Euler(currentAngle, 0f, 0f);

        UpdateLampLogic();
    }

    private void UpdateLampLogic()
    {
        if (lampLight == null)
            return;

        if (!Mathf.Approximately(currentAngle, 0f))
        {
            UpdateLampColor(colorOpen);
            return;
        }

        if (sasController != null && !sasController.SasInProgress)
            UpdateLampColor(colorClosed);
        else
            UpdateLampColor(colorOpen);
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

    private void OpenDoor()
    {
        isOpen = true;

        if (audioSource && openSound)
            audioSource.PlayOneShot(openSound);
    }

    private void CloseDoor()
    {
        isOpen = false;

        if (audioSource && closeSound)
            audioSource.PlayOneShot(closeSound);
    }

    private void UpdateLampColor(Color c)
    {
        lampLight.color = c;
    }
}
