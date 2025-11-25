using UnityEngine;

public class CubeBlinker : MonoBehaviour
{
    [Header("Couleurs")]
    public Color colorA = Color.white;
    public Color colorB = Color.red;

    [Header("Matériel du rebord (avec outline/emission)")]
    public Renderer rend;
    public string colorProperty = "_EmissionColor";

    [Header("Clignotement")]
    public float baseBlinkSpeed = 1f;
    public float maxBlinkSpeed = 12f;
    public float intensity = 0f;

    private Material mat;
    private float timer;

    private void Start()
    {
        if (rend == null) rend = GetComponent<Renderer>();
        mat = rend.material;
    }

    private void Update()
    {
        float blinkSpeed = Mathf.Lerp(baseBlinkSpeed, maxBlinkSpeed, intensity);

        timer += Time.deltaTime * blinkSpeed;

        float t = (Mathf.Sin(timer) + 1f) / 2f;

        Color c = Color.Lerp(colorA, colorB, t);

        mat.SetColor(colorProperty, c * (1f + intensity * 2f)); // intensité glow
    }

    public void SetIntensity(float value)
    {
        intensity = Mathf.Clamp01(value);
    }
}