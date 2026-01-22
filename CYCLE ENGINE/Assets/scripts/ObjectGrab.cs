using UnityEngine;

public class ObjectGrabTMP : MonoBehaviour
{
    [Header("Références")]
    public Camera playerCamera;

    [Header("Paramètres")]
    public float grabRange = 3f;
    public float holdDistance = 2f;

    [Header("Audio")]
    public AudioClip grabSound;
    public AudioClip dropSound;
    public AudioSource audioSource;

    private Rigidbody heldObject;
    private Collider heldCollider;

    void Update()
    {
        HandleGrabToggle();
        HandleHold();
    }

    // =======================
    // GRAB TOGGLE (Left Click)
    // =======================
    void HandleGrabToggle()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (heldObject == null)
        {
            TryGrab();
        }
        else
        {
            DropObject();
        }
    }

    void TryGrab()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (!Physics.Raycast(ray, out RaycastHit hit, grabRange))
            return;

        if (!hit.collider.CompareTag("Grabbable"))
            return;

        Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
        if (rb == null)
            return;

        heldObject = rb;
        heldCollider = hit.collider;

        // 🔓 Si installé → désinstaller VIA LE SLOT
        GasBottleResource bottle = heldObject.GetComponent<GasBottleResource>();
        if (bottle != null && bottle.isInstalled)
        {
            GasBottleSlot slot = bottle.transform.parent?.GetComponent<GasBottleSlot>();
            if (slot != null)
                slot.Uninstall();
        }

        // --- RESET PHYSIQUE ---
        heldObject.transform.SetParent(null);
        heldObject.isKinematic = false;
        heldObject.useGravity = false;
        heldObject.linearVelocity = Vector3.zero;
        heldObject.angularVelocity = Vector3.zero;

        // Éviter collisions joueur
        if (heldCollider)
            heldCollider.enabled = false;

        if (audioSource && grabSound)
            audioSource.PlayOneShot(grabSound);
    }

    void DropObject()
    {
        // --- RETOUR PHYSIQUE NORMAL ---
        heldObject.useGravity = true;
        heldObject.isKinematic = false;

        if (heldCollider)
            heldCollider.enabled = true;

        if (audioSource && dropSound)
            audioSource.PlayOneShot(dropSound);

        heldObject = null;
        heldCollider = null;
    }

    // =======================
    // MAINTIEN
    // =======================
    void HandleHold()
    {
        if (heldObject == null)
            return;

        Vector3 targetPos =
            playerCamera.transform.position +
            playerCamera.transform.forward * holdDistance;

        heldObject.MovePosition(targetPos);
    }
}
