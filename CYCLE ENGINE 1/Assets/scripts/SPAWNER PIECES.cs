using UnityEngine;

public class PickupSpawner : MonoBehaviour
{
    [Header("Prefabs de pickups")]
    public GameObject speedPickupPrefab;
    public GameObject directionPickupPrefab;
    public Transform player;

    [Header("Cubes de la scène")]
    public IndicatorMouseClickFast directionCubeInScene; // cube direction
    public IndicatorMouseClickFast speedCubeInScene;     // cube vitesse

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
        GameObject prefab = (Random.value < 0.5f) ? speedPickupPrefab : directionPickupPrefab;

        Vector2 rand = Random.insideUnitCircle * spawnRadius;
        Vector3 spawnPos = new Vector3(player.position.x + rand.x, player.position.y + spawnHeight, player.position.z + rand.y);

        GameObject newPickup = Instantiate(prefab, spawnPos, Quaternion.identity);

        if (prefab == directionPickupPrefab)
        {
            DirectionCubeHealth dirHealth = newPickup.GetComponent<DirectionCubeHealth>();
            if (dirHealth != null && directionCubeInScene != null)
                dirHealth.directionCube = directionCubeInScene;
        }
        else if (prefab == speedPickupPrefab)
        {
            CubeHealth speedHealth = newPickup.GetComponent<CubeHealth>();
            if (speedHealth != null && speedCubeInScene != null)
                speedHealth.cube = speedCubeInScene;
        }

        currentPickups++;
    }
}
