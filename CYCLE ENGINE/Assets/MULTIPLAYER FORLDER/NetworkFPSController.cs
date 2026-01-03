using UnityEngine;
using Fusion;

namespace Starter.Shooter
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PlayerFPS : NetworkBehaviour
    {
        [Header("References")]
        public Transform BodySphere;
        public Camera PlayerCamera;

        [Header("Movement Settings")]
        public float moveSpeed = 5f;
        public float jumpForce = 5f;

        [Header("Rotation Settings")]
        public float MouseSensitivity = 2f;
        public float MaxLookAngle = 60f;
        public float MinLookAngle = -60f;

        [Networked, HideInInspector]
        public float NetworkYaw { get; set; }

        private Rigidbody rb;
        private float yaw;
        private float pitch;
        private bool isGrounded;

        public override void Spawned()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;

            yaw = BodySphere.localEulerAngles.y;
            pitch = PlayerCamera.transform.localEulerAngles.x;

            if (Object.HasInputAuthority && PlayerCamera != null)
            {
                PlayerCamera.gameObject.SetActive(true);
                var listener = PlayerCamera.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = true;
            }
            else if (PlayerCamera != null)
            {
                PlayerCamera.gameObject.SetActive(false);
                var listener = PlayerCamera.GetComponent<AudioListener>();
                if (listener != null) listener.enabled = false;
            }
        }

        public override void FixedUpdateNetwork()
        {
            if (!Object.HasInputAuthority) return;

            NetworkYaw = yaw;

            // Movement networked ici si besoin plus tard
            Move();
        }

        private void Update()
        {
            if (!Object.HasInputAuthority || BodySphere == null || PlayerCamera == null)
            {
                // Synchronisation de la rotation du corps pour les autres joueurs
                if (BodySphere != null)
                    BodySphere.localRotation = Quaternion.Euler(0f, NetworkYaw, 0f);
                return;
            }

            Look();
        }

        private void Look()
        {
            float mouseX = Input.GetAxis("Mouse X") * MouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * MouseSensitivity;

            yaw += mouseX;
            pitch -= mouseY;
            pitch = Mathf.Clamp(pitch, MinLookAngle, MaxLookAngle);

            BodySphere.localRotation = Quaternion.Euler(0f, yaw, 0f);
            PlayerCamera.transform.localRotation = Quaternion.Euler(pitch, 0f, 0f);
        }

        private void Move()
        {
            float x = Input.GetAxisRaw("Horizontal");
            float z = Input.GetAxisRaw("Vertical");

            Vector3 moveDir = (transform.right * x + transform.forward * z).normalized;
            Vector3 velocity = new Vector3(moveDir.x * moveSpeed, rb.linearVelocity.y, moveDir.z * moveSpeed);

            rb.linearVelocity = velocity;

            if (Input.GetButtonDown("Jump") && isGrounded)
            {
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
                isGrounded = false;
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.CompareTag("Ground"))
                isGrounded = true;
        }
    }
}
