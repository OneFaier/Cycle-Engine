using UnityEngine;

public class GasBottleCheat : MonoBehaviour
{
    [Header("Référence aux slots")]
    public GasBottleSlot[] allSlots;

    [Header("Bouteille à installer pour le cheat")]
    public GasBottleResource cheatBottlePrefab;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            ActivateCheat();
        }
    }

    void ActivateCheat()
    {
        if (allSlots == null || cheatBottlePrefab == null) return;

        foreach (var slot in allSlots)
        {
            if (slot.IsFree)
            {
                // Instancier une bouteille pour le slot
                GasBottleResource newBottle = Instantiate(cheatBottlePrefab);
                slot.TryInstall(newBottle);
            }
        }

        Debug.Log("Cheat activé : tous les slots remplis !");
    }
}