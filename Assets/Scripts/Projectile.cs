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
        transform.rotation = Quaternion.Euler(0, 0, rot + 180);
    }

    void Update()
    {
        if (isMoving && !isStuck)
        {
            Vector2 tipPosition = (Vector2)transform.position + (Vector2)transform.right * tipOffset;
            Collider2D hitCollider = Physics2D.OverlapCircle(tipPosition, 0.1f, floorLayer);
            Collider2D hitColliderEnemy = Physics2D.OverlapCircle(tipPosition, 0.1f, enemyLayer);

            if (hitCollider != null)
            {
                StickToSurface();
                return;
            }
            
            //if (hitColliderEnemy != null)
            //{
            //    Debug.Log("here in hit collider enemy: " + hitColliderEnemy);
            //    Destroy(this.gameObject, 0f);
            //    return;
            //}

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

    private void StickToSurface()
    {
        isStuck = true;
        isMoving = false;

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;
        rb.bodyType = RigidbodyType2D.Kinematic;

        Destroy(gameObject, 1f);
    }

    private void StickToEnemy()
    {
        //todo make stick to enemy so it's not all fucky.
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
}
