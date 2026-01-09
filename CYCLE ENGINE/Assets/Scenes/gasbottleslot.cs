using UnityEngine;

public enum GasSlotType { Speed, Direction }

public class GasBottleSlot : MonoBehaviour
{
    [Header("Type de slot")]
    public GasSlotType slotType;

    [Header("Bouteille actuelle")]
    public GasBottleResource currentBottle;

    public bool IsFree => currentBottle == null;

    public bool TryInstall(GasBottleResource bottle)
    {
        if (!IsFree || bottle == null) return false;

        currentBottle = bottle;

        bottle.isInstalled = true;
        bottle.isActive = false; // activée par le manager

        // Parent et position
        bottle.transform.SetParent(transform);
        bottle.transform.localPosition = Vector3.zero;
        bottle.transform.localScale = bottle.originalScale;

        // Rotation immédiate à -90° sur X
        bottle.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);

        Rigidbody rb = bottle.GetComponent<Rigidbody>();
        if (rb) Destroy(rb);

        return true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Grabbable")) return;

        GasBottleResource bottle = other.GetComponent<GasBottleResource>();
        if (bottle == null) return;

        TryInstall(bottle);
    }
}