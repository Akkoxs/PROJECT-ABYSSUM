using UnityEngine;

public class TrapEnemy : MonoBehaviour, IRadarDetectable
{
    [Header("General")]
    [SerializeField] private GameObject player;

    [Header("Combat")]
    [SerializeField] private float enemyDamage = 10f;

    [Header("Detection Settings")]
    [SerializeField] private float triggerDistance = 2f;
    [SerializeField] private float detectionCheckInterval = 0.1f;

    [Header("Transform Settings")]
    [SerializeField] private float transformDuration = 1f;

    [Header("Animation Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private string transformTrigger = "Transform";
    [SerializeField] private string attackTrigger = "Attack";

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 10f;

    [Header("Attack Cooldown")]
    [SerializeField] private float hitCooldown = 1f;

    private Transform playerTransform;
    private float detectionTimer = 0f;
    private float stateTimer = 0f;

    private enum EnemyState { Idle, Transforming, Attacking, HitCooldown }
    public enum AnimState {Idle, Transforming, Swimming, Biting,}

    private EnemyState currentState = EnemyState.Idle;
    public AnimState currentAnimState = AnimState.Idle;

    private void Start()
    {
        if (player != null)
        {
            playerTransform = player.transform;
        }
        
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Update()
    {
        if (playerTransform == null) return;

        switch (currentState)
        {
            case EnemyState.Idle:
                CheckForPlayer();
                break;
            case EnemyState.Transforming:
                HandleTransform();
                break;
            case EnemyState.Attacking:
                AttackPlayer();
                break;
            case EnemyState.HitCooldown:
                HandleCooldown();
                break;
        }
    }

    private void CheckForPlayer()
    {
        detectionTimer += Time.deltaTime;
        if (detectionTimer < detectionCheckInterval) return;
        detectionTimer = 0f;

        float distance = Vector2.Distance(transform.position, playerTransform.position);

        if (distance <= triggerDistance)
        {
            StartTransform();
        }
    }

    private void StartTransform()
    {
        currentState = EnemyState.Transforming;
        stateTimer = 0f;
        
        if (animator != null)
        {
            animator.SetTrigger(transformTrigger);
        }
    }

    private void HandleTransform()
    {
        stateTimer += Time.deltaTime;

        if (stateTimer >= transformDuration)
        {
            StartAttack();
        }
    }

    private void StartAttack()
    {
        currentState = EnemyState.Attacking;
        
        if (animator != null)
        {
            animator.SetTrigger(attackTrigger);
        }
    }

    private void AttackPlayer()
    {
        Vector2 direction = (playerTransform.position - transform.position).normalized;
        transform.position += (Vector3)direction * moveSpeed * Time.deltaTime;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (currentState != EnemyState.Attacking) return;

        if (collision.collider.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(enemyDamage);
            StartCooldown();
        }
    }

    private void StartCooldown()
    {
        currentState = EnemyState.HitCooldown;
        stateTimer = 0f;
    }

    private void HandleCooldown()
    {
        stateTimer += Time.deltaTime;

        if (stateTimer >= hitCooldown)
        {
            StartAttack();
        }
    }

    public RadarObjectType GetObjectType()
    {
        return RadarObjectType.Artifact;
    }

    public string GetRadarDisplayName()
    {
        return null;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, triggerDistance);
    }
}