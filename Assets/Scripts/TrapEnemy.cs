using UnityEngine;

public class TrapEnemy : MonoBehaviour
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

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // State machine
    private enum TrapState { Dormant, Activating, Patrol, Charging, Retreating, Pausing }
    private TrapState currentState = TrapState.Dormant;
    private float stateTimer = 0f;

    private float patrolDirection;
    private bool isInvulnerable = false;
    private float invulnerabilityTimer = 0f;
    private Vector2 targetVelocity = Vector2.zero;
    private bool isActivated = false;

    void Start()
    {
        patrolDirection = initialDirection;

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
        }
    }

    void Update()
    {
        if (playerTransform == null) return;

        // Handle invulnerability
        if (isInvulnerable)
        {
            animator.SetBool("nohit", true);
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0f)
            {
                isInvulnerable = false;
                animator.SetBool("nohit", false);
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
        // Stay completely still
        targetVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;

        // Check if player is close enough to activate
        if (distanceToPlayer <= activationRange)
        {
            currentState = TrapState.Activating;
            stateTimer = activationDelay;
            isActivated = true;

            // Trigger activation animation
            if (animator != null)
            {
                animator.SetTrigger(activationAnimationTrigger);
            }

            Debug.Log("Trap activated!");
        }
    }

    void HandleActivating()
    {
        // Stay still during activation animation
        targetVelocity = Vector2.zero;

        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

        // After activation delay, decide what to do based on player distance
        if (stateTimer <= 0f)
        {
            if (distanceToPlayer <= attackRange)
            {
                // Player is close - start attacking immediately
                currentState = TrapState.Charging;
                stateTimer = chargeDuration;
                Debug.Log("Trap activated - player close, charging!");
            }
            else
            {
                // Player is far - start patrolling
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

        // Only start charging if player is within attack range (not detection range)
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

        // Return to patrol if player gets too far
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
            animator.SetTrigger("hurt");
            Debug.Log("Trap hit! Now invulnerable for " + invulnerabilityDuration + " seconds");

            currentState = TrapState.Patrol;
        }

        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

        if (damageable != null && isActivated)
        {
            damageable.TakeDamage(damageAmount);
            animator.SetTrigger("attack");
            collision.gameObject.GetComponent<Animator>().SetTrigger("hit");
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain") || collision.gameObject.layer == LayerMask.NameToLayer("Floor"))
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
}