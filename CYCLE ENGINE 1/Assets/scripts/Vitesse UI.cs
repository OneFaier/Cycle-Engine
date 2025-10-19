using UnityEngine;
using TMPro;

public class SpeedUI : MonoBehaviour
{
    [Header("Références")]
    public ENGINEMovements engine;       // Le script du véhicule
    public TextMeshProUGUI speedText;    // Le texte UI TMP

    void Update()
    {
        if (engine == null || speedText == null)
            return;

        // Affiche la vitesse avec 1 décimale
        speedText.text = "Vitesse : " + engineSpeed().ToString("F1") + " m/s";
    }

    float engineSpeed()
    {
        // On retourne la vitesse actuelle du moteur
        // (elle est privée, donc on la récupère via une méthode)
        return GetCurrentSpeed(engine);
    }

    float GetCurrentSpeed(ENGINEMovements e)
    {
        // Accède à la vitesse via réflexion (si tu veux éviter ça, lis plus bas 👇)
        var field = typeof(ENGINEMovements).GetField("currentSpeed", 
                     System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        return (float)field.GetValue(e);
    }
}
