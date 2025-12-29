using UnityEngine;

public class DoorButton3D_Click : MonoBehaviour
{
    [Header("Porte à contrôler")]
    public SlidingDoorWithAudio door;

    [Header("Animation bouton")]
    public Transform buttonPivot;
    public Vector3 pressedOffset = new Vector3(0, -0.05f, 0);
    public float pressSpeed = 5f;

    private Vector3 initialButtonPos;
    private bool isPressed = false;
    private Camera playerCamera;

    void Start()
    {
        initialButtonPos = buttonPivot.localPosition;
        playerCamera = Camera.main;
    }

    void Update()
    {
        if (playerCamera == null) return;

        HandleClick();
        AnimateButton();
    }

    void HandleClick()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.gameObject == gameObject && door != null)
            {
                isPressed = true;
                door.ToggleDoor();
            }
        }
    }

    void AnimateButton()
    {
        Vector3 targetPos = isPressed
            ? initialButtonPos + pressedOffset
            : initialButtonPos;

        buttonPivot.localPosition = Vector3.Lerp(
            buttonPivot.localPosition,
            targetPos,
            Time.deltaTime * pressSpeed
        );

        if (isPressed && Vector3.Distance(buttonPivot.localPosition, targetPos) < 0.001f)
            isPressed = false;
    }
}