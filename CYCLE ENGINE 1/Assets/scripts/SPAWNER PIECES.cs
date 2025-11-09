using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [Header("Prefabs de pickups")]
    public GameObject speedPickupPrefab;
    public GameObject directionPickupPrefab;
    public GameObject elevationPickupPrefab; // 🆕 Nouveau prefab d’élévation

    [Header("Références de scène")]
    public Transform player;

    [Header("Cubes de la scène")]
    public IndicatorMouseClickFast directionCubeInScene; // cube direction
    public IndicatorMouseClickFast speedCubeInScene;     // cube vitesse
    public IndicatorMouseClickFast elevationCubeInScene; // 🆕 cube élévation

    [Header("Paramètres de spawn")]
    public float spawnRadius = 5f;
    public float spawnHeight = 1f;
    public float spawnInterval = 3f;
    public int maxPickups = 10;

    private float timer = 0f;
    private int currentPickups = 0;

    void Update()
    {
        if (player == null) return;

        timer += Time.deltaTime;

        if (timer >= spawnInterval && currentPickups < maxPickups)
        {
            SpawnRandomPickup();
            timer = 0f;
        }
    }

    void SpawnRandomPickup()
    {
        // 🌀 Choix aléatoire entre les 3 types
        float rand = Random.value;
        GameObject prefab;

        if (rand < 0.33f)
            prefab = speedPickupPrefab;
        else if (rand < 0.66f)
            prefab = directionPickupPrefab;
        else
            prefab = elevationPickupPrefab; // 🆕

        // Position de spawn autour du joueur
        Vector2 offset = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = new Vector3(
            player.position.x + offset.x,
            player.position.y + spawnHeight,
            player.position.z + offset.y
        );

        GameObject newPickup = Instantiate(prefab, spawnPos, Quaternion.identity);

        // 🧠 Liaison automatique du bon cube selon le prefab
        if (prefab == speedPickupPrefab)
        {
            CubeHealth speedHealth = newPickup.GetComponent<CubeHealth>();
            if (speedHealth != null && speedCubeInScene != null)
                speedHealth.cube = speedCubeInScene;
        }
        else if (prefab == directionPickupPrefab)
        {
            DirectionCubeHealth dirHealth = newPickup.GetComponent<DirectionCubeHealth>();
            if (dirHealth != null && directionCubeInScene != null)
                dirHealth.directionCube = directionCubeInScene;
        }
        currentPickups++;
    }
}
