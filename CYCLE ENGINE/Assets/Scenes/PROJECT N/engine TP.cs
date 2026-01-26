using UnityEngine;

public class EngineTeleporter : MonoBehaviour
{
    [Header("Références")]
    public SpaceshipAdvanced engine;      // Le vaisseau à téléporter
    public SimpleFPSController player;   // Le joueur
    public IndicatorGearMouse gearLever; // Le levier du vaisseau

    [Header("Paramètres")]
    public float sideDistance = 3f;      // Distance à droite du joueur

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            TeleportEngine();
        }
    }

    void TeleportEngine()
    {
        if (engine == null || player == null) return;

        // --- Positionner à droite du joueur ---
        Vector3 rightPos = player.transform.position + player.transform.right * sideDistance;
        engine.transform.position = rightPos;

        // --- Réinitialiser rotation et roll ---
        engine.transform.rotation = Quaternion.identity;
        engine.currentRoll = 0f;
        engine.yawInputSmooth = 0f;

        // --- Réinitialiser le levier ---
        if (gearLever != null)
        {
            gearLever.currentGear = 0;

            if (gearLever.gearCrans.Length > 0)
                gearLever.transform.position = gearLever.gearCrans[0].position;
        }
    }
}