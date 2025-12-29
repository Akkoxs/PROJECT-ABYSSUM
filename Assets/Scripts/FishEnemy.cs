using UnityEngine;

public class FishEnemy : MonoBehaviour
{
    [Header("Player Reference")]
    [SerializeField] private Transform playerTransform;
    
    [Header("Follow Settings")]
    [SerializeField] private float followSpeed = 3f;
    [SerializeField] private float maxFollowDistance = 5f;
    [SerializeField] private float minFollowDistance = 0.5f;
    
    [Header("Patrol Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float initialDirection = 1f;
    
    [Header("Combat")]
    [SerializeField] private float damageAmount = 10f;
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float knockbackDuration = 0.5f;
    
    [Header("Options")]
    [SerializeField] private bool smoothMovement = true;
    
    private SpriteRenderer spriteRenderer;
    private bool isFollowing = false;
    private bool isKnockedBack = false;
    private float knockbackTimer = 0f;
    private Vector2 knockbackVelocity = Vector2.zero;
    public float patrolDirection;
    private Rigidbody2D rb;
    
    void Start()
    {
        patrolDirection = initialDirection;
        
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            rb.linearDamping = 0f; // NO DRAG - let knockback work
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
            else
            {
                Debug.LogWarning("FishEnemy: No player found! Make sure player has 'Player' tag or assign manually.");
            }
        }
    }
    
    void FixedUpdate()
    {
        // Handle knockback in FixedUpdate for physics
        if (isKnockedBack)
        {
            // Force the knockback velocity every physics frame
            rb.linearVelocity = knockbackVelocity;
            return;
        }
    }
    
    void Update()
    {
        // Count down knockback timer
        if (isKnockedBack)
        {
            knockbackTimer -= Time.deltaTime;
            if (knockbackTimer <= 0f)
            {
                isKnockedBack = false;
                knockbackVelocity = Vector2.zero;
                rb.linearVelocity = Vector2.zero;
            }
            return; // Don't do anything else
        }
        
        // Return early if no player reference
        if (playerTransform == null)
        {
            Patrol();
            return;
        }
        
        float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
        
        // Check if should follow or patrol
        if (distanceToPlayer <= maxFollowDistance && distanceToPlayer > minFollowDistance)
        {
            if (!isFollowing)
            {
                isFollowing = true;
                rb.linearVelocity = Vector2.zero;
            }
            FollowPlayer();
        }
        else
        {
            if (isFollowing)
            {
                isFollowing = false;
                if (playerTransform.position.x > transform.position.x)
                {
                    patrolDirection = 1f;
                }
                else
                {
                    patrolDirection = -1f;
                }
            }
            Patrol();
        }
    }
    
    void FollowPlayer()
    {
        // Stop velocity so transform movement works
        rb.linearVelocity = Vector2.zero;
        
        // Flip sprite
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = playerTransform.position.x < transform.position.x;
        }
        
        if (smoothMovement)
        {
            transform.position = Vector3.Lerp(
                transform.position, 
                playerTransform.position, 
                followSpeed * Time.deltaTime
            );
        }
        else
        {
            transform.position = Vector3.MoveTowards(
                transform.position, 
                playerTransform.position, 
                followSpeed * Time.deltaTime
            );
        }
    }
    
    void Patrol()
    {
        rb.linearVelocity = new Vector2(patrolDirection * patrolSpeed, 0f);
        
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = patrolDirection < 0;
        }
    }
    
    void OnCollisionEnter2D(Collision2D collision)
    {
        Debug.Log($"Fish collided with: {collision.gameObject.name}");
        
        if (isKnockedBack)
        {
            Debug.Log("Already knocked back");
            return;
        }
        
        IDamageable damageable = collision.gameObject.GetComponent<IDamageable>();
        
        if (damageable != null)
        {
            Debug.Log("Dealing damage and applying knockback!");
            
            damageable.TakeDamage(damageAmount);
            
            // Get knockback direction from collision
            Vector2 contactPoint = collision.contacts[0].point;
            Vector2 knockbackDirection = ((Vector2)transform.position - contactPoint).normalized;
            
            // Store knockback velocity
            knockbackVelocity = knockbackDirection * knockbackForce;
            
            // Start knockback
            isKnockedBack = true;
            knockbackTimer = knockbackDuration;
            
            Debug.Log($"Knockback started! Direction: {knockbackDirection}, Velocity: {knockbackVelocity}, Duration: {knockbackDuration}");
        }
    }
   
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, maxFollowDistance);
    }
}