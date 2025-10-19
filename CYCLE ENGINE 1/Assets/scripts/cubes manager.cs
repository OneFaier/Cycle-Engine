using UnityEngine;
using UnityEngine.UI;

public class EngineCubeManager : MonoBehaviour
{
    [Header("Références")]
    public CubeHealth speedCubeHealth; // Cube qui gère la vitesse
    public CubeHealth directionCubeHealth; // Cube qui gère la direction
    public Slider externalSpeedSlider; // Slider externe à contrôler

    [Header("Snap check")]
    public Transform snapParent; // Le réceptacle où les cubes doivent être snapés

    void Update()
    {
        float speedValue = 0f;

        // Vérifie si un cube est snapé (enfant du réceptacle)
        if (speedCubeHealth != null && speedCubeHealth.transform.parent == snapParent)
        {
            // Le slider dépend de la vie du cube
            speedValue = speedCubeHealth.GetHealthRatio() * speedCubeHealth.maxHealth;
        }

        // Si aucun cube snapé, slider = 0
        if (speedCubeHealth == null || speedCubeHealth.transform.parent != snapParent)
        {
            speedValue = 0f;
        }

        if (externalSpeedSlider != null)
            externalSpeedSlider.value = speedValue;
    }
}
