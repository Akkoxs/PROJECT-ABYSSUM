using UnityEngine;

public class FishEnemy : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator animator;

    [Header("Detection")]
    [SerializeField] private float detectionRange = 8f;
    [SerializeField] private float attackRange = 5f;

    [Header("Patrol Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float initialDirection = 1f;

    [Header("Attack Settings")]
    [SerializeField] private float chargeSpeed = 8f; // Speed when lunging at player
    [SerializeField] private float retreatSpeed = 4f; // Speed when backing off
    [SerializeField] private float chargeDuration = 0.8f; // How long to charge forward
    [SerializeField] private float retreatDuration = 0.6f; // How long to retreat
    [SerializeField] private float pauseBetweenBites = 0.5f; // Pause before next bite
    [SerializeField] private float minAttackDistance = 1.5f; // Stop charging when this close

    [Header("Underwater Physics")]
    [SerializeField] private float acceleration = 3f;
    [SerializeField] private float deceleration = 0.92f;
    [SerializeField] private float waterDrag = 0.95f;

    [Header("Combat")]
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float invulnerabilityDuration = 2f; // How long fish is invulnerable after being hit

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // State machine
    private enum FishState { Patrol, Charging, Retreating, Pausing }
    private FishState currentState = FishState.Patrol;
    private float stateTimer = 0f;

    private float patrolDirection;
    private bool isInvulnerable = false;
    private float invulnerabilityTimer = 0f;
    private Vector2 targetVelocity = Vector2.zero;

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

        // Auto-find player if not assigned
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
        if (playerTransform == null)
        {
            currentState = FishState.Patrol;
        }

        // Handle invulnerability timer
        if (isInvulnerable)
        {
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
            case FishState.Patrol:
                HandlePatrol(distanceToPlayer);
                break;

            case FishState.Charging:
                HandleCharging(distanceToPlayer);
                break;

            case FishState.Retreating:
                HandleRetreating();
                break;

            case FishState.Pausing:
                HandlePausing(distanceToPlayer);
                break;
        }

        // Update state timer
        stateTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        // Smoothly accelerate/decelerate to target velocity
        Vector2 currentVelocity = rb.linearVelocity;

        if (targetVelocity.magnitude > 0.1f)
        {
            // Accelerate toward target
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            // Decelerate when no target
            currentVelocity *= deceleration;
        }

        // Apply water drag
        rb.linearVelocity = currentVelocity * waterDrag;
    }

    void HandlePatrol(float distanceToPlayer)
    {
        targetVelocity = new Vector2(patrolDirection * patrolSpeed, 0f);

        // Flip sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = patrolDirection > 0; // For left-facing sprites
        }

        // Check if player is in detection range
        if (distanceToPlayer <= detectionRange)
        {
            // Start attacking
            currentState = FishState.Charging;
            stateTimer = chargeDuration;
        }
    }

    void HandleCharging(float distanceToPlayer)
    {
        // Calculate direction to player
        Vector2 directionToPlayer = (playerTransform.position - transform.position).normalized;
        targetVelocity = directionToPlayer * chargeSpeed;

        // Flip sprite based on direction
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = directionToPlayer.x > 0; // For charging
        }

        // Check if close enough or time ran out
        if (distanceToPlayer <= minAttackDistance || stateTimer <= 0f)
        {
            currentState = FishState.Retreating;
            stateTimer = retreatDuration;
        }

        // If player too far, return to patrol
        if (distanceToPlayer > detectionRange)
        {
            currentState = FishState.Patrol;
        }
    }

    void HandleRetreating()
    {
        // Move away from player
        Vector2 directionFromPlayer = (transform.position - playerTransform.position).normalized;
        targetVelocity = directionFromPlayer * retreatSpeed;

        // When retreat time is up, pause before next bite
        if (stateTimer <= 0f)
        {
            currentState = FishState.Pausing;
            stateTimer = pauseBetweenBites;
            targetVelocity = Vector2.zero; // Stop during pause
        }
    }

    void HandlePausing(float distanceToPlayer)
    {
        targetVelocity = Vector2.zero; // Stay still

        // After pause, decide next action
        if (stateTimer <= 0f)
        {
            if (distanceToPlayer <= attackRange)
            {
                // Start another bite
                currentState = FishState.Charging;
                stateTimer = chargeDuration;
            }
            else
            {
                // Return to patrol
                currentState = FishState.Patrol;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if hit by projectile while not invulnerable
        if (collision.gameObject.CompareTag("Projectile") && !isInvulnerable)
        {
            // Start invulnerability period
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityDuration;

            Debug.Log("Fish hit! Now invulnerable for " + invulnerabilityDuration + " seconds");

            // Reset to patrol after being hit
            currentState = FishState.Patrol;

            // Optional: Add visual feedback (flashing, color change, etc.)
            // StartCoroutine(FlashSprite());
            animator.SetBool("nohit", true);
        }

        // Handle damage to player
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damageAmount);
            animator.SetTrigger("hurt");
        }

        // Reverse patrol direction on wall collision
        if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain") || collision.gameObject.layer == LayerMask.NameToLayer("Floor"))
        {
            patrolDirection *= -1f;
        }
    }

    void OnDrawGizmosSelected()
    {
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