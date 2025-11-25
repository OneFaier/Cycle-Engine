using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlayerShipTrigger : MonoBehaviour
{
    public Rigidbody shipRb; // Rigidbody du vaisseau
    private GameObject player;
    private Rigidbody playerRb;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.gameObject;
            playerRb = player.GetComponent<Rigidbody>();

            // Désactive la physique globale pour éviter l’éjection
            if (playerRb != null) playerRb.isKinematic = true;

            // Met le joueur en enfant pour suivre rotation + translation
            player.transform.SetParent(shipRb.transform);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && player != null)
        {
            // Rétablit la physique
            if (playerRb != null) playerRb.isKinematic = false;

            // Défait le parent
            player.transform.SetParent(null);

            player = null;
            playerRb = null;
        }
    }
}