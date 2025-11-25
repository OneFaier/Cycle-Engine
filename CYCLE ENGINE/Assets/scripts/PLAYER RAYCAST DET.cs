// using UnityEngine;
//
// public class PlayerInteraction : MonoBehaviour
// {
//     public Camera cam;
//     public float maxDistance = 3f;
//     public KeyCode interactKey = KeyCode.E;
//
//     void Update()
//     {
//         if (Input.GetKeyDown(interactKey))
//             TryInteract();
//     }
//
//     void TryInteract()
//     {
//         if (cam == null) cam = Camera.main;
//
//         Ray ray = new Ray(cam.transform.position, cam.transform.forward);
//
//         if (Physics.Raycast(ray, out RaycastHit hit, maxDistance))
//         {
//             DoorButton button = hit.collider.GetComponent<DoorButton>();
//
//             if (button != null)
//                 button.TriggerDoor();
//         }
//     }
// }