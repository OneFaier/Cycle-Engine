using UnityEngine;

public enum GasSlotType { Speed, Direction }

public class GasBottleSlot : MonoBehaviour
{
    [Header("Type de slot")]
    public GasSlotType slotType;

    [Header("Bouteille actuelle")]
    public GasBottleResource currentBottle;

    [Header("Audio")]
    public AudioSource audioSource;       // AudioSource pour jouer les sons
    public AudioClip addModuleClip;       // Son quand une bouteille est ajoutée
    public AudioClip removeModuleClip;    // Son quand une bouteille est retirée

    public bool IsFree => currentBottle == null;

    public bool TryInstall(GasBottleResource bottle)
    {
        if (!IsFree || bottle == null) return false;

        currentBottle = bottle;

        // 🔊 Jouer le son d'ajout
        if (audioSource != null && addModuleClip != null)
            audioSource.PlayOneShot(addModuleClip, 1f);

        bottle.isInstalled = true;
        bottle.isActive = false;

        // Parent & transform
        bottle.transform.SetParent(transform);
        bottle.transform.localPosition = Vector3.zero;
        bottle.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
        bottle.transform.localScale = bottle.originalScale;

        // 🔒 PHYSIQUE BLOQUÉE MAIS PAS DÉTRUITE
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

        // 🔊 Jouer le son de retrait
        if (audioSource != null && removeModuleClip != null)
            audioSource.PlayOneShot(removeModuleClip, 1f);

        bottle.isInstalled = false;

        bottle.transform.SetParent(null);

        Rigidbody rb = bottle.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Grabbable")) return;

        GasBottleResource bottle = other.GetComponent<GasBottleResource>();
        if (bottle == null || bottle.isInstalled) return;

        TryInstall(bottle);
    }
}
