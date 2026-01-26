using UnityEngine;

public class GasBottleSpawnerTrigger : MonoBehaviour
{
    [Header("Tag des objets à contrôler")]
    public string grabbableTag = "Grabbable";

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(grabbableTag))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb)
                rb.isKinematic = true; // bloque la bouteille dans le spawner
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(grabbableTag))
        {
            Rigidbody rb = other.GetComponent<Rigidbody>();
            if (rb)
                rb.isKinematic = false; // la bouteille peut tomber / être grab
        }
    }
}