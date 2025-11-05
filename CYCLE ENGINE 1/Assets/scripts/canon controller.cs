using UnityEngine;

public class TurretController : MonoBehaviour
{
    [Header("Références")]
    public Transform turretBase;      // Partie qui tourne horizontalement (axe Y)
    public Transform turretBarrel;    // Partie qui monte/descend (axe X)
    public Transform firePoint;       // Embout du canon
    public GameObject projectilePrefab;

    [Header("Paramètres")]
    public float rotationSpeed = 5f;
    public float projectileSpeed = 80f;
    public float fireCooldown = 0.3f;
    public KeyCode fireKey = KeyCode.Mouse0;

    private Camera playerCamera;
    private bool isActive = false;
    private float nextFireTime;

    public void ActivateTurret(Camera cam, bool state)
    {
        isActive = state;
        playerCamera = cam;
    }

    private void Update()
    {
        if (!isActive || !playerCamera) return;

        HandleRotation();

        if (Input.GetKey(fireKey) && Time.time >= nextFireTime)
        {
            Fire();
            nextFireTime = Time.time + fireCooldown;
        }
    }

    private void HandleRotation()
    {
        // Rotation horizontale (base)
        Vector3 camYaw = playerCamera.transform.eulerAngles;
        turretBase.rotation = Quaternion.Lerp(
            turretBase.rotation,
            Quaternion.Euler(0, camYaw.y, 0),
            Time.deltaTime * rotationSpeed
        );

        // Rotation verticale (canon)
        Vector3 camPitch = playerCamera.transform.localEulerAngles;
        float xRot = camPitch.x > 180 ? camPitch.x - 360 : camPitch.x;
        xRot = Mathf.Clamp(xRot, -45, 45); // Limite d'angle du canon
        turretBarrel.localRotation = Quaternion.Lerp(
            turretBarrel.localRotation,
            Quaternion.Euler(xRot, 0, 0),
            Time.deltaTime * rotationSpeed
        );
    }

    private void Fire()
    {
        if (!firePoint || !projectilePrefab) return;

        GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        if (proj.TryGetComponent<Rigidbody>(out var rb))
            rb.linearVelocity = firePoint.forward * projectileSpeed;
    }
}
