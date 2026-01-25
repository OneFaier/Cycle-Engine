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

    // ===================== INSTALL =====================
    public bool TryInstall(GasBottleResource bottle)
    {
        if (!IsFree || bottle == null) return false;

        currentBottle = bottle;
        bottle.previewSlot = null;
        bottle.isInstalled = true;
        bottle.isActive = false;

        // Parent + position
        bottle.transform.SetParent(transform);
        bottle.transform.localPosition = Vector3.zero;
        bottle.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        bottle.transform.localScale = bottle.originalScale;

        // Rigidbody
        Rigidbody rb = bottle.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        // ⚡ Layer pour désactiver collisions avec le vaisseau
        bottle.gameObject.layer = LayerMask.NameToLayer("Vehicule");

        // Son
        if (audioSource && addModuleClip)
            audioSource.PlayOneShot(addModuleClip);

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
            rb.useGravity = false;
        }

        // ⚡ Remet layer par défaut pour collisions normales
        bottle.gameObject.layer = LayerMask.NameToLayer("Default");
    }
}
