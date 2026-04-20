using UnityEngine;
using UnityEngine.Rendering.Universal;

[RequireComponent(typeof(Rigidbody2D))]
public class FlyingEnemy : MonoBehaviour
{
    [Header("Player")]
    public Transform player;
    public PlayerController playerController;

    [Header("Sight")]
    public float sightRange = 12f;
    public LayerMask sightBlockers;

    [Header("Fade In")]
    public float fadeInTime = 1f;
    public float fadeOutTime = 0.5f;
    public float minLingerTime = 2f;
    public float maxLingerTime = 6f;

    [Header("Reappear")]
    public float minReappearTime = 8f;
    public float maxReappearTime = 12f;

    [Header("Movement")]
    public float hoverAmount = 0.25f;
    public float hoverSpeed = 2f;
    public float attackSpeed = 10f;
    public float hitDistance = 0.4f;

    private SpriteRenderer spriteRenderer;
    private SpriteRenderer[] allSpriteRenderers;
    private Light2D[] glowLights;
    private float[] glowLightBaseIntensity;
    private Collider2D mainCollider;
    private Rigidbody2D rb;
    private Vector3 startPosition;
    private Vector3 attackTarget;
    private float fadeTimer;
    private float fadeOutTimer;
    private float lingerTimer;
    private float hiddenTimer;
    private float currentLingerTime;
    private float currentReappearTime;
    private int attackCount;
    private bool hasHitPlayerThisAttack;
    private bool isHidden;
    private bool isFadingIn = true;
    private bool isFadingOut;
    private bool isWatching;
    private bool isAttacking;
    private bool isReturning;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        allSpriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
        glowLights = GetComponentsInChildren<Light2D>(true);
        glowLightBaseIntensity = new float[glowLights.Length];
        mainCollider = GetComponent<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        startPosition = transform.position;
        FindPlayerIfNeeded();
        PrepareForAppearance();

        for (int i = 0; i < glowLights.Length; i++)
        {
            glowLightBaseIntensity[i] = glowLights[i].intensity;
        }

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.linearVelocity = Vector2.zero;
        }

        if (mainCollider != null)
        {
            mainCollider.isTrigger = true;
        }
    }

    void Update()
    {
        FindPlayerIfNeeded();

        if (player == null || playerController == null)
        {
            return;
        }

        if (isHidden)
        {
            HandleHiddenState();
            return;
        }

        if (isFadingIn)
        {
            HandleFadeIn();
            return;
        }

        if (isFadingOut)
        {
            HandleFadeOut();
            return;
        }

        if (isAttacking)
        {
            HandleAttack();
            return;
        }

        if (isReturning)
        {
            ReturnToStart();
            return;
        }

        HoverInPlace();
        WatchForPlayerMovement();
    }

    void HandleHiddenState()
    {
        hiddenTimer += Time.deltaTime;

        if (hiddenTimer >= currentReappearTime)
        {
            PrepareForAppearance();
        }
    }

    void HandleFadeIn()
    {
        fadeTimer += Time.deltaTime;
        float alpha = 1f;

        if (fadeInTime > 0f)
        {
            alpha = fadeTimer / fadeInTime;
        }

        SetAlpha(alpha);

        if (fadeTimer >= fadeInTime)
        {
            isFadingIn = false;
            isWatching = true;
            SetAlpha(1f);
            SetColliderState(true);
        }
    }

    void HandleFadeOut()
    {
        fadeOutTimer += Time.deltaTime;
        float alpha = 0f;

        if (fadeOutTime > 0f)
        {
            alpha = 1f - (fadeOutTimer / fadeOutTime);
        }

        SetAlpha(alpha);

        if (fadeOutTimer >= fadeOutTime)
        {
            FinishHiddenCycle();
        }
    }

    void WatchForPlayerMovement()
    {
        lingerTimer += Time.deltaTime;

        if (CanSeePlayer() && playerController.IsPlayerMoving())
        {
            isWatching = false;
            StartAttack();
            return;
        }

        if (lingerTimer >= currentLingerTime)
        {
            StartFadeOut();
        }
    }

    void HoverInPlace()
    {
        float hoverOffset = Mathf.Sin(Time.time * hoverSpeed) * hoverAmount;
        Vector3 hoverPosition = startPosition + new Vector3(0f, hoverOffset, 0f);
        transform.position = hoverPosition;
    }

    void HandleAttack()
    {
        transform.position = Vector3.MoveTowards(transform.position, attackTarget, attackSpeed * Time.deltaTime);

        float distanceToTarget = Vector3.Distance(transform.position, attackTarget);
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (!hasHitPlayerThisAttack && distanceToPlayer <= hitDistance)
        {
            hasHitPlayerThisAttack = true;
            playerController.TakeDamage(1);
        }

        if (distanceToTarget <= 0.1f)
        {
            attackCount += 1;

            if (attackCount >= 3)
            {
                StartFadeOut();
                return;
            }

            isAttacking = false;
            isReturning = true;
        }
    }

    void ReturnToStart()
    {
        transform.position = Vector3.MoveTowards(transform.position, startPosition, attackSpeed * Time.deltaTime);

        float distanceToStart = Vector3.Distance(transform.position, startPosition);

        if (distanceToStart <= 0.1f)
        {
            isReturning = false;
            StartAttack();
        }
    }

    void StartAttack()
    {
        isAttacking = true;
        attackTarget = player.position;
        hasHitPlayerThisAttack = false;
    }

    void PrepareForAppearance()
    {
        isHidden = false;
        isFadingIn = true;
        isFadingOut = false;
        isWatching = false;
        isAttacking = false;
        isReturning = false;
        fadeTimer = 0f;
        fadeOutTimer = 0f;
        lingerTimer = 0f;
        hiddenTimer = 0f;
        attackCount = 0;
        hasHitPlayerThisAttack = false;
        currentLingerTime = Random.Range(minLingerTime, maxLingerTime);
        currentReappearTime = Random.Range(minReappearTime, maxReappearTime);
        transform.position = startPosition;
        SetAlpha(0f);
        SetColliderState(false);
    }

    void StartFadeOut()
    {
        if (isHidden || isFadingOut)
        {
            return;
        }

        isFadingIn = false;
        isFadingOut = true;
        isWatching = false;
        isAttacking = false;
        isReturning = false;
        fadeOutTimer = 0f;
        hasHitPlayerThisAttack = false;
        SetColliderState(false);
    }

    void FinishHiddenCycle()
    {
        isHidden = true;
        isFadingIn = false;
        isFadingOut = false;
        isWatching = false;
        isAttacking = false;
        isReturning = false;
        hiddenTimer = 0f;
        hasHitPlayerThisAttack = false;
        currentReappearTime = Random.Range(minReappearTime, maxReappearTime);
        transform.position = startPosition;
        SetAlpha(0f);
        SetColliderState(false);
    }

    void SetColliderState(bool active)
    {
        if (mainCollider != null)
        {
            mainCollider.enabled = active;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        PlayerController hitPlayer = other.GetComponent<PlayerController>();

        if (hitPlayer == null)
        {
            return;
        }

        hasHitPlayerThisAttack = true;
        hitPlayer.TakeDamage(1);
    }

    bool CanSeePlayer()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer > sightRange)
        {
            return false;
        }

        if (sightBlockers.value == 0)
        {
            return true;
        }

        Vector2 direction = player.position - transform.position;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction.normalized, distanceToPlayer, sightBlockers);

        if (hit.collider == null)
        {
            return true;
        }

        return false;
    }

    void SetAlpha(float alpha)
    {
        float clampedAlpha = Mathf.Clamp01(alpha);

        for (int i = 0; i < allSpriteRenderers.Length; i++)
        {
            if (allSpriteRenderers[i] == null)
            {
                continue;
            }

            Color color = allSpriteRenderers[i].color;
            color.a = clampedAlpha;
            allSpriteRenderers[i].color = color;
        }

        for (int i = 0; i < glowLights.Length; i++)
        {
            if (glowLights[i] == null)
            {
                continue;
            }

            glowLights[i].intensity = glowLightBaseIntensity[i] * clampedAlpha;
            glowLights[i].enabled = clampedAlpha > 0.01f;
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
}
