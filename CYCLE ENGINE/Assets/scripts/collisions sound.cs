using UnityEngine;

[RequireComponent(typeof(Collider))]
public class SpaceshipCollisionSound : MonoBehaviour
{
    [Header("Collision Sounds")]
    public AudioClip[] collisionClips;

    [Tooltip("Force minimale pour déclencher un son")]
    public float collisionMinImpact = 2f;

    [Tooltip("Volume maximal du son")]
    public float collisionVolume = 1f;

    [Tooltip("Pitch global du son (moins que 1 = plus grave)")]
    public float globalPitch = 0.85f;

    [Tooltip("Distance minimale et maximale pour la spatialisation 3D")]
    public float minDistance = 1f;
    public float maxDistance = 50f;

    void OnCollisionEnter(Collision collision)
    {
        if (collisionClips == null || collisionClips.Length == 0)
            return;

        float impactForce = collision.relativeVelocity.magnitude;
        if (impactForce < collisionMinImpact)
            return;

        // Choisir un clip aléatoire
        AudioClip clip = collisionClips[Random.Range(0, collisionClips.Length)];

        // Créer un GameObject temporaire à l'endroit de l'impact
        GameObject tempAudio = new GameObject("CollisionSound");
        tempAudio.transform.position = collision.contacts[0].point;

        AudioSource aSource = tempAudio.AddComponent<AudioSource>();
        aSource.clip = clip;

        // Volume proportionnel à l'impact
        aSource.volume = Mathf.Clamp(impactForce / 10f, 0f, collisionVolume);

        // Pitch global légèrement plus grave
        aSource.pitch = globalPitch;

        aSource.spatialBlend = 1f; // son 3D
        aSource.minDistance = minDistance;
        aSource.maxDistance = maxDistance;
        aSource.Play();

        // Détruire le GameObject après le son
        Destroy(tempAudio, clip.length / aSource.pitch);
    }
}