using UnityEngine;

public class WaterDrainEffect : MonoBehaviour
{
    public float speed = 2f;

    private RectTransform rect;
    private Vector2 topPos;
    private Vector2 bottomPos;

    private bool movingDown;
    private bool movingUp;

    void Start()
    {
        rect = GetComponent<RectTransform>();

        topPos = rect.anchoredPosition;
        bottomPos = new Vector2(topPos.x, -Screen.height);

        gameObject.SetActive(false);
    }

    void Update()
    {
        if (movingDown)
        {
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, bottomPos, Time.deltaTime * speed);

            if (Vector2.Distance(rect.anchoredPosition, bottomPos) < 1f)
            {
                movingDown = false;
                rect.anchoredPosition = bottomPos;
                gameObject.SetActive(false);
            }
        }

        if (movingUp)
        {
            rect.anchoredPosition = Vector2.Lerp(rect.anchoredPosition, topPos, Time.deltaTime * speed);

            if (Vector2.Distance(rect.anchoredPosition, topPos) < 1f)
            {
                movingUp = false;
                rect.anchoredPosition = topPos;
                gameObject.SetActive(false);
            }
        }
    }

    public void StartWaterDrain()
    {
        gameObject.SetActive(true);
        rect.anchoredPosition = topPos;

        movingDown = true;
        movingUp = false;
    }

    public void StartWaterRise()
    {
        gameObject.SetActive(true);
        rect.anchoredPosition = bottomPos;

        movingUp = true;
        movingDown = false;
    }
}
