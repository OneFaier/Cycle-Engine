using UnityEngine;

public class TurretControllerStable : MonoBehaviour
{
    [Header("Références")]
    public Transform turretBase;
    public Transform turretBarrel;
    public Transform firePoint;
    public GameObject projectilePrefab;

    [Header("Moteur (recul / physique)")]
    public HoverSpaceshipAdvanced engine;

    [Header("Tir")]
    public float projectileSpeed = 80f;
    public float fireCooldown = 0.3f;
    public KeyCode fireKey = KeyCode.Mouse0;

    [Header("Rotation")]
    public float maxPitch = 60f;
    public float minPitch = -10f;
    public float horizontalSensitivity = 2f;
    public float verticalSensitivity = 2f;

    private float nextFireTime;
    private float currentYaw;
    private float currentPitch;

    void Update()
    {
        HandleRotation();
        HandleFire();
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity;

        currentYaw += mouseX;
        currentPitch -= mouseY;
        currentPitch = Mathf.Clamp(currentPitch, minPitch, maxPitch);

        if (turretBase)
            turretBase.localRotation = Quaternion.Euler(0f, currentYaw, 0f);
        if (turretBarrel)
            turretBarrel.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
    }

    private void HandleFire()
    {
        if (Input.GetKey(fireKey) && Time.time >= nextFireTime && firePoint && projectilePrefab)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            if (proj.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = firePoint.forward * projectileSpeed;

            if (engine != null)
                engine.ApplyCannonImpulse(firePoint.forward);

            nextFireTime = Time.time + fireCooldown;
        }
    }
}