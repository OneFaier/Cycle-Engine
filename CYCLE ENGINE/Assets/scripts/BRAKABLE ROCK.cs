using UnityEngine;

public class BreakableRock : MonoBehaviour
{
    private Rigidbody[] fragments;

    void Start()
    {
        // Récupère automatiquement tous les Rigidbody enfants
        fragments = GetComponentsInChildren<Rigidbody>();

        // Au départ, tout est kinematic
        foreach (Rigidbody rb in fragments)
            rb.isKinematic = true;
    }

    // Méthode publique pour débloquer tous les fragments
    public void Break()
    {
        foreach (Rigidbody rb in fragments)
            rb.isKinematic = false;
    }
}