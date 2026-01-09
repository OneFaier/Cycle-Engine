using UnityEngine;
using System.Collections;

public class GasBottleSpawner : MonoBehaviour
{
    public GameObject bottlePrefab;
    public float respawnDelay = 15f;

    void Start()
    {
        Spawn();
    }

    void Spawn()
    {
        Instantiate(bottlePrefab, transform.position, transform.rotation);
    }
}