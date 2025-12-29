using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RadarPulseUI : MonoBehaviour
{
    [Header("RawImage à pulser")]
    public RawImage radarImage;

    [Header("Paramètres du pulse")]
    public float pulseDuration = 0.3f; // durée pendant laquelle le pulse est visible
    public float pulseDelay = 3f;      // temps entre chaque pulse
    public float maxScale = 1.5f;      // taille maximale du pulse

    private Vector3 originalScale;

    private void Start()
    {
        if (!radarImage)
        {
            Debug.LogError("RadarPulseUI : pas de RawImage assignée !");
            return;
        }

        originalScale = radarImage.transform.localScale;
        radarImage.gameObject.SetActive(false); // caché au départ
        StartCoroutine(PulseRoutine());
    }

    private IEnumerator PulseRoutine()
    {
        while (true)
        {
            // Attend le délai avant le prochain pulse
            yield return new WaitForSeconds(pulseDelay);

            // Affiche le pulse
            radarImage.gameObject.SetActive(true);

            float t = 0f;
            while (t < pulseDuration)
            {
                t += Time.deltaTime;
                float progress = t / pulseDuration;

                // Scale du pulse
                float scale = Mathf.Lerp(0f, maxScale, progress);
                radarImage.transform.localScale = originalScale * scale;

                // Alpha fade out
                Color c = radarImage.color;
                c.a = 1f - progress;
                radarImage.color = c;

                yield return null;
            }

            // Reset et cache le pulse
            radarImage.transform.localScale = originalScale;
            Color resetColor = radarImage.color;
            resetColor.a = 1f;
            radarImage.color = resetColor;
            radarImage.gameObject.SetActive(false);
        }
    }
}
