using UnityEngine;

public class TurretControllerStable : MonoBehaviour
{
    [Header("Références")]
    public Transform turretPivot;     // 🔹 Point autour duquel tourne la tourelle (pivot horizontal)
    public Transform turretBarrel;    // 🔹 Partie qui s’incline verticalement
    public Transform firePoint;       // 🔹 Point de sortie du projectile
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
    public float rotationSmooth = 10f;

    private float nextFireTime;
    private float targetYaw;
    private float targetPitch;
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

        // 🔹 MàJ des cibles
        targetYaw += mouseX;
        targetPitch -= mouseY;
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        // 🔹 Interpolation fluide
        currentYaw = Mathf.Lerp(currentYaw, targetYaw, Time.deltaTime * rotationSmooth);
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * rotationSmooth);

        // 🔹 Application de la rotation autour du pivot
        if (turretPivot)
            turretPivot.localRotation = Quaternion.Euler(0f, currentYaw, 0f);

        if (turretBarrel)
            turretBarrel.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);
    }

    private void HandleFire()
    {
        if (Input.GetKey(fireKey) && Time.time >= nextFireTime && firePoint && projectilePrefab)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            if (proj.TryGetComponent<Rigidbody>(out var rb))
#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = firePoint.forward * projectileSpeed;
#else
                rb.velocity = firePoint.forward * projectileSpeed;
#endif

            if (engine != null)
                engine.ApplyCannonImpulse(firePoint.forward);

            nextFireTime = Time.time + fireCooldown;
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        if (firePoint)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.forward * 5f);
        }

        if (turretPivot)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawSphere(turretPivot.position, 0.15f);
        }
    }
#endif
}
