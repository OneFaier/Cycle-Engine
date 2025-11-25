using UnityEngine;
using UnityEngine.UI;

public class SeatSnapWithIndicator : MonoBehaviour
{
    [Header("Références")]
    public Transform seatTarget;       // Empty assigné dans l’inspecteur
    public Transform healthIndicator;  // Cube voyant qui change de couleur

    [Header("Sliders externes")]
    public Slider externalSpeedSlider;
    public Slider externalDirectionSlider;

    private GameObject snappedObject;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Grabbable")) return;
        if (snappedObject != null) return;

        // Snap à la position du seatTarget
        other.transform.position = seatTarget.position;
        other.transform.rotation = seatTarget.rotation;
        other.transform.SetParent(seatTarget);

        // Supprime Rigidbody pour figer l'objet
        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb != null) Destroy(rb);

        snappedObject = other.gameObject;

        UpdateHealthIndicator();
    }

    private void Update()
    {
        if (snappedObject != null)
        {
            UpdateHealthIndicator();
        }
        else
        {
            SetIndicatorColor(Color.red);

            if (externalSpeedSlider != null)
                externalSpeedSlider.value = 0f;
            if (externalDirectionSlider != null)
                externalDirectionSlider.value = 0f;
        }
    }

    private void UpdateHealthIndicator()
    {
        if (healthIndicator == null || snappedObject == null) return;

        float ratio = 0f;

        // Cube classique
        CubeHealth cubeHealth = snappedObject.GetComponent<CubeHealth>();
        if (cubeHealth != null)
            ratio = cubeHealth.GetHealthRatio();

        // Cube directionnel
        DirectionCubeHealth dirHealth = snappedObject.GetComponent<DirectionCubeHealth>();
        if (dirHealth != null)
            ratio = dirHealth.GetHealthRatio();

        // Met à jour le voyant
        SetIndicatorColor(Color.Lerp(Color.red, Color.green, ratio));

        // Met à jour les sliders externes si assignés
        if (cubeHealth != null && externalSpeedSlider != null)
            externalSpeedSlider.value = ratio * externalSpeedSlider.maxValue;

        if (dirHealth != null && externalDirectionSlider != null)
            externalDirectionSlider.value = ratio * externalDirectionSlider.maxValue;
    }

    private void SetIndicatorColor(Color color)
    {
        if (healthIndicator == null) return;
        Renderer rend = healthIndicator.GetComponent<Renderer>();
        if (rend != null)
            rend.material.color = color;
    }

    public void DestroySnappedObject()
    {
        if (snappedObject == null) return;

        Destroy(snappedObject);
        snappedObject = null;

        SetIndicatorColor(Color.black);

        if (externalSpeedSlider != null)
            externalSpeedSlider.value = 0f;
        if (externalDirectionSlider != null)
            externalDirectionSlider.value = 0f;
    }
}
