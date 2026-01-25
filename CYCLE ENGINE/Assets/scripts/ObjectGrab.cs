using UnityEngine;

public class ObjectGrabTMP : MonoBehaviour
{
    [Header("Références")]
    public Camera playerCamera;

    public Rigidbody HeldObject => heldObject;

    [Header("Paramètres Grab")]
    public float grabRange = 3f;
    public float holdDistance = 2f;
    public float moveSpeed = 15f;
    public float slotSnapSpeed = 10f;

    [Header("Hold Offset (visuel uniquement)")]
    public Vector3 holdOffset = new Vector3(0.35f, -0.15f, 0f);

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
        // 🔥 Raycast CENTRE caméra (inchangé)
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

        heldObject.isKinematic = false;
        heldObject.useGravity = false;

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

            if (bottle.previewSlot != null)
            {
                bottle.previewSlot.TryInstall(bottle);
                bottle.previewSlot = null;
            }
        }

        heldObject.useGravity = true;

        if (audioSource && dropSound)
            audioSource.PlayOneShot(dropSound);

        heldObject = null;
        heldCollider = null;
    }

    // ===================== HOLD + PRÉVISUALISATION =====================
    void HandleHold()
    {
        if (heldObject == null) return;

        GasBottleResource bottle = heldObject.GetComponent<GasBottleResource>();

        Transform cam = playerCamera.transform;

        // 🎯 Position décalée visuellement (raycast inchangé)
        Vector3 targetPos =
            cam.position
            + cam.forward * holdDistance
            + cam.right * holdOffset.x
            + cam.up * holdOffset.y;

        // --- Raycast slot ---
        Ray ray = new Ray(cam.position, cam.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, grabRange))
        {
            GasBottleSlot slot = hit.collider.GetComponent<GasBottleSlot>();
            if (slot != null && slot.IsFree)
            {
                bottle.previewSlot = slot;

                Vector3 slotPos = slot.transform.position;
                heldObject.MovePosition(
                    Vector3.Lerp(
                        heldObject.position,
                        slotPos,
                        slotSnapSpeed * Time.deltaTime
                    )
                );

                Quaternion targetRot = Quaternion.Euler(-90f, 0f, 0f);
                heldObject.rotation = Quaternion.Slerp(
                    heldObject.rotation,
                    targetRot,
                    0.1f
                );

                return;
            }
            else
            {
                bottle.previewSlot = null;
            }
        }
        else
        {
            bottle.previewSlot = null;
        }

        // --- Hold normal ---
        Vector3 moveDir = (targetPos - heldObject.position) * moveSpeed;
        heldObject.linearVelocity = Vector3.Lerp(
            heldObject.linearVelocity,
            moveDir,
            0.2f
        );

        heldObject.rotation = Quaternion.Slerp(
            heldObject.rotation,
            Quaternion.identity,
            0.05f
        );

        heldObject.transform.localScale = bottle.originalScale;
    }
}
