using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightPulse : MonoBehaviour
{
    public float lifeTime = 1.5f;
    public float expandSpeed = 2f;
    public float startScalePercent = 0.08f;

    private SpriteRenderer sr;
    private CircleCollider2D circleCollider;
    private Rigidbody2D rb;
    private Light2D pulseLight;
    private Vector3 fullScale;
    private Color pulseColor = Color.white;
    private float maxLightIntensity = 1f;
    private HashSet<ChaseEnemy> activatedEnemies = new HashSet<ChaseEnemy>();

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        pulseLight = GetComponentInParent<Light2D>();

        if (circleCollider == null)
        {
            circleCollider = gameObject.AddComponent<CircleCollider2D>();
        }

        circleCollider.isTrigger = true;

        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            rb = gameObject.AddComponent<Rigidbody2D>();
        }

        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
    }

    void Start()
    {
        fullScale = transform.localScale;
        float clampedStartScale = Mathf.Clamp(startScalePercent, 0.01f, 1f);
        transform.localScale = fullScale * clampedStartScale;

        if (sr != null)
        {
            sr.color = pulseColor;
        }

        if (pulseLight != null)
        {
            pulseLight.color = new Color(pulseColor.r, pulseColor.g, pulseColor.b, 1f);
            pulseLight.intensity = maxLightIntensity;
        }

        Destroy(transform.root.gameObject, lifeTime);
    }

    void Update()
    {
        transform.localScale = Vector3.MoveTowards(transform.localScale, fullScale, expandSpeed * Time.deltaTime);

        if (sr == null)
        {
            return;
        }

        Color c = sr.color;
        c.a -= Time.deltaTime / lifeTime;
        sr.color = c;

        if (pulseLight != null)
        {
            float alphaPercent = 0f;

            if (pulseColor.a > 0.001f)
            {
                alphaPercent = Mathf.Clamp01(c.a / pulseColor.a);
            }

            pulseLight.intensity = maxLightIntensity * alphaPercent;
        }
    }

    public void SetPulseSettings(float newLifeTime, float newExpandSpeed, float newStartScalePercent, Color newPulseColor)
    {
        lifeTime = Mathf.Max(0.05f, newLifeTime);
        expandSpeed = Mathf.Max(0.1f, newExpandSpeed);
        startScalePercent = Mathf.Clamp(newStartScalePercent, 0.01f, 1f);
        pulseColor = newPulseColor;
        maxLightIntensity = Mathf.Lerp(0.35f, 1.7f, Mathf.Clamp01(pulseColor.a));

        if (sr != null)
        {
            sr.color = pulseColor;
        }

        if (pulseLight != null)
        {
            pulseLight.color = new Color(pulseColor.r, pulseColor.g, pulseColor.b, 1f);
            pulseLight.intensity = maxLightIntensity;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryActivateChaseEnemy(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryActivateChaseEnemy(other);
    }

    void TryActivateChaseEnemy(Collider2D other)
    {
        if (other == null)
        {
            return;
        }

        ChaseEnemy enemy = other.GetComponent<ChaseEnemy>();

        if (enemy == null)
        {
            enemy = other.GetComponentInParent<ChaseEnemy>();
        }

        if (enemy == null || activatedEnemies.Contains(enemy))
        {
            return;
        }

        activatedEnemies.Add(enemy);
        enemy.HearPulse(transform.position);
    }
}
