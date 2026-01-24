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

    // ===================== GRAB =====================
    void HandleGrabToggle()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        if (heldObject == null)
            TryGrab();
        else
            DropObject();
    }

    void TryGrab()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        if (!Physics.Raycast(ray, out RaycastHit hit, grabRange)) return;
        if (!hit.collider.CompareTag("Grabbable")) return;

        Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
        if (!rb) return;

        heldObject = rb;
        heldCollider = hit.collider;

        GasBottleResource bottle = heldObject.GetComponent<GasBottleResource>();
        if (bottle != null)
        {
            bottle.isGrabbed = true;

            if (bottle.isInstalled)
            {
                GasBottleSlot slot = bottle.transform.parent?.GetComponent<GasBottleSlot>();
                if (slot != null)
                    slot.Uninstall();
            }
        }

        heldObject.transform.SetParent(null);
        heldObject.isKinematic = false;
        heldObject.useGravity = false;
        heldObject.linearVelocity = Vector3.zero;
        heldObject.angularVelocity = Vector3.zero;

        if (heldCollider)
            heldCollider.enabled = false;

        if (audioSource && grabSound)
            audioSource.PlayOneShot(grabSound);
    }

    void DropObject()
    {
        if (heldObject == null) return;

        GasBottleResource bottle = heldObject.GetComponent<GasBottleResource>();
        if (bottle != null)
        {
            bottle.isGrabbed = false;

            // installation finale si previewSlot
            if (bottle.previewSlot != null)
            {
                bottle.previewSlot.TryInstall(bottle);
                bottle.previewSlot = null;
            }
        }

        heldObject.useGravity = true;
        heldObject.isKinematic = false;

        if (heldCollider)
            heldCollider.enabled = true;

        if (audioSource && dropSound)
            audioSource.PlayOneShot(dropSound);

        heldObject = null;
        heldCollider = null;
    }

    // ===================== HOLD =====================
    void HandleHold()
    {
        if (heldObject == null) return;

        GasBottleResource bottle = heldObject.GetComponent<GasBottleResource>();

        if (bottle != null && bottle.previewSlot != null)
        {
            heldObject.MovePosition(bottle.previewSlot.transform.position);
            heldObject.MoveRotation(bottle.previewSlot.transform.rotation);
        }
        else
        {
            Vector3 targetPos =
                playerCamera.transform.position +
                playerCamera.transform.forward * holdDistance;

            heldObject.MovePosition(targetPos);
        }
    }
}
    