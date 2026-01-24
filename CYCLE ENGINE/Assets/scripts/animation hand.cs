using UnityEngine;

public class HandAnimationToggle : MonoBehaviour
{
    [Header("Références")]
    public Animator handAnimator; // Animator de la main

    private bool isGrabbing = false;

    void Start()
    {
        if (handAnimator)
            handAnimator.SetBool("isGrabbing", false); // par défaut Idle
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // clic gauche
        {
            ToggleHandAnimation();
        }
    }

    void ToggleHandAnimation()
    {
        if (!handAnimator) return;

        isGrabbing = !isGrabbing; // inverse l'état
        handAnimator.SetBool("isGrabbing", isGrabbing);

        Debug.Log("Hand animation toggled: " + (isGrabbing ? "Grab" : "Idle"));
    }
}