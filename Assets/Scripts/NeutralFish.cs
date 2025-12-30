using UnityEngine;

public class NeutralFish : MonoBehaviour
{
    [Header("Patrol Settings")]
    [SerializeField] private float patrolSpeed = 2f;
    [SerializeField] private float initialDirection = 1f; // 1 for right, -1 for left
    
    public float patrolDirection; // Made public so BetweenObjects can access it
    private Rigidbody2D rb;
    
    void Start()
    {
        patrolDirection = initialDirection;
        
        rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }
    }
    
    void Update()
    {
        Patrol();
    }
    
    void Patrol()
    {
        // Move continuously in the patrol direction
        rb.linearVelocity = new Vector2(patrolDirection * patrolSpeed, 0f);
    }
}