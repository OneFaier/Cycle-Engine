using UnityEngine;

public enum GasSlotType { Speed, Direction }

public class GasBottleSlot : MonoBehaviour
{
    [Header("Type de slot")]
    public GasSlotType slotType;

    [Header("Bouteille actuelle")]
    public GasBottleResource currentBottle;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip addModuleClip;
    public AudioClip removeModuleClip;

    public bool IsFree => currentBottle == null;

    // ===================== PREVIEW =====================
    void PreviewSnap(GasBottleResource bottle)
    {
        bottle.previewSlot = this;

        bottle.transform.position = transform.position;
        bottle.transform.rotation = Quaternion.Euler(-90f, 0f, 0f);
        bottle.transform.localScale = bottle.originalScale;
    }

    // ===================== INSTALL =====================
    public bool TryInstall(GasBottleResource bottle)
    {
        if (!IsFree || bottle == null) return false;

        currentBottle = bottle;
        bottle.previewSlot = null;
        bottle.isInstalled = true;
        bottle.isActive = false;

        if (audioSource && addModuleClip)
            audioSource.PlayOneShot(addModuleClip);

        bottle.transform.SetParent(transform);
        bottle.transform.localPosition = Vector3.zero;
        bottle.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        bottle.transform.localScale = bottle.originalScale;

        Rigidbody rb = bottle.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        return true;
    }

    public void Uninstall()
    {
        if (currentBottle == null) return;

        GasBottleResource bottle = currentBottle;
        currentBottle = null;

        if (audioSource && removeModuleClip)
            audioSource.PlayOneShot(removeModuleClip);

        bottle.isInstalled = false;
        bottle.transform.SetParent(null);

        Rigidbody rb = bottle.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    // ===================== TRIGGERS =====================
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Grabbable")) return;

        GasBottleResource bottle = other.GetComponent<GasBottleResource>();
        if (bottle == null || bottle.isInstalled) return;

        if (bottle.isGrabbed)
            PreviewSnap(bottle);   // 👻 prévisualisation
        else
            TryInstall(bottle);   // 📦 auto-install si pas grab
    }

    private void OnTriggerExit(Collider other)
    {
        GasBottleResource bottle = other.GetComponent<GasBottleResource>();
        if (bottle && bottle.previewSlot == this)
            bottle.previewSlot = null;
    }
}
