    using UnityEngine;

    public class IndicatorMouseClickFast : MonoBehaviour
    {
        public enum CubeType { Vitesse, Direction }

        [Header("Type de Cube")]
        public CubeType cubeType = CubeType.Vitesse;

        [Header("Rail")]
        public Transform railStart;
        public Transform railEnd;

        [Header("Contrôles")]
        public float sensitivity = 0.05f;
        [Range(0f, 1f)] public float positionNormalized = 0.5f;
        public float snapThreshold = 0.05f;
        public float maxGrabDistance = 3f;
        public float returnSpeed = 2f;

        [Header("Surbrillance")]
        public Color highlightColor = Color.yellow;
        private Color baseColor;
        private Renderer rend;

        private Camera playerCamera;
        private bool isGrabbed = false;
        private bool isHovered = false;

        // Anti-clignotement
        private float lastValidHoverTime = -1f;
        private float hoverMemoryDuration = 0.1f;

        // 🔒 Gestion du curseur
        private bool cursorWasLockedBefore = false;
        private Vector3 grabScreenPos;

        void Start()
        {
            playerCamera = Camera.main;
            rend = GetComponent<Renderer>();
            if (rend != null)
                baseColor = rend.material.color;
        }

        void Update()
        {
            HandleHover();

            // Commencer le grab
            if (Input.GetMouseButtonDown(0) && isHovered && !isGrabbed)
                StartGrab();

            // Relâcher
            if (Input.GetMouseButtonUp(0) && isGrabbed)
                StopGrab();

            // Déplacement si attrapé
            if (isGrabbed)
            {
                float mouseX = Input.GetAxis("Mouse X");
                positionNormalized += mouseX * sensitivity;
                positionNormalized = Mathf.Clamp01(positionNormalized);
            }
            else
            {
                if (cubeType == CubeType.Direction)
                {
                    // Retour progressif au centre
                    positionNormalized = Mathf.Lerp(positionNormalized, 0.5f, Time.deltaTime * returnSpeed);

                    if (Mathf.Abs(positionNormalized - 0.5f) < snapThreshold)
                        positionNormalized = 0.5f;
                }
                else
                {
                    // Snap vitesse
                    if (Mathf.Abs(positionNormalized - 0f) < snapThreshold) positionNormalized = 0f;
                    else if (Mathf.Abs(positionNormalized - 0.5f) < snapThreshold) positionNormalized = 0.5f;
                    else if (Mathf.Abs(positionNormalized - 1f) < snapThreshold) positionNormalized = 1f;
                }
            }

            // Appliquer la position sur le rail
            transform.position = Vector3.Lerp(railStart.position, railEnd.position, positionNormalized);
        }

        void HandleHover()
        {
            Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
            bool hitThisFrame = false;

            if (Physics.Raycast(ray, out RaycastHit hit, maxGrabDistance))
            {
                if (hit.collider.gameObject == gameObject)
                {
                    hitThisFrame = true;
                    lastValidHoverTime = Time.time;
                }
            }

            bool shouldBeHovered = hitThisFrame || (Time.time - lastValidHoverTime < hoverMemoryDuration);

            if (!shouldBeHovered)
            {
                Vector3 screenPoint = playerCamera.WorldToScreenPoint(transform.position);
                float distToMouse = Vector2.Distance(Input.mousePosition, screenPoint);
                if (distToMouse < 40f)
                {
                    shouldBeHovered = true;
                    lastValidHoverTime = Time.time;
                }
            }

            if (shouldBeHovered != isHovered)
            {
                isHovered = shouldBeHovered;
                SetHighlight(isHovered);
            }
        }

        void SetHighlight(bool active)
        {
            if (rend == null) return;
            rend.material.color = active ? highlightColor : baseColor;
        }

        // 🔒 Verrouillage du curseur pendant le grab
        void StartGrab()
        {
            isGrabbed = true;

            // On garde l’état précédent du curseur pour le restaurer ensuite
            cursorWasLockedBefore = Cursor.lockState == CursorLockMode.Locked;
            grabScreenPos = Input.mousePosition;

            // Désactive la rotation de la caméra et "verrouille" la souris à la position actuelle
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        void StopGrab()
        {
            isGrabbed = false;

            // Restaure l’état du curseur précédent
            Cursor.lockState = cursorWasLockedBefore ? CursorLockMode.Locked : CursorLockMode.None;
            Cursor.visible = true;
        }
    }
