using UnityEngine;

public class BackDoor : MonoBehaviour
{
    [Header("Pivot et rotation")]
    public Transform pivot;           // Empty autour duquel la porte tourne
    public float openAngle = 90f;     // Angle d'ouverture
    public float openSpeed = 90f;     // Degrés par seconde

    private bool isOpen = false;
    private float currentAngle = 0f;  // Angle ouvert par rapport à la rotation initiale

    void Start()
    {
        if (pivot == null) pivot = transform; // fallback si aucun pivot assigné
    }

    void Update()
    {
        RotateDoor();
    }

    void RotateDoor()
    {
        float targetAngle = isOpen ? openAngle : 0f;
        float step = openSpeed * Time.deltaTime;

        float delta = targetAngle - currentAngle;
        if (Mathf.Abs(delta) > 0.01f)
        {
            float rotateStep = Mathf.Sign(delta) * Mathf.Min(Mathf.Abs(delta), step);
            transform.RotateAround(pivot.position, Vector3.right, rotateStep);
            currentAngle += rotateStep;
        }
    }

    // Appel public pour ouvrir/fermer la porte
    public void ToggleDoor()
    {
        isOpen = !isOpen;
    }
}