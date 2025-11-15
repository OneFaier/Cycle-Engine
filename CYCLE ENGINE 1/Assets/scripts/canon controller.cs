using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TurretControllerTrigger : MonoBehaviour
{
    [Header("Références")]
    public Transform turretBarrel;   // Pivot vertical du canon
    public Transform firePoint;      // Point de tir
    public GameObject projectilePrefab;
    public HoverSpaceshipAdvanced shipEngine; // Référence au vaisseau pour le recul

    [Header("Rotation")]
    public float horizontalSensitivity = 2f;
    public float verticalSensitivity = 2f;
    public float rotationSmooth = 10f;
    public float maxPitch = 60f;
    public float minPitch = -10f;
    public float maxYaw = 180f;
    public float minYaw = -180f;

    [Header("Tir")]
    public float projectileSpeed = 80f;
    public float fireCooldown = 0.3f;
    public KeyCode fireKey = KeyCode.Mouse0;
    public AudioClip fireSound;
    public AudioSource fireAudioSource;

    private bool playerInRange = false;
    private float targetYaw, targetPitch;
    private float currentYaw, currentPitch;
    private float nextFireTime;

    private Quaternion initialTurretRotation;
    private Quaternion initialBarrelRotation;
    private Quaternion initialPlayerRotation;

    private SimpleFPSController playerController;

    private void Start()
    {
        // Collider en trigger
        GetComponent<Collider>().isTrigger = true;

        // AudioSource si non assigné
        if (!fireAudioSource)
        {
            fireAudioSource = gameObject.AddComponent<AudioSource>();
            fireAudioSource.spatialBlend = 1f;
        }

        // Stocke les rotations initiales
        initialTurretRotation = transform.localRotation;
        if (turretBarrel)
            initialBarrelRotation = turretBarrel.localRotation;
    }

    private void Update()
    {
        if (!playerInRange) return;

        HandleRotation();
        HandleFire();
    }

    private void HandleRotation()
    {
        float mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity;

        targetYaw += mouseX;
        targetPitch -= mouseY;

        targetYaw = Mathf.Clamp(targetYaw, minYaw, maxYaw);
        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        currentYaw = Mathf.Lerp(currentYaw, targetYaw, Time.deltaTime * rotationSmooth);
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * rotationSmooth);

        // Rotation de la base en Y
        transform.localRotation = Quaternion.Euler(0f, currentYaw, 0f);

        // Inclinaison du canon en X
        if (turretBarrel)
            turretBarrel.localRotation = Quaternion.Euler(currentPitch, 0f, 0f);

        // Caméra verrouillée sur la tourelle
        if (playerController)
        {
            playerController.transform.rotation = transform.rotation;
            playerController.playerCamera.transform.localRotation =
                Quaternion.Euler(currentPitch, 0f, 0f);
        }
    }

    private void HandleFire()
    {
        if (!Input.GetKey(fireKey) || Time.time < nextFireTime) return;

        if (firePoint && projectilePrefab)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
            if (proj.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = firePoint.forward * projectileSpeed;
        }

        if (fireSound && fireAudioSource)
            fireAudioSource.PlayOneShot(fireSound);

        // Recul physique du vaisseau
        if (shipEngine != null)
            shipEngine.ApplyCannonImpulse(firePoint);

        nextFireTime = Time.time + fireCooldown;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        playerController = other.GetComponent<SimpleFPSController>();

        if (playerController)
        {
            playerController.canMove = false;
            initialPlayerRotation = playerController.transform.rotation;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        // Reset tourelle
        transform.localRotation = initialTurretRotation;
        if (turretBarrel)
            turretBarrel.localRotation = initialBarrelRotation;

        // Reset joueur
        if (playerController)
        {
            playerController.transform.rotation = initialPlayerRotation;
            playerController.canMove = true;
            playerController = null;
        }

        // Reset interne
        targetYaw = currentYaw = 0f;
        targetPitch = currentPitch = 0f;
    }
}
