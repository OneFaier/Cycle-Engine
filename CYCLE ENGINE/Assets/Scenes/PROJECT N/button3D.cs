using UnityEngine;
using TMPro;
using UnityEngine.Events;

[RequireComponent(typeof(Renderer))]
public class Button3D : MonoBehaviour
{
    [Header("UI")]
    public TextMeshProUGUI buttonText;       // optionnel, texte au-dessus du bouton
    public string hoverMessage = "Cliquez";

    [Header("Visuals")]
    public Color baseColor = Color.white;
    public Color highlightColor = Color.cyan;
    public float maxClickDistance = 3f;

    [Header("Action")]
    public UnityEvent onClick;               // action à déclencher au clic

    private Renderer rend;
    private Camera cam;
    private bool isHovered = false;

    void Start()
    {
        cam = Camera.main;
        rend = GetComponent<Renderer>();
        if (rend) rend.material.color = baseColor;

        if (buttonText)
            buttonText.text = hoverMessage;
    }

    void Update()
    {
        HandleHover();

        // clic gauche
        if (isHovered && Input.GetMouseButtonDown(0))
        {
            onClick.Invoke();
        }
    }

    void HandleHover()
    {
        isHovered = false;

        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, maxClickDistance))
        {
            if (hit.collider.gameObject == gameObject)
                isHovered = true;
        }

        // couleur du bouton
        if (rend)
            rend.material.color = isHovered ? highlightColor : baseColor;

        // texte sur le bouton
        if (buttonText)
            buttonText.gameObject.SetActive(isHovered);
    }
}
