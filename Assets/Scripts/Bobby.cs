using UnityEngine;

public class Bobby : MonoBehaviour
{
    [Header("Buoyancy Settings")]
    [SerializeField] private float bobbingStrength = 10f;
    [SerializeField] private float bobbingSpeed = 2f;
    [SerializeField] private float verticalDamping = 0.98f; 

    private Rigidbody2D rb;
    private float bobbingTimer = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * verticalDamping);

        if (rb.linearVelocity.magnitude < 0.5f)
        {
            bobbingTimer += Time.fixedDeltaTime * bobbingSpeed;
            float bobbingForce = Mathf.Sin(bobbingTimer) * bobbingStrength;
            rb.AddForce(Vector2.up * bobbingForce, ForceMode2D.Force);
        }
        else
        {
            bobbingTimer = 0f;
        }
    }
}