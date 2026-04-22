using UnityEngine;

public class LeechEnemy : MonoBehaviour, IRadarDetectable
{
    //This is a copy of fishEnemy but the attack params will be different and it will rotate to face player

    [Header("Player Reference")]
    [SerializeField] private Transform playerTransform;
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

    [Header("Audio")]
    [SerializeField] private AudioClip deathSFX;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    // State machine
    private enum LeechState { Patrol, Charging, Retreating, Pausing }
    private LeechState currentState = LeechState.Patrol;
    private float stateTimer = 0f;
    private Health health;
    private Transform subTransform;
    private float patrolDirection;
    private bool isInvulnerable = false;
    private float invulnerabilityTimer = 0f;
    private Vector2 targetVelocity = Vector2.zero;

    void Start()
    {
        patrolDirection = initialDirection;
        health = GetComponent<Health>();

        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
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

        if(health.CurrentHealth <= 0)
        {
            AudioEventBus.RequestSFX(new SFXEvent(deathSFX, volume: 1f, pitch: Random.Range(0.9f, 1.1f), pos: transform.position));
            Destroy(gameObject);
            return;
        }

        if (subTransform == null)
        {
            GameObject submarine = GameObject.FindGameObjectWithTag("Submarine");
            if (submarine != null)
            {
                subTransform = submarine.transform;
            }
        }

        UpdateMainTarget();

        if (mainTransform == null)
        {
            currentState = LeechState.Patrol;
            return;
        }

        if (isInvulnerable)
        {
            invulnerabilityTimer -= Time.deltaTime;
            if (invulnerabilityTimer <= 0f)
                isInvulnerable = false;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, mainTransform.position);

        switch (currentState)
        {
            case LeechState.Patrol:
                HandlePatrol(distanceToPlayer);
                break;

            case LeechState.Charging:
                HandleCharging(distanceToPlayer);
                break;

            case LeechState.Retreating:
                HandleRetreating();
                break;

            case LeechState.Pausing:
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
            mainTransform = distToPlayer < distToSub ? playerTransform : subTransform; // both in range, pick closer
        else if (playerInRange)
            mainTransform = playerTransform;
        else if (subInRange)
            mainTransform = subTransform;
        //neither in range - leave mainTransform unchanged so enemy finishes current chase
    }

    void HandlePatrol(float distanceToPlayer)
    {
        targetVelocity = new Vector2(patrolDirection * patrolSpeed, 0f);

        //reset rotation, restore flipX for horizontal patrol
        transform.rotation = Quaternion.identity;
        if (spriteRenderer != null)
            spriteRenderer.flipX = patrolDirection > 0;

        if (distanceToPlayer <= detectionRange)
        {
            currentState = LeechState.Charging;
            stateTimer = chargeDuration;
        }
    }

    void HandleCharging(float distanceToPlayer)
    {
        Vector2 directionToPlayer = (mainTransform.position - transform.position).normalized;
        targetVelocity = directionToPlayer * chargeSpeed;

        RotateTowardDirection(directionToPlayer); // replaces spriteRenderer.flipX

        if (distanceToPlayer <= minAttackDistance || stateTimer <= 0f)
        {
            currentState = LeechState.Retreating;
            stateTimer = retreatDuration;
        }

        if (distanceToPlayer > detectionRange)
        {
            currentState = LeechState.Patrol;
        }
    }

    void HandleRetreating()
    {
        Vector2 directionFromPlayer = (transform.position - mainTransform.position).normalized;
        targetVelocity = directionFromPlayer * retreatSpeed;

        if (stateTimer <= 0f)
        {
            currentState = LeechState.Pausing;
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
                currentState = LeechState.Charging;
                stateTimer = chargeDuration;
            }
            else
            {
                currentState = LeechState.Patrol;
            }
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Projectile") && !isInvulnerable)
        {
            isInvulnerable = true;
            invulnerabilityTimer = invulnerabilityDuration;
            currentState = LeechState.Patrol;
        }

        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();

        if (damageable != null)
        {
            damageable.TakeDamage(damageAmount);
        }

        if (collision.gameObject.layer == LayerMask.NameToLayer("Terrain") || collision.gameObject.layer == LayerMask.NameToLayer("Floor"))
        {
            patrolDirection *= -1f;
        }
    }

    void RotateTowardDirection(Vector2 direction)
    {
        if (direction == Vector2.zero) return;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }    

    void Die()
    {
        Destroy(gameObject);
    }

    public RadarObjectType GetObjectType()
    {
        return RadarObjectType.Fauna;
    }

    public string GetRadarDisplayName()
    {
        return null;
    }

}