using UnityEngine;
using TMPro;

public class ObjectGrabTMP : MonoBehaviour
{
    [Header("Références")]
    public Camera playerCamera;
    public TextMeshProUGUI interactText; // Texte affiché quand on regarde un objet grabbable

    [Header("Paramètres")]
    public float grabRange = 3f;
    public float holdDistance = 2f;

    private Rigidbody heldObject;

    void Start()
    {
        if (interactText != null)
            interactText.gameObject.SetActive(false); // Caché au démarrage
    }

    void Update()
    {
        // --- Vérifie s’il y a un objet grabbable devant ---
        bool canGrab = false;

        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, grabRange))
        {
            if (hit.collider.CompareTag("Grabbable"))
            {
                canGrab = true;
            }
        }

        // --- Gère l’affichage du texte TMP ---
        if (interactText != null)
            interactText.gameObject.SetActive(canGrab && heldObject == null);

        // --- Attraper un objet (clic gauche) ---
        if (Input.GetMouseButtonDown(0) && heldObject == null && canGrab)
        {
            Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
            if (rb != null)
            {
                heldObject = rb;
                heldObject.useGravity = false;

                // Détacher si déjà parenté
                if (heldObject.transform.parent != null)
                    heldObject.transform.SetParent(null);
            }
        }

        // --- Lâcher (clic droit) ---
        if (Input.GetMouseButtonDown(1) && heldObject != null)
        {
            heldObject.useGravity = true;
            heldObject = null;
        }

        // --- Maintenir devant soi ---
        if (heldObject != null)
        {
            Vector3 targetPos = playerCamera.transform.position + playerCamera.transform.forward * holdDistance;
            heldObject.position = targetPos;
        }
    }
}
