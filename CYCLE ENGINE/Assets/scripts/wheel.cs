using UnityEngine;

public class SimpleWheelVisual : MonoBehaviour
{
    public WheelCollider collider;
    public Transform mesh;

    void Update()
    {
        if (!collider || !mesh) return;

        collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
        mesh.position = pos;
        mesh.rotation = rot;
    }
}
