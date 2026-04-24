using UnityEngine;

public class TrapEnemy : MonoBehaviour, IRadarDetectable
{
    [Header("Player Reference")]
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Animator animator;

    [Header("Activation")]
    [SerializeField] private float activationRange = 5f;
    [SerializeField] private string activationAnimationTrigger = "activate";
    [SerializeField] private float activationDelay = 0.5f;

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
    [SerializeField] private AudioClip attackSFX;
    [SerializeField] private AudioClip deathSFX;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Health health;

    private enum TrapState { Dormant, Activating, Patrol, Charging, Retreating, Pausing }
    private TrapState currentState = TrapState.Dormant;
    private float stateTimer = 0f;

    private float patrolDirection;
    private bool isInvulnerable = false;
    private float invulnerabilityTimer = 0f;
    private Vector2 targetVelocity = Vector2.zero;
    private bool isActivated = false;
    private BoxCollider2D boxCollider2D;

    private Transform subTransform;
    private Transform mainTransform; // the active chase target, either player or submarine

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
                playerTransform = player.transform;
        }
    }

    void Update()
    {
        // Fallback find for player
        if (playerTransform == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) playerTransform = player.transform;
            else return;
        }

        // Fallback find for submarine
        if (subTransform == null)
        {
            GameObject submarine = GameObject.FindGameObjectWithTag("Submarine");
            if (submarine != null)
                subTransform = submarine.transform;
        }

        UpdateMainTarget();

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

        // Use mainTransform for distance checks when activated, playerTransform for dormant activation
        float distanceToTarget = mainTransform != null
            ? Vector2.Distance(transform.position, mainTransform.position)
            : Vector2.Distance(transform.position, playerTransform.position);

        switch (currentState)
        {
            case TrapState.Dormant:
                HandleDormant(distanceToTarget);
                break;
            case TrapState.Activating:
                HandleActivating();
                break;
            case TrapState.Patrol:
                HandlePatrol(distanceToTarget);
                break;
            case TrapState.Charging:
                HandleCharging(distanceToTarget);
                break;
            case TrapState.Retreating:
                HandleRetreating();
                break;
            case TrapState.Pausing:
                HandlePausing(distanceToTarget);
                break;
        }

        stateTimer -= Time.deltaTime;

        if (health != null && health.CurrentHealth <= 0 && isActivated)
        {
            AudioEventBus.RequestSFX(new SFXEvent(deathSFX, volume: 1f, pitch: Random.Range(0.9f, 1.1f), pos: transform.position));
            Destroy(gameObject);
        }
    }

    void UpdateMainTarget()
    {
        float distToPlayer = playerTransform != null
            ? Vector2.Distance(transform.position, playerTransform.position)
            : float.MaxValue;

        float distToSub = subTransform != null
            ? Vector2.Distance(transform.position, subTransform.position)
            : float.MaxValue;

        bool playerInRange = distToPlayer <= detectionRange;
        bool subInRange = distToSub <= detectionRange;

        if (playerInRange && subInRange)
            mainTransform = distToPlayer < distToSub ? playerTransform : subTransform;
        else if (playerInRange)
            mainTransform = playerTransform;
        else if (subInRange)
            mainTransform = subTransform;
        // neither in range � leave mainTransform unchanged so enemy finishes current chase
    }

    void FixedUpdate()
    {
        Vector2 currentVelocity = rb.linearVelocity;

        if (targetVelocity.magnitude > 0.1f)
            currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        else
            currentVelocity *= deceleration;

        rb.linearVelocity = currentVelocity * waterDrag;
    }

    void HandleDormant(float distanceToTarget)
    {
        targetVelocity = Vector2.zero;
        rb.linearVelocity = Vector2.zero;

        if (distanceToTarget <= activationRange)
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

        if (mainTransform == null) return;

        float distanceToTarget = Vector2.Distance(transform.position, mainTransform.position);

        if (stateTimer <= 0f)
        {
            if (distanceToTarget <= attackRange)
            {
                currentState = TrapState.Charging;
                stateTimer = chargeDuration;
                Debug.Log("Trap activated - target close, charging!");
            }
            else
            {
                currentState = TrapState.Patrol;
                Debug.Log("Trap activated - patrolling!");
            }
        }
    }

    void HandlePatrol(float distanceToTarget)
    {
        targetVelocity = new Vector2(patrolDirection * patrolSpeed, 0f);

        if (spriteRenderer != null)
            spriteRenderer.flipX = patrolDirection > 0;

        if (distanceToTarget <= attackRange)
        {
            currentState = TrapState.Charging;
            stateTimer = chargeDuration;
        }
    }

    void HandleCharging(float distanceToTarget)
    {
        if (mainTransform == null) return;

        Vector2 directionToTarget = (mainTransform.position - transform.position).normalized;
        targetVelocity = directionToTarget * chargeSpeed;
        AudioEventBus.RequestSFX(new SFXEvent(attackSFX, volume: 1f, pitch: Random.Range(0.8f, 1.2f), pos: transform.position));

        if (spriteRenderer != null)
            spriteRenderer.flipX = directionToTarget.x > 0;

        if (distanceToTarget <= minAttackDistance || stateTimer <= 0f)
        {
            currentState = TrapState.Retreating;
            stateTimer = retreatDuration;
        }

        if (distanceToTarget > attackRange * 1.5f)
            currentState = TrapState.Patrol;
    }

    void HandleRetreating()
    {
        if (mainTransform == null) return;

        Vector2 directionFromTarget = (transform.position - mainTransform.position).normalized;
        targetVelocity = directionFromTarget * retreatSpeed;

        if (stateTimer <= 0f)
        {
            currentState = TrapState.Pausing;
            stateTimer = pauseBetweenBites;
            targetVelocity = Vector2.zero;
        }
    }

    void HandlePausing(float distanceToTarget)
    {
        targetVelocity = Vector2.zero;

        if (stateTimer <= 0f)
        {
            if (distanceToTarget <= attackRange)
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
            //AudioEventBus.RequestSFX(new SFXEvent(hurtSFX, volume: 1f, pitch: Random.Range(0.8f, 1.2f), pos: transform.position));
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
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, activationRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, minAttackDistance);
    }

    public RadarObjectType GetObjectType() => RadarObjectType.Artifact;
    public string GetRadarDisplayName() => "HP+";
}