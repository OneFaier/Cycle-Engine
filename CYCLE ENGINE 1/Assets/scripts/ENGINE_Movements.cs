using UnityEngine;
using TMPro;

[RequireComponent(typeof(Rigidbody))]
public class ENGINEMovements : MonoBehaviour
{
    [Header("Boost Particles")]
    public ParticleSystem boostParticles; 

    [Header("Meshes des roues")]
    public Transform frontLeftWheel;
    public Transform frontRightWheel;
    public Transform rearLeftWheel;
    public Transform rearRightWheel;

    [Header("WheelColliders")]
    public WheelCollider frontLeftCollider;
    public WheelCollider frontRightCollider;
    public WheelCollider rearLeftCollider;
    public WheelCollider rearRightCollider;

    [Header("Paramètres véhicule")]
    public float acceleration = 18000f;
    public float turnPower = 250f;
    public float maxSpeed = 50f;
    public float groundDrag = 0.98f;
    public float airControl = 0.4f;

    [Header("Boost")]
    public float boostForce = 35000f;
    public KeyCode boostKey = KeyCode.LeftShift;

    [Header("Physique")]
    public float gravityForce = 50f;
    public float groundCheckDistance = 1.2f;
    public LayerMask groundLayer;

    [Header("Visuel roues")]
    public float wheelRotationSpeed = 5f;
    public float maxSteerAngle = 35f;

    [Header("Reset Vehicle")]
    public KeyCode resetKey = KeyCode.R;
    public float resetHeight = 1.0f;

    [Header("UI")]
    public TextMeshProUGUI speedText;

    [HideInInspector] public bool canControl = false; // contrôle activé seulement quand joueur assis
    [HideInInspector] public Rigidbody rb;

    private bool grounded;
    private float steerInput;
    private float moveInput;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Start()
    {
        rb.mass = 1200f;
        rb.linearDamping = 0.05f;
        rb.angularDamping = 0.3f;
        rb.centerOfMass = new Vector3(0, -0.6f, 0);

        // Bloque la voiture au départ
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.isKinematic = true;
        canControl = false;
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("NoCarCollision"))
            Physics.IgnoreCollision(collision.collider, GetComponent<Collider>());
    }

    void Update()
    {
        if (!canControl) return;

        moveInput = Input.GetAxis("Vertical");
        steerInput = Input.GetAxis("Horizontal");
        grounded = Physics.Raycast(transform.position + Vector3.up * 0.2f, Vector3.down, groundCheckDistance, groundLayer);

        // Boost
        if (Input.GetKey(boostKey))
        {
            rb.AddForce(transform.forward * boostForce * Time.fixedDeltaTime, ForceMode.Force);
            if (boostParticles != null && !boostParticles.isPlaying)
                boostParticles.Play();
        }
        else if (boostParticles != null && boostParticles.isPlaying)
            boostParticles.Stop();

        // Reset voiture
        if (Input.GetKeyDown(resetKey))
            ResetVehicle();

        // Mise à jour vitesse TMP
        if (speedText != null)
        {
            float speed = rb.linearVelocity.magnitude * 3.6f;
            speedText.text = "Vitesse : " + speed.ToString("F1") + " km/h";
        }

        // Visuel roues
        UpdateWheelVisuals();
        UpdateWheelColliders();
    }

    void FixedUpdate()
    {
        if (!canControl) return;

        // Accélération
        if (moveInput != 0f)
        {
            Vector3 force = transform.forward * moveInput * acceleration * Time.fixedDeltaTime;
            if (rb.linearVelocity.magnitude < maxSpeed)
                rb.AddForce(force, ForceMode.Force);
        }

        // Rotation
        if (grounded)
        {
            rb.AddTorque(Vector3.up * steerInput * turnPower * Time.fixedDeltaTime, ForceMode.Force);
            rb.linearVelocity *= groundDrag;
        }
        else
            rb.AddTorque(Vector3.up * steerInput * turnPower * airControl * Time.fixedDeltaTime, ForceMode.Force);

        // Gravité supplémentaire
        rb.AddForce(Vector3.down * gravityForce, ForceMode.Acceleration);
    }

    void UpdateWheelVisuals()
    {
        float wheelSpin = rb.linearVelocity.magnitude * wheelRotationSpeed * Time.deltaTime;

        if (frontLeftWheel != null) frontLeftWheel.Rotate(Vector3.right, wheelSpin, Space.Self);
        if (frontRightWheel != null) frontRightWheel.Rotate(Vector3.right, wheelSpin, Space.Self);
        if (rearLeftWheel != null) rearLeftWheel.Rotate(Vector3.right, wheelSpin, Space.Self);
        if (rearRightWheel != null) rearRightWheel.Rotate(Vector3.right, wheelSpin, Space.Self);

        float steerAngle = steerInput * maxSteerAngle;
        if (frontLeftWheel != null)
        {
            Vector3 rot = frontLeftWheel.localEulerAngles;
            rot.y = steerAngle;
            frontLeftWheel.localEulerAngles = rot;
        }
        if (frontRightWheel != null)
        {
            Vector3 rot = frontRightWheel.localEulerAngles;
            rot.y = steerAngle;
            frontRightWheel.localEulerAngles = rot;
        }
    }

    void UpdateWheelColliders()
    {
        float steerAngle = steerInput * maxSteerAngle;
        if (frontLeftCollider != null) frontLeftCollider.steerAngle = steerAngle;
        if (frontRightCollider != null) frontRightCollider.steerAngle = steerAngle;
    }

    void ResetVehicle()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        transform.rotation = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
        transform.position = new Vector3(transform.position.x, resetHeight, transform.position.z);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = grounded ? Color.green : Color.red;
        Gizmos.DrawLine(transform.position + Vector3.up * 0.2f,
                        transform.position + Vector3.up * 0.2f + Vector3.down * groundCheckDistance);
    }
}
