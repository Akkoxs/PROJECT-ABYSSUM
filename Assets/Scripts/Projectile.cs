using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector2 launchDirection;
    private Rigidbody2D rb;
    private bool isMoving;
    private bool isStuck = false;

    [Header("Projectile Settings")]
    [SerializeField] private float force;
    [SerializeField] private float minVelocity;
    [SerializeField] private float waterDrag;
    [SerializeField] private Animator harpoonAnimator;
    [SerializeField] private float harpDamage = 10f;

    [Header("Collision Settings")]
    [SerializeField] private LayerMask floorLayer;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private float tipOffset;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (isMoving && !isStuck)
        {
            Vector2 tipPosition = (Vector2)transform.position + (Vector2)transform.right * tipOffset;
            Collider2D hitCollider = Physics2D.OverlapCircle(tipPosition, 0.1f, floorLayer);
            if (hitCollider != null)
            {
                StickToSurface();
                return;
            }

            float currentSpeed = rb.linearVelocity.magnitude;
            float newSpeed = currentSpeed - (waterDrag * Time.fixedDeltaTime);

            if (newSpeed <= minVelocity)
            {
                rb.linearVelocity = Vector2.zero;
                isMoving = false;
                Destroy(gameObject, 1f);
            }
            else
            {
                rb.linearVelocity = rb.linearVelocity.normalized * newSpeed;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.TryGetComponent<IDamageable>(out var damageInterface))
        {
            damageInterface.TakeDamage(harpDamage);
            Destroy(gameObject, 0.2f);
        }
    }

    private void StickToSurface()
    {
        isStuck = true;
        isMoving = false;
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;
        Destroy(gameObject, 1f);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Vector2 tipPosition = (Vector2)transform.position + (Vector2)transform.right * tipOffset;
        Gizmos.DrawWireSphere(tipPosition, 0.1f);
    }

    public void InitializeProjectile(Vector2 direction)
    {
        launchDirection = direction.normalized;
        isMoving = true;

        if (rb == null) rb = GetComponent<Rigidbody2D>();

        rb.linearVelocity = launchDirection * force;

        float angle = Mathf.Atan2(launchDirection.y, launchDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        if (harpoonAnimator != null)
        {
            harpoonAnimator.SetBool("fly", true);
        }
    }

    public void SetHarpDamage(float newDamage)
    {
        harpDamage = newDamage;
    }

    public void SetHarpForce(float newForce)
    {
        force = newForce;
    }
}