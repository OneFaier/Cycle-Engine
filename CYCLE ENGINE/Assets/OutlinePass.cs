using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

class OutlinePass : CustomPass
{
    public Color outlineColor = Color.yellow;
    public LayerMask outlineLayer;

    Material outlineMaterial;

    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
        outlineMaterial = new Material(Shader.Find("Hidden/UnlitColor")); // simple unlit color
    }

    protected override void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera camera, CullingResults cullingResult)
    {
        if (outlineMaterial == null) return;

        outlineMaterial.SetColor("_Color", outlineColor);

        var renderers = GameObject.FindObjectsOfType<Renderer>();
        foreach (var r in renderers)
        {
            if (((1 << r.gameObject.layer) & outlineLayer) != 0)
            {
                cmd.DrawRenderer(r, outlineMaterial);
            }
        }
    }
}