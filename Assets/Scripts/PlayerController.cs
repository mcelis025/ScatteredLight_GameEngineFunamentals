using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float normalSpeed = 5f;
    public float slowSpeed = 2f;
    public float jumpForce = 10f;

    [Header("Light")]
    public LightEmitter lightEmitter;
    public float footstepPulseSize = 5f;
    public float footstepHearingRange = 6f;
    public float footstepInterval = 0.28f;

    [Header("Player Aura")]
    public Color auraColor = new Color(0.46f, 0.53f, 0.78f, 1f);
    public float auraIntensity = 0.7f;
    public float auraInnerRadius = 0.45f;
    public float auraOuterRadius = 2.6f;

    [Header("Float")]
    public float floatFallMultiplier = 0.5f;

    [Header("Stamina")]
    public float maxStamina = 3f;

    [Header("Health")]
    public int maxHealth = 3;
    public float damageCooldown = 1f;

    [Header("Landing Impact")]
    public float softLandingImpact = 4f;
    public float hardLandingImpact = 8f;
    public float softLandingLightSize = 1f;
    public float hardLandingLightSize = 6f;

    [Header("Light Dash")]
    public float lightDashWindow = 8f;
    public float lightDashDistance = 8f;
    public float lightDashDuration = 0.15f;
    public float lightDashCooldown = 0.4f;
    public float lightDashBrightness = 8f;
    
    [Header("Stamina Dash")]
    public float staminaDashDistanceMultiplier = 0.5f;
    public float staminaDashCostPercent = 0.5f;

    private Rigidbody2D rb;
    private Collider2D bodyCollider;
    private Light2D auraLight;
    private LineRenderer dashLine;

    private bool isGrounded;
    private bool hasLandedOnce = false;
    private bool isSneaking;
    private bool isFloating;
    private bool hasUsedLightDash;
    private bool lightDashIsActive;
    private bool isDashing;
    private bool shiftDashUsedThisHold;
    private bool ignoreDamageWhileDashing;

    private int currentHealth;
    private float damageCooldownTimer;
    private float stamina;
    private float lastYVelocity;
    private float footstepTimer;
    private float moveInputX;
    private float moveInputY;
    private bool dashInputPressedThisFrame;

    private float lightDashTimer;
    private float dashTimer;
    private float dashCooldownTimer;
    private float dashVisualTimer;
    private Vector2 dashStartPosition;
    private Vector2 dashEndPosition;
    private Vector2 dashDirection = Vector2.right;
    private HashSet<Collider2D> groundColliders = new HashSet<Collider2D>();

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        bodyCollider = GetComponent<Collider2D>();

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.angularVelocity = 0f;
        }

        stamina = maxStamina;
        currentHealth = maxHealth;

        SetUpDashLine();
        EnsureAuraLight();
        StaminaUI.EnsureExists(this);
        EnsureEnemyManager();
        EnsureCameraFollow();
    }

    void Update()
    {
        if (Keyboard.current == null || rb == null)
        {
            return;
        }

        ReadMovementInput();
        HandleLightDashMode();
        HandleStaminaDash();
        UpdateTimers();

        if (!isDashing)
        {
            HandleMovement();
            HandleJumpAndFloat();
            HandleStamina();
            HandleFootsteps();
        }
    }

    void FixedUpdate()
    {
        if (rb == null)
        {
            return;
        }

        if (isDashing)
        {
            MoveDuringLightDash();
        }
        else if (isFloating && rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * floatFallMultiplier);
        }

        lastYVelocity = rb.linearVelocity.y;
    }

    void ReadMovementInput()
    {
        moveInputX = 0f;
        moveInputY = 0f;
        dashInputPressedThisFrame = false;

        if (Keyboard.current.aKey.isPressed)
        {
            moveInputX = -1f;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            moveInputX = 1f;
        }

        if (Keyboard.current.sKey.isPressed)
        {
            moveInputY = -1f;
        }

        if (Keyboard.current.wKey.isPressed)
        {
            moveInputY = 1f;
        }

        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            dashInputPressedThisFrame = true;
        }

        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            dashInputPressedThisFrame = true;
        }

        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            dashInputPressedThisFrame = true;
        }

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            dashInputPressedThisFrame = true;
        }

        Vector2 currentDirection = new Vector2(moveInputX, moveInputY);

        if (currentDirection.sqrMagnitude > 0.01f)
        {
            dashDirection = currentDirection.normalized;
        }

        if (!Keyboard.current.leftShiftKey.isPressed)
        {
            shiftDashUsedThisHold = false;
        }

        isSneaking = Keyboard.current.cKey.isPressed && stamina > 0f;
    }

    void HandleMovement()
    {
        float speed = normalSpeed;

        if (isSneaking)
        {
            speed = slowSpeed;
        }

        rb.linearVelocity = new Vector2(moveInputX * speed, rb.linearVelocity.y);
    }

    void HandleJumpAndFloat()
    {
        if (Keyboard.current.spaceKey.wasPressedThisFrame && isGrounded)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);

            if (AudioManager.instance != null)
            {
                AudioManager.instance.PlayJump();
            }
        }

        if (!isGrounded && Keyboard.current.spaceKey.isPressed && stamina > 0f)
        {
            isFloating = true;
        }
        else
        {
            isFloating = false;
        }
    }

    void HandleStamina()
    {
        if (isFloating)
        {
            stamina -= Time.deltaTime;
        }

        if (isSneaking && Mathf.Abs(moveInputX) > 0.1f)
        {
            stamina -= Time.deltaTime;
        }

        if (isGrounded && Mathf.Abs(moveInputX) < 0.1f && !isSneaking && !isFloating)
        {
            stamina += Time.deltaTime;
        }

        stamina = Mathf.Clamp(stamina, 0f, maxStamina);
    }

    void HandleFootsteps()
    {
        if (Mathf.Abs(moveInputX) <= 0.1f || !isGrounded)
        {
            footstepTimer = 0f;
            return;
        }

        footstepTimer += Time.deltaTime;

        if (footstepTimer <= footstepInterval)
        {
            return;
        }

        footstepTimer = 0f;

        if (isSneaking)
        {
            return;
        }

        if (lightEmitter != null)
        {
            lightEmitter.EmitFootstepLight(GetFootstepPosition(), footstepPulseSize);
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayFootstep();
        }
    }

    Vector2 GetFootstepPosition()
    {
        if (bodyCollider != null)
        {
            Bounds bounds = bodyCollider.bounds;
            return new Vector2(bounds.center.x, bounds.min.y + 0.02f);
        }

        return (Vector2)transform.position + Vector2.down * 0.5f;
    }

    void EnsureAuraLight()
    {
        auraLight = GetComponentInChildren<Light2D>();

        if (auraLight == null)
        {
            GameObject auraObject = new GameObject("PlayerAuraLight");
            auraObject.transform.SetParent(transform, false);
            auraObject.transform.localPosition = new Vector3(0f, -0.08f, 0f);
            auraLight = auraObject.AddComponent<Light2D>();
        }

        auraLight.lightType = Light2D.LightType.Point;
        auraLight.color = auraColor;
        auraLight.intensity = auraIntensity;
        auraLight.pointLightInnerRadius = auraInnerRadius;
        auraLight.pointLightOuterRadius = auraOuterRadius;
        auraLight.falloffIntensity = 0.85f;
        auraLight.shadowsEnabled = false;
    }

    void HandleLightDashMode()
    {
        if (Keyboard.current.eKey.wasPressedThisFrame && !hasUsedLightDash)
        {
            lightDashIsActive = true;
            hasUsedLightDash = true;
            lightDashTimer = lightDashWindow;
        }

        if (!lightDashIsActive)
        {
            return;
        }

        if (isDashing)
        {
            return;
        }

        if (dashCooldownTimer > 0f)
        {
            return;
        }

        Vector2 currentDirection = new Vector2(moveInputX, moveInputY);

        if (dashInputPressedThisFrame && currentDirection.sqrMagnitude > 0.01f)
        {
            dashDirection = currentDirection.normalized;
            StartLightDash();
        }
    }

    void HandleStaminaDash()
    {
        if (Keyboard.current.leftShiftKey == null)
        {
            return;
        }

        if (!Keyboard.current.leftShiftKey.isPressed)
        {
            return;
        }

        if (shiftDashUsedThisHold || isDashing || dashCooldownTimer > 0f)
        {
            return;
        }

        Vector2 currentDirection = new Vector2(moveInputX, moveInputY);

        if (currentDirection.sqrMagnitude <= 0.01f)
        {
            return;
        }

        float staminaDashCost = GetStaminaDashCost();

        if (stamina < staminaDashCost)
        {
            return;
        }

        shiftDashUsedThisHold = true;
        dashDirection = currentDirection.normalized;
        stamina -= staminaDashCost;
        stamina = Mathf.Clamp(stamina, 0f, maxStamina);
        StartStaminaDash();
    }

    void UpdateTimers()
    {
        if (lightDashIsActive)
        {
            lightDashTimer -= Time.deltaTime;

            if (lightDashTimer <= 0f)
            {
                lightDashIsActive = false;
            }
        }

        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }

        if (dashVisualTimer > 0f)
        {
            dashVisualTimer -= Time.deltaTime;

            if (dashVisualTimer <= 0f && dashLine != null)
            {
                dashLine.enabled = false;
            }
        }

        if (damageCooldownTimer > 0f)
        {
            damageCooldownTimer -= Time.deltaTime;
        }
    }

    void StartLightDash()
    {
        StartDash(lightDashDistance, lightDashBrightness, true);
    }

    void StartStaminaDash()
    {
        float staminaDashDistance = lightDashDistance * staminaDashDistanceMultiplier;
        StartDash(staminaDashDistance, 0f, false);
    }

    void StartDash(float dashDistance, float dashLightSize, bool shouldIgnoreDamage)
    {
        isDashing = true;
        ignoreDamageWhileDashing = shouldIgnoreDamage;
        dashTimer = lightDashDuration;
        dashCooldownTimer = lightDashCooldown;

        dashStartPosition = rb.position;
        dashEndPosition = dashStartPosition + dashDirection * dashDistance;

        rb.linearVelocity = Vector2.zero;
        ShowLightDashStreak();

        if (lightEmitter != null && dashLightSize > 0f)
        {
            lightEmitter.EmitLight(dashStartPosition, dashLightSize);
            lightEmitter.EmitLight(dashEndPosition, dashLightSize);
        }
    }

    void MoveDuringLightDash()
    {
        dashTimer -= Time.fixedDeltaTime;

        if (dashTimer <= 0f)
        {
            rb.position = dashEndPosition;
            rb.linearVelocity = Vector2.zero;
            isDashing = false;
            ignoreDamageWhileDashing = false;
            return;
        }

        float dashPercent = 1f - (dashTimer / lightDashDuration);
        Vector2 newPosition = Vector2.Lerp(dashStartPosition, dashEndPosition, dashPercent);
        rb.MovePosition(newPosition);
    }

    void SetUpDashLine()
    {
        GameObject lineObject = new GameObject("Light Dash Streak");
        lineObject.transform.SetParent(transform);

        dashLine = lineObject.AddComponent<LineRenderer>();
        dashLine.positionCount = 2;
        dashLine.startWidth = 0.35f;
        dashLine.endWidth = 0.05f;
        dashLine.startColor = Color.white;
        dashLine.endColor = Color.yellow;

        Shader dashShader = Shader.Find("Sprites/Default");

        if (dashShader != null)
        {
            dashLine.material = new Material(dashShader);
        }

        dashLine.enabled = false;
    }

    void ShowLightDashStreak()
    {
        if (dashLine == null)
        {
            return;
        }

        dashLine.enabled = true;
        dashLine.SetPosition(0, dashStartPosition);
        dashLine.SetPosition(1, dashEndPosition);
        dashVisualTimer = 0.2f;
    }

    void HandleLanding(Collision2D collision)
    {
        float impactFromCollision = Mathf.Abs(collision.relativeVelocity.y);
        float impactFromFallSpeed = Mathf.Max(0f, -lastYVelocity);
        float impact = Mathf.Max(impactFromCollision, impactFromFallSpeed);

        if (!hasLandedOnce)
        {
            hasLandedOnce = true;
            return;
        }

        if (impact < softLandingImpact)
        {
            return;
        }

        if (impact >= hardLandingImpact)
        {
            MakeLandingNoise(impact, hardLandingLightSize);
        }
        else
        {
            MakeLandingNoise(impact, softLandingLightSize);
        }
    }

    bool IsEnemyCollider(Collider2D otherCollider)
    {
        if (otherCollider == null)
        {
            return false;
        }

        if (otherCollider.GetComponent<ChaseEnemy>() != null)
        {
            return true;
        }

        if (otherCollider.GetComponent<FlyingEnemy>() != null)
        {
            return true;
        }

        if (otherCollider.GetComponent<SpeedEnemy>() != null)
        {
            return true;
        }

        return false;
    }

    bool IsGroundCollision(Collision2D collision)
    {
        if (collision.collider == null || collision.collider.isTrigger)
        {
            return false;
        }

        if (IsEnemyCollider(collision.collider))
        {
            return false;
        }

        ContactPoint2D[] contacts = collision.contacts;

        for (int i = 0; i < contacts.Length; i++)
        {
            if (contacts[i].normal.y > 0.3f)
            {
                return true;
            }
        }

        return false;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!IsGroundCollision(collision))
        {
            return;
        }

        bool wasGrounded = isGrounded;
        groundColliders.Add(collision.collider);
        isGrounded = true;

        if (!wasGrounded)
        {
            HandleLanding(collision);
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (!IsGroundCollision(collision))
        {
            return;
        }

        groundColliders.Add(collision.collider);
        isGrounded = true;
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider == null || collision.collider.isTrigger)
        {
            return;
        }

        if (IsEnemyCollider(collision.collider))
        {
            return;
        }

        groundColliders.Remove(collision.collider);
        isGrounded = groundColliders.Count > 0;
    }

    void MakeLandingNoise(float impact, float lightSize)
    {
        float visualSize = lightSize;
        bool isHardLanding = impact >= hardLandingImpact;

        if (isHardLanding)
        {
            visualSize = GetCameraViewRange();
        }

        if (lightEmitter != null)
        {
            if (isHardLanding)
            {
                lightEmitter.EmitHardLandingLight(transform.position, visualSize);
            }
            else
            {
                lightEmitter.EmitSoftLandingLight(transform.position, visualSize);
            }
        }

        if (AudioManager.instance != null)
        {
            AudioManager.instance.PlayImpact(impact);
        }
    }

    float GetCameraViewRange()
    {
        if (Camera.main == null || !Camera.main.orthographic)
        {
            return hardLandingLightSize;
        }

        float verticalSize = Camera.main.orthographicSize * 2f;
        float horizontalSize = verticalSize * Camera.main.aspect;
        return Mathf.Max(verticalSize, horizontalSize);
    }

    public float GetStaminaPercent()
    {
        if (maxStamina <= 0f)
        {
            return 0f;
        }

        return stamina / maxStamina;
    }

    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    public int GetMaxHealth()
    {
        return maxHealth;
    }

    public bool IsPlayerMoving()
    {
        return Mathf.Abs(moveInputX) > 0.1f || Mathf.Abs(moveInputY) > 0.1f;
    }

    public bool IsPlayerSneaking()
    {
        return isSneaking;
    }

    public bool IsPlayerFloating()
    {
        return isFloating;
    }

    public bool IsLightDashing()
    {
        return isDashing && ignoreDamageWhileDashing;
    }

    float GetStaminaDashCost()
    {
        return maxStamina * staminaDashCostPercent;
    }

    public void DrainStamina(float amount)
    {
        stamina -= amount;
        stamina = Mathf.Clamp(stamina, 0f, maxStamina);
    }

    public void TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            return;
        }

        if (isDashing && ignoreDamageWhileDashing)
        {
            return;
        }

        if (damageCooldownTimer > 0f)
        {
            return;
        }

        currentHealth -= damage;
        damageCooldownTimer = damageCooldown;

        if (currentHealth <= 0)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    void EnsureCameraFollow()
    {
        Camera mainCamera = Camera.main;

        if (mainCamera == null)
        {
            return;
        }

        CameraFollow cameraFollow = mainCamera.GetComponent<CameraFollow>();

        if (cameraFollow == null)
        {
            cameraFollow = mainCamera.gameObject.AddComponent<CameraFollow>();
        }

        cameraFollow.target = transform;
    }

    void EnsureEnemyManager()
    {
        EnemyManager enemyManager = FindFirstObjectByType<EnemyManager>();

        if (enemyManager != null)
        {
            return;
        }

        GameObject managerObject = new GameObject("EnemyManager");
        managerObject.AddComponent<EnemyManager>();
    }
}
