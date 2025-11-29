using UnityEngine;

public class SpeedParticles : MonoBehaviour
{
    [Header("Références")]
    public HoverSpaceshipAdvanced spaceship; // ton script de vaisseau
    public ParticleSystem speedParticles;    // le système de particules à activer

    [Header("Seuil")]
    [Range(0f, 1f)]
    public float speedThreshold = 0.66f;     // 2/3 de la vitesse max

    private void Update()
    {
        if (spaceship == null || speedParticles == null)
            return;

        // Calculer la vitesse avant du vaisseau
        float forwardSpeed = Vector3.Dot(spaceship.GetComponent<Rigidbody>().linearVelocity, spaceship.transform.forward);

        // Prendre la valeur absolue
        forwardSpeed = Mathf.Abs(forwardSpeed);

        // Activer ou désactiver les particules selon la vitesse
        if (forwardSpeed >= spaceship.maxForwardSpeed * speedThreshold)
        {
            if (!speedParticles.isPlaying)
                speedParticles.Play();
        }
        else
        {
            if (speedParticles.isPlaying)
                speedParticles.Stop();
        }
    }
}