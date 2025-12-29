using UnityEngine;

public class EnemyGetsHit : MonoBehaviour
{
    [Header("Knockback Settings")]
    [SerializeField] private float knockbackForce;
    [SerializeField] private string projectileTag = "Projectile";

    [Header("Underwater Deceleration")]
    //[SerializeField] private float decelerationRate;
    [SerializeField] private float minVelocity;
    [SerializeField] private float waterDrag; 

    private Rigidbody2D rb;
    private bool isBeingKnockedBack = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    //void FixedUpdate()
    //{
    //    if (isBeingKnockedBack)
    //    {
    //        float currentSpeed = rb.linearVelocity.magnitude;
    //        float newSpeed = currentSpeed - (decelerationRate * Time.fixedDeltaTime);

    //        if (newSpeed <= minVelocity)
    //        {
    //            rb.linearVelocity = Vector2.zero; 
    //            isBeingKnockedBack = false;
    //        }
    //        else
    //        {
    //            rb.linearVelocity = rb.linearVelocity.normalized * newSpeed;
    //        }
    //    }
    //}

    void FixedUpdate()
    {
        if (isBeingKnockedBack)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x * waterDrag,
                rb.linearVelocity.y * waterDrag
            );

            float currentSpeed = rb.linearVelocity.magnitude;
            if (currentSpeed <= minVelocity)
            {
                rb.linearVelocity = Vector2.zero; 
                isBeingKnockedBack = false;
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(projectileTag))
        {
            Vector2 knockbackDir = (transform.position - collision.transform.position).normalized;
            rb.linearVelocity = knockbackDir * knockbackForce;
            isBeingKnockedBack = true;
            Destroy(collision.gameObject);
        }
    }
}