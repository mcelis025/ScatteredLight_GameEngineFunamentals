using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class SpeedEnemy : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public PlayerController playerController;

    [Header("Chase")]
    public float speed = 7f;
    public float chaseTime = 5f;
    public float cameraViewTimeNeeded = 1f;
    public float retargetTime = 0.3f;

    [Header("Explosion")]
    public float tiredTime = 2f;
    public float explosionSizeMultiplier = 3f;
    public float tiredPulseSize = 0.15f;
    public float warningRingWidth = 0.08f;
    public Color warningRingColor = new Color(1f, 0.5f, 0.1f, 0.8f);

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private LineRenderer warningRing;
    private Vector3 startScale;
    private float chaseTimer;
    private float tiredTimer;
    private float retargetTimer;
    private float sameCameraTimer;
    private float chaseDirection;
    private bool hasDetectedPlayer;
    private bool isChasing = true;
    private bool isTired;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb.gravityScale = 1f;
        rb.freezeRotation = true;
        startScale = transform.localScale;
        SetUpWarningRing();

        FindPlayerIfNeeded();
    }

    void Update()
    {
        FindPlayerIfNeeded();

        if (player == null || playerController == null)
        {
            StopMoving();
            return;
        }

        UpdateCameraViewDetection();

        if (!hasDetectedPlayer)
        {
            StopMoving();
            return;
        }

        if (isChasing)
        {
            HandleChase();
        }
        else if (isTired)
        {
            HandleTiredState();
        }
    }

    void HandleChase()
    {
        chaseTimer += Time.deltaTime;
        retargetTimer -= Time.deltaTime;

        if (retargetTimer <= 0f)
        {
            UpdateChaseDirection();
            retargetTimer = retargetTime;
        }

        MoveTowardPlayer();

        if (chaseTimer >= chaseTime)
        {
            isChasing = false;
            isTired = true;
            StopMoving();
        }
    }

    void HandleTiredState()
    {
        tiredTimer += Time.deltaTime;
        StopMoving();
        ShowTiredPulse();
        UpdateWarningRing();

        if (tiredTimer >= tiredTime)
        {
            Explode();
        }
    }

    void MoveTowardPlayer()
    {
        rb.linearVelocity = new Vector2(chaseDirection * speed, rb.linearVelocity.y);
    }

    void UpdateChaseDirection()
    {
        float distanceToPlayer = player.position.x - transform.position.x;

        if (Mathf.Abs(distanceToPlayer) < 0.1f)
        {
            return;
        }

        chaseDirection = Mathf.Sign(distanceToPlayer);
    }

    void UpdateCameraViewDetection()
    {
        if (hasDetectedPlayer)
        {
            return;
        }

        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            sameCameraTimer = 0f;
            return;
        }

        bool playerIsVisible = IsInsideCameraView(mainCamera.WorldToViewportPoint(player.position));
        bool enemyIsVisible = IsInsideCameraView(mainCamera.WorldToViewportPoint(transform.position));

        if (!playerIsVisible || !enemyIsVisible)
        {
            sameCameraTimer = 0f;
            return;
        }

        sameCameraTimer += Time.deltaTime;

        if (sameCameraTimer >= cameraViewTimeNeeded)
        {
            StartChase();
        }
    }

    bool IsInsideCameraView(Vector3 viewportPoint)
    {
        if (viewportPoint.z < 0f)
        {
            return false;
        }

        if (viewportPoint.x < 0f || viewportPoint.x > 1f)
        {
            return false;
        }

        if (viewportPoint.y < 0f || viewportPoint.y > 1f)
        {
            return false;
        }

        return true;
    }

    void StartChase()
    {
        hasDetectedPlayer = true;
        sameCameraTimer = 0f;
        chaseTimer = 0f;
        tiredTimer = 0f;
        isChasing = true;
        isTired = false;
        HideWarningRing();
        UpdateChaseDirection();
        retargetTimer = retargetTime;
    }

    void Explode()
    {
        transform.localScale = startScale;
        HideWarningRing();

        float explosionRadius = GetExplosionRadius();
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= explosionRadius)
        {
            playerController.TakeDamage(1);
        }

        Destroy(gameObject);
    }

    float GetExplosionRadius()
    {
        if (spriteRenderer != null)
        {
            float enemySize = spriteRenderer.bounds.size.x;
            return enemySize * explosionSizeMultiplier;
        }

        return transform.localScale.x * explosionSizeMultiplier;
    }

    void ShowTiredPulse()
    {
        float pulse = Mathf.Sin(Time.time * 12f) * tiredPulseSize;
        transform.localScale = startScale + new Vector3(pulse, pulse, 0f);
    }

    void StopMoving()
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
        }
    }

    void SetUpWarningRing()
    {
        GameObject ringObject = new GameObject("ExplosionWarningRing");
        ringObject.transform.SetParent(transform);
        ringObject.transform.localPosition = Vector3.zero;

        warningRing = ringObject.AddComponent<LineRenderer>();
        warningRing.useWorldSpace = false;
        warningRing.loop = true;
        warningRing.positionCount = 40;
        warningRing.startWidth = warningRingWidth;
        warningRing.endWidth = warningRingWidth;
        warningRing.material = new Material(Shader.Find("Sprites/Default"));
        warningRing.startColor = warningRingColor;
        warningRing.endColor = warningRingColor;
        warningRing.sortingOrder = 2;
        warningRing.enabled = false;
    }

    void UpdateWarningRing()
    {
        if (warningRing == null)
        {
            return;
        }

        float radius = GetExplosionRadius();
        float pulse = 1f + Mathf.Sin(Time.time * 12f) * 0.08f;

        for (int i = 0; i < warningRing.positionCount; i++)
        {
            float angle = (float)i / warningRing.positionCount * Mathf.PI * 2f;
            float x = Mathf.Cos(angle) * radius * pulse;
            float y = Mathf.Sin(angle) * radius * pulse;
            warningRing.SetPosition(i, new Vector3(x, y, 0f));
        }

        warningRing.enabled = true;
    }

    void HideWarningRing()
    {
        if (warningRing != null)
        {
            warningRing.enabled = false;
        }
    }

    void FindPlayerIfNeeded()
    {
        if (player != null && playerController != null)
        {
            return;
        }

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject == null)
        {
            return;
        }

        player = playerObject.transform;
        playerController = playerObject.GetComponent<PlayerController>();
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController hitPlayer = collision.gameObject.GetComponent<PlayerController>();

            if (hitPlayer != null)
            {
                hitPlayer.TakeDamage(1);
            }
        }
    }

    void OnDisable()
    {
        HideWarningRing();
    }
}
