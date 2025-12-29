using UnityEngine;

public class RadarCameraFollow : MonoBehaviour
{
    [Header("Cible à suivre")]
    public Transform target; // le joueur

    [Header("Décalage caméra")]
    public Vector3 offset = new Vector3(0f, 50f, 0f); // hauteur par défaut

    void LateUpdate()
    {
        if (!target) return;

        // Position = joueur + offset
        transform.position = target.position + offset;

        // Orientation fixe : toujours vers le bas
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }
}