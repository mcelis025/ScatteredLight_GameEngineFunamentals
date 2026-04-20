using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ScatteredLightCollectible : MonoBehaviour
{
    public float followHeight = 1.5f;
    public float bobAmount = 0.15f;
    public float bobSpeed = 3f;
    public float rayLength = 0.9f;
    public float rayWidth = 0.04f;
    public int rayCount = 6;
    public float idleLightIntensity = 0.55f;
    public float collectedLightIntensity = 1.2f;
    public float idleOuterRadius = 2.1f;
    public float collectedOuterRadius = 3.2f;
    public float lightPulseAmount = 0.15f;
    public float lightPulseSpeed = 4f;

    private Transform player;
    private bool isCollected;
    private LineRenderer[] rays;
    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;
    private Light2D light2D;

    void Start()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        light2D = GetComponentInChildren<Light2D>();
        startPosition = transform.position;
        EnsureLightExists();
        CreateRays();
    }

    void Update()
    {
        if (isCollected && player != null)
        {
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.position = player.position + new Vector3(0f, followHeight + bobOffset, 0f);
        }
        else
        {
            float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobAmount;
            transform.position = startPosition + new Vector3(0f, bobOffset, 0f);
        }

        UpdateRays();
        UpdateLight();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        player = other.transform;
        isCollected = true;

        Collider2D collectibleCollider = GetComponent<Collider2D>();

        if (collectibleCollider != null)
        {
            collectibleCollider.enabled = false;
        }
    }

    void CreateRays()
    {
        rays = new LineRenderer[rayCount];

        for (int i = 0; i < rayCount; i++)
        {
            GameObject rayObject = new GameObject("LightRay_" + i);
            rayObject.transform.SetParent(transform);
            rayObject.transform.localPosition = Vector3.zero;

            LineRenderer ray = rayObject.AddComponent<LineRenderer>();
            ray.positionCount = 2;
            ray.useWorldSpace = false;
            ray.startWidth = rayWidth;
            ray.endWidth = 0.01f;
            ray.material = new Material(Shader.Find("Sprites/Default"));
            ray.startColor = Color.white;
            ray.endColor = new Color(1f, 1f, 0.6f, 0f);
            ray.sortingOrder = spriteRenderer != null ? spriteRenderer.sortingOrder - 1 : 0;
            rays[i] = ray;
        }
    }

    void EnsureLightExists()
    {
        if (light2D != null)
        {
            return;
        }

        GameObject lightObject = new GameObject("ScatteredLightGlow");
        lightObject.transform.SetParent(transform);
        lightObject.transform.localPosition = Vector3.zero;

        light2D = lightObject.AddComponent<Light2D>();
        light2D.lightType = Light2D.LightType.Point;
        light2D.color = new Color(1f, 0.95f, 0.7f, 1f);
    }

    void UpdateLight()
    {
        if (light2D == null)
        {
            return;
        }

        float pulse = Mathf.Sin(Time.time * lightPulseSpeed) * lightPulseAmount;

        if (isCollected)
        {
            light2D.intensity = collectedLightIntensity + pulse;
            light2D.pointLightOuterRadius = collectedOuterRadius + pulse;
        }
        else
        {
            light2D.intensity = idleLightIntensity + pulse * 0.5f;
            light2D.pointLightOuterRadius = idleOuterRadius + pulse * 0.5f;
        }

        light2D.pointLightInnerRadius = light2D.pointLightOuterRadius * 0.35f;
    }

    void UpdateRays()
    {
        if (rays == null)
        {
            return;
        }

        for (int i = 0; i < rays.Length; i++)
        {
            float angle = (360f / rays.Length) * i + Time.time * 25f;
            float angleRadians = angle * Mathf.Deg2Rad;
            float pulseLength = rayLength + Mathf.Sin(Time.time * 4f + i) * 0.15f;
            Vector3 endPoint = new Vector3(Mathf.Cos(angleRadians) * pulseLength, Mathf.Sin(angleRadians) * pulseLength, 0f);

            rays[i].SetPosition(0, Vector3.zero);
            rays[i].SetPosition(1, endPoint);
        }
    }
}
