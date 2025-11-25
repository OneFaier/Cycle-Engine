// using UnityEngine;

// public class GrabAndSimpleSnap : MonoBehaviour
// {
//     public Camera playerCamera;
//     public float grabRange = 3f;
//     public float holdDistance = 2f;
//     public Transform snapTarget; // empty où l'objet sera placé quand lâché

//     private Rigidbody heldObject = null;

//     void Update()
//     {
//         Attraper objet
//         if (Input.GetMouseButtonDown(0) && heldObject == null)
//         {
//             Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
//             if (Physics.Raycast(ray, out RaycastHit hit, grabRange))
//             {
//                 Rigidbody rb = hit.collider.GetComponent<Rigidbody>();
//                 if (rb != null && hit.collider.CompareTag("Grabbable"))
//                 {
//                     heldObject = rb;
//                     heldObject.useGravity = false;

//                     Détache si déjà snapé
//                     if (heldObject.transform.parent != null)
//                         heldObject.transform.SetParent(null);
//                 }
//             }
//         }

//         Lâcher objet
//         if (Input.GetMouseButtonDown(1) && heldObject != null)
//         {
//             if (snapTarget != null)
//             {
//                 Téléporte l'objet directement à la target
//                 heldObject.transform.position = snapTarget.position;
//                 heldObject.transform.rotation = snapTarget.rotation;

//                 Devenir enfant
//                 heldObject.transform.SetParent(snapTarget);
//             }

//             heldObject.useGravity = true;
//             heldObject = null;
//         }

//         Maintenir l’objet devant soi
//         if (heldObject != null)
//         {
//             Vector3 targetPos = playerCamera.transform.position + playerCamera.transform.forward * holdDistance;
//             Vector3 moveDir = targetPos - heldObject.position;
//             heldObject.linearVelocity = moveDir / Time.deltaTime;
//         }
//     }
// }
