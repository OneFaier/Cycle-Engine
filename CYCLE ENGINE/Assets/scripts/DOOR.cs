using UnityEngine;

public class BackDoor : MonoBehaviour
{
    [Header("Porte")]
    public Transform pivot;
    public float openAngle = 90f;
    public float openSpeed = 90f;

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
    public SasController sasAndSubmarine;

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

        // 🟥 Porte en mouvement ou ouverte → rouge
        if (!Mathf.Approximately(currentAngle, 0f))
        {
            UpdateLampColor(colorOpen);
            return;
        }

        // 🟩 Porte fermée → vert si SAS ne travaille pas
        if (sasAndSubmarine != null && !sasAndSubmarine.SasInProgress)
            UpdateLampColor(colorClosed); 
        else
            UpdateLampColor(colorOpen);
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

    private void UpdateLampColor(Color c)
    {
        lampLight.color = c;
    }
}