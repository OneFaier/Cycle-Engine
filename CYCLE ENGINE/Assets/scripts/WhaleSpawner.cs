using UnityEngine;
using System.Collections.Generic;

public class WhaleSpawner : MonoBehaviour
{
    [System.Serializable]
    public class SeaCreature
    {
        public string name;
        public GameObject prefab;
        public float swimSpeed = 2f;
        public float turnSpeed = 1f;
    }

    [Header("Spawn Settings")]
    public List<SeaCreature> creatures;
    public int numberToSpawn = 10;
    public Vector3 spawnAreaSize = new Vector3(200f, 80f, 200f);

    [Header("Terrain / Abyss")]
    public Terrain terrain;     // Le terrain du fond (obligatoire)
    public float safeHeightAboveTerrain = 20f; // Distance minimale au-dessus du sol

    private List<SeaCreatureAI> activeCreatures = new List<SeaCreatureAI>();

    void Start()
    {
        if (terrain == null)
        {
            Debug.LogError("Aucun terrain assigné au WhaleSpawner !");
            return;
        }

        for (int i = 0; i < numberToSpawn; i++)
        {
            SpawnCreature();
        }
    }

    void SpawnCreature()
    {
        if (creatures.Count == 0) return;

        // Choisir aléatoirement une créature
        SeaCreature creature = creatures[Random.Range(0, creatures.Count)];

        Vector3 randomPos = transform.position + new Vector3(
            Random.Range(-spawnAreaSize.x, spawnAreaSize.x),
            Random.Range(10f, spawnAreaSize.y),  // Hauteur TJS au-dessus du sol
            Random.Range(-spawnAreaSize.z, spawnAreaSize.z)
        );

        // Ajuster la hauteur par rapport au terrain
        float terrainHeight = terrain.SampleHeight(randomPos);
        randomPos.y = Mathf.Max(randomPos.y, terrainHeight + safeHeightAboveTerrain);

        GameObject newCreature = Instantiate(creature.prefab, randomPos, Quaternion.identity);
        SeaCreatureAI ai = newCreature.AddComponent<SeaCreatureAI>();
        ai.Init(creature, terrain, safeHeightAboveTerrain);

        activeCreatures.Add(ai);
    }
}