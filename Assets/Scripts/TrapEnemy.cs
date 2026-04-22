using UnityEngine;

public class TrapEnemy : MonoBehaviour, IRadarDetectable
{
    [Header("Player Reference")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator animator;

    [Header("Activation")]
    [SerializeField] private float activationRange = 5f; // Distance to activate trap
    [SerializeField] private string activationAnimationTrigger = "activate"; // Animation trigger name
    [SerializeField] private float activationDelay = 0.5f; // Time before trap starts moving after activation

    [Header("Detection")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 5f;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float initialDirection = 1f;

    [Header("Attack Settings")]
    [SerializeField] private float chargeSpeed = 10f;
    [SerializeField] private float retreatSpeed = 4f;
    [SerializeField] private float chargeDuration = 0.6f;
    [SerializeField] private float retreatDuration = 0.4f;
    [SerializeField] private float pauseBetweenBites = 0.1f;
    [SerializeField] private float minAttackDistance = 1.5f;

    [Header("Underwater Physics")]
    [SerializeField] private float acceleration = 3f;
    [SerializeField] private float deceleration = 0.92f;
    [SerializeField] private float waterDrag = 0.95f;

    [Header("Combat")]
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float invulnerabilityDuration = 2f;

    [Header("Audio")]
    [SerializeField] private AudioClip hurtSFX;
    [SerializeField] private AudioClip deathSFX;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Health health;

    // State machine
    private enum TrapState { Dormant, Activating, Patrol, Charging, Retreating, Pausing }
    private TrapState currentState = TrapState.Dormant;
    private float stateTimer = 0f;

    private float patrolDirection;
    private bool isInvulnerable = false;
    private float invulnerabilityTimer = 0f;
    private Vector2 targetVelocity = Vector2.zero;
    private bool isActivated = false;
    private BoxCollider2D boxCollider2D;

    void Start()
    {
        patrolDirection = initialDirection;
        health = GetComponent<Health>();
        boxCollider2D = GetComponent<BoxCollider2D>();
        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearDamping = 0f;
        }

        spriteRenderer = GetComponent<SpriteRenderer>();

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
            else
            {
            }
        }

    }

    void Update()
    {
        if (playerTransform == null)
        {
            return;
        }

        // Handle invulnerability
        if (isInvulnerable)
        {
            if (animator != null) animator.SetBool("nohit", true);
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0f)
            {
                isInvulnerable = false;
                if (animator != null) animator.SetBool("nohit", false);
            }
        }

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // State machine
        switch (currentState)
        {
            case TrapState.Dormant:
                HandleDormant(distanceToPlayer);
                break;

            case TrapState.Activating:
                HandleActivating();
                break;

            case TrapState.Patrol:
                HandlePatrol(distanceToPlayer);
                break;

            case TrapState.Charging:
                HandleCharging(distanceToPlayer);
                break;

            case TrapState.Retreating:
                HandleRetreating();
                break;

            case TrapState.Pausing:
                HandlePausing(distanceToPlayer);
                break;
        }

        stateTimer -= Time.deltaTime;

        if (health != null && health.CurrentHealth <= 0 && health != null && isActivated)
        {
            AudioEventBus.RequestSFX(new SFXEvent(deathSFX, volume: 1f, pitch: Random.Range(0.9f, 1.1f), pos: transform.position));
            Destroy(gameObject);
        }
    }

    void FixedUpdate()
    {
        Vector2 currentVelocity = rb.linearVelocity;

        if (targetVelocity.magnitude > 0.1f)
        {
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentVelocity *= deceleration;
        }
        rb.linearVelocity = currentVelocity * waterDrag;
    }

    void HandleDormant(float distanceToPlayer)
    {
        targetVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;

        if (distanceToPlayer <= activationRange)
        {
            currentState = TrapState.Activating;
            stateTimer = activationDelay;
            isActivated = true;
            boxCollider2D.enabled = true;

            if (animator != null)
                animator.SetTrigger(activationAnimationTrigger);

            Debug.Log("Trap activated!");
        }
    }

    void HandleActivating()
    {
        targetVelocity = Vector2.zero;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        if (stateTimer <= 0f)
        {
            if (distanceToPlayer <= attackRange)
            {
                currentState = TrapState.Charging;
                stateTimer = chargeDuration;
                Debug.Log("Trap activated - player close, charging!");
            }
            else
            {
                currentState = TrapState.Patrol;
                Debug.Log("Trap activated - patrolling!");
            }
        }
    }

    void HandlePatrol(float distanceToPlayer)
    {
        targetVelocity = new Vector2(patrolDirection * patrolSpeed, 0f);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = patrolDirection > 0;
        }

        if (distanceToPlayer <= attackRange)
        {
            currentState = TrapState.Charging;
            stateTimer = chargeDuration;
        }
    }

    void HandleCharging(float distanceToPlayer)
    {
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        targetVelocity = directionToPlayer * chargeSpeed;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = directionToPlayer.x > 0;
        }

        if (distanceToPlayer <= minAttackDistance || stateTimer <= 0f)
        {
            currentState = TrapState.Retreating;
            stateTimer = retreatDuration;
        }

        if (distanceToPlayer > attackRange * 1.5f)
        {
            currentState = TrapState.Patrol;
        }
    }

    void HandleRetreating()
    {
        Vector2 directionFromPlayer = (transform.position - playerTransform.position).normalized;
        targetVelocity = directionFromPlayer * retreatSpeed;

        if (stateTimer <= 0f)
        {
            currentState = TrapState.Pausing;
            stateTimer = pauseBetweenBites;
            targetVelocity = Vector2.zero;
        }
    }

    void HandlePausing(float distanceToPlayer)
    {
        targetVelocity = Vector2.zero;

        if (stateTimer <= 0f)
        {
            if (distanceToPlayer <= attackRange)
            {
                currentState = TrapState.Charging;
                stateTimer = chargeDuration;
            }
            else
            {
                currentState = TrapState.Patrol;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile") && !isInvulnerable && isActivated)
        {
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityDuration;
            if (animator != null) animator.SetTrigger("hurt");
            AudioEventBus.RequestSFX(new SFXEvent(hurtSFX, volume: 1f, pitch: Random.Range(0.8f, 1.2f), pos: transform.position));
            currentState = TrapState.Patrol;
        }

        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        if (damageable != null && isActivated)
        {
            PlayerController pc = playerTransform != null
                ? playerTransform.gameObject.GetComponent<PlayerController>()
                : null;

            bool playerIsInvulnerable = pc != null && pc.isInvulnerable;

            if (!playerIsInvulnerable)
            {
                damageable.TakeDamage(damageAmount);
                if (animator != null) animator.SetTrigger("attack");
            }
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Floor"))
        {
            patrolDirection *= -1f;
        }
    }

    void OnDrawGizmosSelected()
    {
        // Activation range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activationRange);

        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Min attack distance
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minAttackDistance);
    }

    public RadarObjectType GetObjectType()
    {
        return RadarObjectType.Artifact;
    }

    public string GetRadarDisplayName()
    {
        return "HP+";
    }
}