using UnityEngine;

public class SlidingDoorWithAudio : MonoBehaviour
{
    [Header("Positions")]
    public Transform closedPosition;
    public Transform openPosition;
    public float slideSpeed = 2f;

    [Header("SAS Options")]
    public bool isSasDoor = false;            // ⭐ Cette porte appartient au SAS
    public SasController sasController;       // ⭐ Référence au SAS

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

    // 🔥 OUVERURE PROTÉGÉE
    public void ToggleDoor()
    {
        if (isOpen)  // on veut fermer → toujours autorisé
        {
            CloseDoor();
            return;
        }

        // On veut OUVRIR la porte
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

    // ⛔ Conditions d'ouverture
    private bool CanOpenDoor()
    {
        // Si c’est une porte SAS → elle NE PEUT PAS s’ouvrir tant que le SAS tourne
        if (isSasDoor && sasController != null)
        {
            if (sasController.SasInProgress)
                return false;  // ⛔ SAS actif

            // ⛔ SAS ne doit être ni en remplissage ni en vidange
        }

        return true;
    }

    // ⭐ Utilisé par le SasController pour vérifier si on peut lancer un cycle
    public bool IsFullyClosed()
    {
        return Vector3.Distance(transform.position, closedPosition.position) < 0.01f;
    }

    public bool IsFullyOpen()
    {
        return Vector3.Distance(transform.position, openPosition.position) < 0.01f;
    }
}
