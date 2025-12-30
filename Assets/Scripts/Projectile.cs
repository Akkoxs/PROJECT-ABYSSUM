using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 mousePos;
    private Camera mainCamera;
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
        Vector3 direction = mousePos - transform.position;
        Vector3 rotation = transform.position - mousePos;
        rb.linearVelocity = new Vector2(direction.x, direction.y).normalized * force;
        float rot = Mathf.Atan2(rotation.y, rotation.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, rot);
        harpoonAnimator.SetBool("fly", true);
    }

    void Update()
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
            float newSpeed = currentSpeed - (waterDrag * Time.deltaTime);

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

    public void InitializeProjectile(Vector3 mousePos, Camera mainCamera)
    {
        this.mousePos = mousePos;
        this.mainCamera = mainCamera;
        isMoving = true;
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
