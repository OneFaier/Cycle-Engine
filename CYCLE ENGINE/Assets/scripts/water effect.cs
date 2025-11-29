using UnityEngine;

public class WaterDrainEffect : MonoBehaviour
{
    public float speed = 2f;  // vitesse

    private bool startDrain = false;
    private bool startRise = false;

    private RectTransform rect;
    private Vector2 topPos;    // eau haute
    private Vector2 bottomPos; // eau basse

    void Start()
    {
        rect = GetComponent<RectTransform>();

        topPos = rect.anchoredPosition;             // eau haute
        bottomPos = new Vector2(topPos.x, -Screen.height); // eau basse

        gameObject.SetActive(false); // désactivée par défaut
    }

    void Update()
    {
        if (startDrain)
        {
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, bottomPos, Time.deltaTime * speed);

            if (Vector2.Distance(rect.anchoredPosition, bottomPos) < 1f)
            {
                startDrain = false;
                rect.anchoredPosition = bottomPos;
                gameObject.SetActive(false);
            }
        }

        if (startRise)
        {
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, topPos, Time.deltaTime * speed);

            if (Vector2.Distance(rect.anchoredPosition, topPos) < 1f)
            {
                startRise = false;
                rect.anchoredPosition = topPos;
                gameObject.SetActive(false);
            }
        }
    }

    // Eau descend → vide le SAS
    public void StartWaterDrain()
    {
        gameObject.SetActive(true);
        rect.anchoredPosition = topPos; // assure départ en haut
        startDrain = true;
        startRise = false;
    }

    // Eau remonte → remplie le SAS
    public void StartWaterRise()
    {
        gameObject.SetActive(true);
        rect.anchoredPosition = bottomPos; // assure départ en bas
        startRise = true;
        startDrain = false;
    }
}