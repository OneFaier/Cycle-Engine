using UnityEngine;

[ExecuteAlways]
public class TerrainHeightBlendOfficial : MonoBehaviour
{
    public float blendStartLowToMid = 0.25f;
    public float blendEndLowToMid = 0.35f;
    public float blendStartMidToHigh = 0.6f;
    public float blendEndMidToHigh = 0.7f;

    private Terrain terrain;
    private TerrainData terrainData;

    void OnEnable()
    {
        ApplyHeightBlend();
    }

    void OnValidate()
    {
        ApplyHeightBlend();
    }

    void ApplyHeightBlend()
    {
        terrain = GetComponent<Terrain>();
        if (terrain == null) return;

        terrainData = terrain.terrainData;
        if (terrainData == null || terrainData.alphamapLayers < 3) return;

        int w = terrainData.alphamapWidth;
        int h = terrainData.alphamapHeight;

        // créer la map [width, height, layers]
        float[,,] alphaMap = new float[w, h, terrainData.alphamapLayers];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float normX = (float)x / (w - 1);
                float normY = (float)y / (h - 1);

                // hauteur normalisée de 0 à 1
                float height = terrainData.GetInterpolatedHeight(normX, normY) / terrainData.size.y;

                // calcul des poids
                float lowWeight = 0f;
                float midWeight = 0f;
                float highWeight = 0f;

                // zone basse pure
                if (height <= blendStartLowToMid)
                {
                    lowWeight = 1.0f;
                }
                // transition basse → moyenne
                else if (height < blendEndLowToMid)
                {
                    float t = Mathf.InverseLerp(blendStartLowToMid, blendEndLowToMid, height);
                    lowWeight = 1.0f - t;
                    midWeight = t;
                }
                // zone moyenne pure
                else if (height <= blendStartMidToHigh)
                {
                    midWeight = 1.0f;
                }
                // transition moyenne → haute
                else if (height < blendEndMidToHigh)
                {
                    float t = Mathf.InverseLerp(blendStartMidToHigh, blendEndMidToHigh, height);
                    midWeight = 1.0f - t;
                    highWeight = t;
                }
                // zone haute pure
                else
                {
                    highWeight = 1.0f;
                }

                alphaMap[x, y, 0] = lowWeight;
                alphaMap[x, y, 1] = midWeight;
                alphaMap[x, y, 2] = highWeight;
            }
        }

        terrainData.SetAlphamaps(0, 0, alphaMap);
    }
}
