using UnityEngine;

public class SimpleModuleSpawner : MonoBehaviour
{
    [Header("Prefab du module à spawn")]
    public GameObject modulePrefab;

    [Header("Point de spawn (Empty)")]
    public Transform spawnPoint;

    [Header("Cube dans la scène")]
    public IndicatorMouseClickFast sceneCube; // vitesse ou direction

    [Header("Paramètres")]
    public float spawnInterval = 5f;

    private float timer = 0f;

    private void Update()
    {
        if (modulePrefab == null || spawnPoint == null) return;

        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnModule();
            timer = 0f;
        }
    }

    private void SpawnModule()
    {
        GameObject newModule = Instantiate(modulePrefab, spawnPoint.position, spawnPoint.rotation);
        newModule.transform.SetParent(spawnPoint);
        
        // Assignation automatique du cube correspondant
        CubeHealth cubeHealth = newModule.GetComponent<CubeHealth>();
        if (cubeHealth != null && sceneCube != null)
        {
            cubeHealth.cube = sceneCube;
        }

        // Si c'est un module direction
        DirectionCubeHealth dirHealth = newModule.GetComponent<DirectionCubeHealth>();
        if (dirHealth != null && sceneCube != null)
        {
            dirHealth.directionCube = sceneCube;
        }

        newModule.tag = modulePrefab.tag; // garde le tag défini dans le prefab
    }
}