using UnityEngine;

public class LightEmitter : MonoBehaviour
{
    public GameObject lightPrefab;
    public float maxPulseSize = 5f;

    [Header("Footstep Pulse")]
    public float footstepLifeTime = 0.45f;
    public float footstepExpandSpeed = 10f;
    public float footstepStartScalePercent = 0.45f;
    public Color footstepColor = new Color(1f, 0.96f, 0.75f, 0.22f);

    [Header("Soft Landing Pulse")]
    public float softLandingLifeTime = 0.6f;
    public float softLandingExpandSpeed = 8f;
    public float softLandingStartScalePercent = 0.28f;
    public Color softLandingColor = new Color(1f, 0.95f, 0.78f, 0.35f);

    [Header("Hard Landing Pulse")]
    public float hardLandingLifeTime = 1.35f;
    public float hardLandingExpandSpeed = 6f;
    public float hardLandingStartScalePercent = 0.18f;
    public Color hardLandingColor = new Color(0.72f, 0.78f, 0.98f, 0.38f);

    public void EmitLight(Vector2 position, float impact)
    {
        EmitLight(position, impact, impact);
    }

    public void EmitFootstepLight(Vector2 position, float visualSize)
    {
        SpawnPulse(position, visualSize, footstepLifeTime, footstepExpandSpeed, footstepStartScalePercent, footstepColor);
    }

    public void EmitSoftLandingLight(Vector2 position, float visualSize)
    {
        SpawnPulse(position, visualSize, softLandingLifeTime, softLandingExpandSpeed, softLandingStartScalePercent, softLandingColor);
    }

    public void EmitHardLandingLight(Vector2 position, float visualSize)
    {
        SpawnPulse(position, visualSize, hardLandingLifeTime, hardLandingExpandSpeed, hardLandingStartScalePercent, hardLandingColor);
    }

    public void EmitLight(Vector2 position, float visualSize, float hearingRange)
    {
        float maxAllowedSize = maxPulseSize;

        if (Camera.main != null && Camera.main.orthographic)
        {
            maxAllowedSize = Camera.main.orthographicSize * 2f;
        }

        float size = Mathf.Clamp(visualSize, 0.2f, maxAllowedSize);

        if (lightPrefab != null)
        {
            SpawnPulse(position, size, 0.65f, 8f, 0.2f, new Color(1f, 0.97f, 0.8f, 0.32f));
        }
    }

    void SpawnPulse(Vector2 position, float visualSize, float lifeTime, float expandSpeed, float startScalePercent, Color pulseColor)
    {
        if (lightPrefab == null)
        {
            return;
        }

        float maxAllowedSize = maxPulseSize;

        if (Camera.main != null && Camera.main.orthographic)
        {
            maxAllowedSize = Camera.main.orthographicSize * 2f;
        }

        float size = Mathf.Clamp(visualSize, 0.2f, maxAllowedSize);

        GameObject light = Instantiate(lightPrefab, position, Quaternion.identity);
        light.transform.localScale = Vector3.one * size;

        LightPulse pulse = light.GetComponentInChildren<LightPulse>();

        if (pulse != null)
        {
            pulse.SetPulseSettings(lifeTime, expandSpeed, startScalePercent, pulseColor);
        }
    }
}
