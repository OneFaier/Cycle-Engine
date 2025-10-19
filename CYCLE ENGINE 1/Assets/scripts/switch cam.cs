using UnityEngine;

public class CameraSwitcher : MonoBehaviour
{
    [Header("Cameras à alterner")]
    public Camera camera1;
    public Camera camera2;

    private bool usingFirstCamera = true;

    void Start()
    {
        if (camera1 != null && camera2 != null)
        {
            camera1.enabled = true;
            camera2.enabled = false;
        }

        LockCursor(); // on verrouille dès le départ
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            usingFirstCamera = !usingFirstCamera;

            if (camera1 != null && camera2 != null)
            {
                camera1.enabled = usingFirstCamera;
                camera2.enabled = !usingFirstCamera;
            }

            LockCursor(); // on relock après le switch
        }
    }

    private void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
