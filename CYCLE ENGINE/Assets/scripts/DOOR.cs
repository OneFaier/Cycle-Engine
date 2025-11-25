using UnityEngine;

public class BackDoor : MonoBehaviour
{
    public Transform pivot;          // Empty pivot qui tourne en X
    public float openAngle = 90f;
    public float openSpeed = 90f;

    private bool isOpen = false;
    private float currentAngle = 0f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;

    void Start()
    {
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        float target = isOpen ? openAngle : 0f;
        float step = openSpeed * Time.deltaTime;

        currentAngle = Mathf.MoveTowards(currentAngle, target, step);

        // 🔥 ROTATION EN X UNIQUEMENT
        pivot.localRotation = Quaternion.Euler(currentAngle, 0f, 0f);
    }

    public void ToggleDoor()
    {
        isOpen = !isOpen;

        if (audioSource != null)
        {
            if (isOpen && openSound != null)
                audioSource.PlayOneShot(openSound);
            else if (!isOpen && closeSound != null)
                audioSource.PlayOneShot(closeSound);
        }
    }
}