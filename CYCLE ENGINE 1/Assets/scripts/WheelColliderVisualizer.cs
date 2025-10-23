using UnityEngine;

[ExecuteInEditMode]
public class WheelColliderVisualizer : MonoBehaviour
{
    public WheelCollider wheel;

    void OnDrawGizmos()
    {
        if (!wheel) return;

        Vector3 pos;
        Quaternion rot;

        // Essaie de récupérer la pose
        try
        {
            wheel.GetWorldPose(out pos, out rot);
        }
        catch
        {
            return; // Si la roue n'est pas initialisée, on quitte
        }

        // Si le quaternion est invalide (longueur zéro), on le remplace par une rotation neutre
        if (rot == Quaternion.identity || rot.x == 0 && rot.y == 0 && rot.z == 0 && rot.w == 0)
            rot = Quaternion.Euler(Vector3.zero);

        // --- Dessin des gizmos ---
        Gizmos.color = Color.green;
        Gizmos.matrix = Matrix4x4.TRS(pos, rot, Vector3.one);

        // Cercle qui représente le pneu
        Gizmos.DrawWireSphere(Vector3.zero, wheel.radius);

        // Ligne rouge pour la suspension
        Gizmos.color = Color.red;
        Gizmos.DrawLine(Vector3.zero, Vector3.down * wheel.suspensionDistance);
    }
}
