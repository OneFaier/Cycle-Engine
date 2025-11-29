using UnityEngine;

public class HandFollowCamera : MonoBehaviour
{
    [Header("Camera à suivre")]
    public Transform fpsCamera;

    [Header("Offset par rapport à la caméra")]
    public Vector3 positionOffset = new Vector3(0.5f, -0.5f, 1f);

    [Header("Lissage du mouvement")]
    public float followSpeed = 12f;
    public float rotationSpeed = 12f;

    [Header("Peek (décalage du corps)")]
    public float peekAmount = 0.3f; 
    public float peekSpeed = 10f;

    private float currentPeek = 0f;

    void Update()
    {
        if (!fpsCamera) return;

        // 🔵 1) Gestion du peek (Q = gauche, E = droite)
        float targetPeek = 0f;
        if (Input.GetKey(KeyCode.Q)) targetPeek = -peekAmount;
        if (Input.GetKey(KeyCode.E)) targetPeek = peekAmount;

        currentPeek = Mathf.Lerp(currentPeek, targetPeek, Time.deltaTime * peekSpeed);

        // 🔵 2) Position désirée
        Vector3 targetPos =
            fpsCamera.position
            + fpsCamera.transform.right * currentPeek   // peek
            + fpsCamera.transform.TransformDirection(positionOffset);

        // 🔵 3) Suivi position
        transform.position = Vector3.Lerp(
            transform.position,
            targetPos,
            Time.deltaTime * followSpeed
        );

        // 🔵 4) Suivi rotation
        transform.rotation = Quaternion.Lerp(
            transform.rotation,
            fpsCamera.rotation,
            Time.deltaTime * rotationSpeed
        );
    }
}
