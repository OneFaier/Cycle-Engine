using UnityEngine;

public class ChangeColorOnClick : MonoBehaviour
{
    [SerializeField] private Color baseColor = Color.gray;   // couleur de base
    [SerializeField] private Color clickedColor = Color.red; // couleur quand on clique

    private Renderer cubeRenderer;
    private bool isColored = false;

    void Start()
    {
        cubeRenderer = GetComponent<Renderer>();
        cubeRenderer.material.color = baseColor;
    }

    void OnMouseDown()
    {
        isColored = !isColored;
        cubeRenderer.material.color = isColored ? clickedColor : baseColor;
    }
}
