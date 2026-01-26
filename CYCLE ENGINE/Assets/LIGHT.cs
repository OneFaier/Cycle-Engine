using UnityEngine;

public class FlickerLights : MonoBehaviour
{
    [Header("Lights à grésiller")]
    public Light[] lights; // toutes les Point Lights que tu veux faire grésiller

    [Header("Paramètres du grésillement")]
    public float minIntensity = 0f;  // intensité minimale
    public float maxIntensity = 1f;  // intensité maximale
    public float flickerSpeed = 0.1f; // vitesse du grésillement (en secondes)

    void Start()
    {
        // Lance la boucle de grésillement pour chaque lumière
        foreach (Light light in lights)
        {
            if (light != null)
                StartCoroutine(FlickerLight(light));
        }
    }

    System.Collections.IEnumerator FlickerLight(Light light)
    {
        while (true)
        {
            // On choisit une nouvelle intensité aléatoire
            light.intensity = Random.Range(minIntensity, maxIntensity);

            // On attend un temps aléatoire pour rendre le grésillement plus naturel
            float waitTime = Random.Range(flickerSpeed / 2f, flickerSpeed * 1.5f);
            yield return new WaitForSeconds(waitTime);
        }
    }
}