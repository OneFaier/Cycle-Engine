using UnityEngine;

public class AttachToVehicle : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Le joueur devient enfant du véhicule
            other.transform.SetParent(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Le joueur redevient indépendant
            other.transform.SetParent(null);
        }
    }
}
