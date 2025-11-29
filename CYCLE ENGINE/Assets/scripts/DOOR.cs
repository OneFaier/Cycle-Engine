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
    public Color colorClosed = Color.green;  // devient vert si SAS terminé
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

        if (!Mathf.Approximately(currentAngle, 0f))
        {
            UpdateLampColor(colorOpen); // rouge si porte ouverte
            return;
        }

        // Porte fermée : vert si SAS terminé, sinon rouge
        if (sasAndSubmarine != null && !sasAndSubmarine.sasInProgress)
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

        // ⚠ PAS d'appel à sasZone ici : le SAS se déclenche uniquement avec le bouton
    }

    private void UpdateLampColor(Color c)
    {
        lampLight.color = c;
    }
}
