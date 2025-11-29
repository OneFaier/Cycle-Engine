using UnityEngine;

public class SeaCreatureAI : MonoBehaviour
{
    private float swimSpeed;
    private float turnSpeed;
    private Terrain terrain;
    private float safeHeight;

    private Vector3 targetDirection;

    public void Init(WhaleSpawner.SeaCreature creatureData, Terrain terrainRef, float safeHeightRef)
    {
        swimSpeed = creatureData.swimSpeed;
        turnSpeed = creatureData.turnSpeed;
        terrain = terrainRef;
        safeHeight = safeHeightRef;

        PickNewRandomDirection();
    }

    void Update()
    {
        AvoidTerrain();
        MoveForward();
        RandomDirectionChange();
    }

    void MoveForward()
    {
        transform.position += transform.forward * swimSpeed * Time.deltaTime;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDirection), turnSpeed * Time.deltaTime);
    }

    void RandomDirectionChange()
    {
        if (Random.value < 0.01f) // 1% de chance par frame → environ toutes les 1-3 sec
            PickNewRandomDirection();
    }

    void PickNewRandomDirection()
    {
        targetDirection = new Vector3(
            Random.Range(-1f, 1f),
            Random.Range(-0.3f, 0.3f), // légère variation verticale
            Random.Range(-1f, 1f)
        );

        targetDirection.Normalize();
    }

    void AvoidTerrain()
    {
        float terrainHeight = terrain.SampleHeight(transform.position);
        float currentHeight = transform.position.y;

        if (currentHeight < terrainHeight + safeHeight)
        {
            // Remonte instantanément la direction vers le haut
            targetDirection = Vector3.Lerp(targetDirection, Vector3.up, 0.5f);
        }
    }
}
