using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class RadarToggleUI : MonoBehaviour
{
    [Header("Image radar (celle qui s'affiche / se cache)")]
    public RawImage radarImage;

    [Header("Image en dessous")]
    public RawImage backgroundImage;

    [Header("Timing")]
    public float visibleDuration = 0.3f;
    public float hiddenDuration = 3f;

    [Header("Couleur quand le radar est caché")]
    public Color hiddenColor = Color.red;

    private Color originalBackgroundColor;

    private void Start()
    {
        if (!radarImage || !backgroundImage)
        {
            Debug.LogError("RadarToggleUI : RawImage manquante !");
            return;
        }

        originalBackgroundColor = backgroundImage.color;

        StartCoroutine(ToggleRoutine());
    }

    private IEnumerator ToggleRoutine()
    {
        while (true)
        {
            // Radar visible → couleur normale
            radarImage.gameObject.SetActive(true);
            backgroundImage.color = originalBackgroundColor;

            yield return new WaitForSeconds(visibleDuration);

            // Radar caché → couleur custom
            radarImage.gameObject.SetActive(false);
            backgroundImage.color = hiddenColor;

            yield return new WaitForSeconds(hiddenDuration);
        }
    }
}