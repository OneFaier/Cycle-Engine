using UnityEngine;

public enum GasSlotType { Speed, Direction }

public class GasBottleSlot : MonoBehaviour
{
    [Header("Type de slot")]
    public GasSlotType slotType;

    [Header("Bouteille actuelle")]
    public GasBottleResource currentBottle;

    [Header("Visuel (Sprite)")]
    public SpriteRenderer slotRenderer;
    public Color emptyColor = Color.black;
    public Color filledColor = new Color(0.5f, 0.8f, 1f); // bleu ciel

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip addModuleClip;
    public AudioClip removeModuleClip;

    public bool IsFree => currentBottle == null;

    void Start()
    {
        UpdateSlotColor();
    }

    // ===================== INSTALL =====================
    public bool TryInstall(GasBottleResource bottle)
    {
        if (!IsFree || bottle == null) return false;

        currentBottle = bottle;
        bottle.previewSlot = null;
        bottle.isInstalled = true;
        bottle.isActive = false;

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

        bottle.gameObject.layer = LayerMask.NameToLayer("Vehicule");

        if (audioSource && addModuleClip)
            audioSource.PlayOneShot(addModuleClip);

        UpdateSlotColor();
        return true;
    }

    // ===================== UNINSTALL =====================
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

        bottle.gameObject.layer = LayerMask.NameToLayer("Default");

        UpdateSlotColor();
    }

    // ===================== VISUEL =====================
    void UpdateSlotColor()
    {
        if (slotRenderer == null) return;
        slotRenderer.color = IsFree ? emptyColor : filledColor;
    }
}
