using UnityEngine;
using System.Collections;

public class DirectionModuleSpawner : MonoBehaviour
{
    [Header("Prefab Module Direction")]
    public GameObject directionPrefab;

    [Header("Spawn Point")]
    public Transform spawnPoint;

    [Header("Cube Direction dans la scène")]
    public IndicatorMouseClickFast directionCubeInScene;

    [Header("Spawn Settings")]
    public float spawnInterval = 120f; // 2 minutes

    private bool spawnScheduled = false;

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Grabbable")) // Si un module est déjà présent
        {
            spawnScheduled = false; // annule spawn
        }
        else
        {
            if (!spawnScheduled && !IsModulePresent())
                StartCoroutine(SpawnAfterDelay());
        }
    }

    private bool IsModulePresent()
    {
        foreach (Transform child in transform)
        {
            if (child.CompareTag("Direction"))
                return true;
        }
        return false;
    }

    private IEnumerator SpawnAfterDelay()
    {
        spawnScheduled = true;
        yield return new WaitForSeconds(spawnInterval);

        if (!IsModulePresent() && directionPrefab != null && spawnPoint != null)
        {
            GameObject newModule = Instantiate(directionPrefab, spawnPoint.position, spawnPoint.rotation);
            newModule.transform.SetParent(spawnPoint);
            newModule.tag = "Direction";

            // assignation automatique du cube
            DirectionCubeHealth dirHealth = newModule.GetComponent<DirectionCubeHealth>();
            if (dirHealth != null && directionCubeInScene != null)
                dirHealth.directionCube = directionCubeInScene;
        }

        spawnScheduled = false;
    }
}