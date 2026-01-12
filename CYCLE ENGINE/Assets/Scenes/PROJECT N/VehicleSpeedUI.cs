using UnityEngine;
using TMPro;

public class VehicleSpeedTMP : MonoBehaviour
{
    [Header("References")]
    public Rigidbody vehicleRigidbody;
    public TMP_Text speedText;

    [Header("Settings")]
    public bool showDecimal = false;

    void Update()
    {
        if (!vehicleRigidbody || !speedText) return;

#if UNITY_6000_0_OR_NEWER
        float speedMS = vehicleRigidbody.linearVelocity.magnitude;
#else
        float speedMS = vehicleRigidbody.velocity.magnitude;
#endif

        float speedKMH = speedMS * 3.6f;

        speedText.text = showDecimal
            ? $"{speedKMH:F1} km/h"
            : $"{Mathf.RoundToInt(speedKMH)} km/h";
    }
}