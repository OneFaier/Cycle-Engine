using UnityEngine;

[RequireComponent(typeof(Collider))]
public class TurretControllerTrigger : MonoBehaviour
{
    [Header("Références")]
    public Transform turretBarrel;
    public Transform cameraMount;  // Un empty placé EXACTEMENT où doit être la caméra
    public Transform firePoint;
    public GameObject projectilePrefab;
    public HoverSpaceshipAdvanced shipEngine;
    public GameObject fireEffect;

    [Header("Rotation")]
    public float horizontalSensitivity = 2f;
    public float verticalSensitivity = 2f;
    public float rotationSmooth = 12f;
    public float maxPitch = 45f;
    public float minPitch = -5f;

    [Header("Tir")]
    public float projectileSpeed = 80f;
    public float fireCooldown = 0.3f;
    public KeyCode fireKey = KeyCode.Mouse0;
    public AudioClip fireSound;
    public AudioSource fireAudioSource;

    private bool playerInRange = false;
    private float targetYaw = 0f;
    private float targetPitch = 0f;

    private float nextFireTime;
    private Collider turretTrigger;
    private SimpleFPSController playerController;
    private Quaternion initialTurretRotation;
    private Quaternion initialBarrelRotation;


    private void Start()
    {
        turretTrigger = GetComponent<Collider>();
        turretTrigger.isTrigger = true;

        if (!fireAudioSource)
        {
            fireAudioSource = gameObject.AddComponent<AudioSource>();
            fireAudioSource.spatialBlend = 1f;
        }

        initialTurretRotation = transform.localRotation;
        if (turretBarrel)
            initialBarrelRotation = turretBarrel.localRotation;

        if (fireEffect)
            fireEffect.SetActive(false);
    }


    private void Update()
    {
        if (!playerInRange) return;

        HandleRotation();
        HandleFire();
    }


    private void HandleRotation()
    {
        // Input souris
        float mouseX = Input.GetAxis("Mouse X") * horizontalSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * verticalSensitivity;

        targetYaw += mouseX;
        targetPitch -= mouseY;

        targetPitch = Mathf.Clamp(targetPitch, minPitch, maxPitch);

        // Rotation horizontale (base)
        transform.localRotation = Quaternion.Lerp(
            transform.localRotation,
            Quaternion.Euler(0f, targetYaw, 0f),
            Time.deltaTime * rotationSmooth
        );

        // Rotation verticale (canon)
        if (turretBarrel)
        {
            Quaternion barrelRot = Quaternion.Euler(targetPitch, 0f, 0f);
            turretBarrel.localRotation = Quaternion.Lerp(
                turretBarrel.localRotation,
                barrelRot,
                Time.deltaTime * rotationSmooth
            );
        }

        // Caméra suit parfaitement le canon
        if (playerController && cameraMount)
        {
            playerController.playerCamera.transform.position = cameraMount.position;
            playerController.playerCamera.transform.rotation = cameraMount.rotation;
        }
    }


    private void HandleFire()
    {
        if (!Input.GetKey(fireKey) || Time.time < nextFireTime) return;

        if (fireEffect)
        {
            fireEffect.SetActive(true);
            Invoke(nameof(DisableFireEffect), 0.3f);
        }

        if (firePoint && projectilePrefab)
        {
            GameObject proj = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);

            if (proj.TryGetComponent<Rigidbody>(out var rb))
                rb.linearVelocity = firePoint.forward * projectileSpeed;
        }

        if (fireSound && fireAudioSource)
            fireAudioSource.PlayOneShot(fireSound);

        if (shipEngine)
            shipEngine.ApplyCannonImpulse(firePoint);

        nextFireTime = Time.time + fireCooldown;
    }


    private void DisableFireEffect()
    {
        if (fireEffect)
            fireEffect.SetActive(false);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;
        playerController = other.GetComponent<SimpleFPSController>();

        if (playerController)
        {
            playerController.canMove = false;
            playerController.mouseLookEnabled = false; // <-- IMPORTANT
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        ForceExit();
    }


    private void ForceExit()
    {
        playerInRange = false;

        // Reset tourelle
        transform.localRotation = initialTurretRotation;
        if (turretBarrel)
            turretBarrel.localRotation = initialBarrelRotation;

        // Reset player
        if (playerController)
        {
            playerController.canMove = true;
            playerController.mouseLookEnabled = true;
            playerController = null;
        }

        if (fireEffect)
            fireEffect.SetActive(false);

        targetYaw = targetPitch = 0f;
    }
}
