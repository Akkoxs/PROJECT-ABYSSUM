using UnityEngine;

public class FishEnemy : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform subTransform;
    [SerializeField] private Animator animator;
    private Transform mainTransform;

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

        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerTransform = player.transform;
            }
        }

        if (subTransform == null)
        {
            GameObject submarine = GameObject.FindGameObjectWithTag("Submarine");
            if (submarine != null)
            {
                subTransform = submarine.transform;
            }
        }
    }

    void Update()
    {
        if (subTransform.gameObject.GetComponent<Submarine>().PlayerInside)
        {
            mainTransform = subTransform;
        } else
        {
            mainTransform = playerTransform;
        }
        if (mainTransform == null)
        {
            currentState = FishState.Patrol;
        }

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

        float distanceToPlayer = Vector2.Distance(transform.position, mainTransform.position);

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

    void HandlePatrol(float distanceToPlayer)
    {
        targetVelocity = new Vector2(patrolDirection * patrolSpeed, 0f);

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = patrolDirection > 0;
        }

        if (distanceToPlayer <= detectionRange)
        {
            currentState = FishState.Charging;
            stateTimer = chargeDuration;
        }
    }

    void HandleCharging(float distanceToPlayer)
    {
        Vector2 directionToPlayer = (mainTransform.position - transform.position).normalized;
        targetVelocity = directionToPlayer * chargeSpeed;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = directionToPlayer.x > 0;
        }

        if (distanceToPlayer <= minAttackDistance || stateTimer <= 0f)
        {
            currentState = FishState.Retreating;
            stateTimer = retreatDuration;
        }

        if (distanceToPlayer > detectionRange)
        {
            currentState = FishState.Patrol;
        }
    }

    void HandleRetreating()
    {
        Vector2 directionFromPlayer = (transform.position - mainTransform.position).normalized;
        targetVelocity = directionFromPlayer * retreatSpeed;

        if (stateTimer <= 0f)
        {
            currentState = FishState.Pausing;
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
                currentState = FishState.Charging;
                stateTimer = chargeDuration;
            }
            else
            {
                currentState = FishState.Patrol;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile") && !isInvulnerable)
        {
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityDuration;
            animator.SetTrigger("hurt");
            Debug.Log("Fish hit! Now invulnerable for " + invulnerabilityDuration + " seconds");

            currentState = FishState.Patrol;
        }

        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damageAmount);
            collision.gameObject.GetComponent<Animator>().SetTrigger("hit");
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain") || collision.gameObject.layer == LayerMask.NameToLayer("Floor"))
        {
            patrolDirection *= -1f;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minAttackDistance);
    }
}