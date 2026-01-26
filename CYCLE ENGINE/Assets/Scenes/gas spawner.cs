using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class GasBottleSpawner : MonoBehaviour
{
    [Header("Prefab & Spawn")]
    public GameObject bottlePrefab;
    public float spawnInterval = 5f;

    [Header("Tag des bouteilles")]
    public string bottleTag = "Grabbable";

    private float timer = 0f;

    // Toutes les bouteilles actuellement dans le trigger
    private List<Rigidbody> bottlesInTrigger = new List<Rigidbody>();

    private void Awake()
    {
        // S'assurer que le BoxCollider est trigger
        BoxCollider col = GetComponent<BoxCollider>();
        col.isTrigger = true;
    }

    private void Update()
    {
        timer += Time.deltaTime;

        // spawn uniquement si aucune bouteille dans le trigger
        if (bottlesInTrigger.Count == 0 && timer >= spawnInterval)
        {
            SpawnBottle();
            timer = 0f;
        }
    }

    private void SpawnBottle()
    {
        if (bottlePrefab == null) return;

        // rotation fixe : 90° en X
        Quaternion spawnRotation = Quaternion.Euler(90f, 0f, 0f);

        GameObject bottle = Instantiate(
            bottlePrefab,
            transform.position,
            spawnRotation
        );

        // Assigner tag
        bottle.tag = bottleTag;

        Rigidbody rb = bottle.GetComponent<Rigidbody>();
        if (rb)
        {
            rb.isKinematic = true; // reste bloquée dans le spawner
            rb.useGravity = true;  // gravity activée mais kinematic = bloquée
        }
    }

    // ===================== Trigger =====================
    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(bottleTag)) return;

        GasBottleResource bottle = other.GetComponent<GasBottleResource>();
        if (bottle != null && bottle.isGrabbed) return; // <-- IGNORED si grab

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb && !bottlesInTrigger.Contains(rb))
        {
            rb.isKinematic = true;
            bottlesInTrigger.Add(rb);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(bottleTag)) return;

        GasBottleResource bottle = other.GetComponent<GasBottleResource>();
        if (bottle != null && bottle.isGrabbed) return; // <-- IGNORED si grab

        Rigidbody rb = other.GetComponent<Rigidbody>();
        if (rb && bottlesInTrigger.Contains(rb))
        {
            rb.isKinematic = false; // redevient normal
            bottlesInTrigger.Remove(rb);
        }
    }

}
