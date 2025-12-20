using UnityEngine;

public class Projectile : MonoBehaviour
{
    private Vector3 mousePos;
    private Camera mainCamera;
    private Rigidbody2D rb;
    [SerializeField] private float force;

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

    }

    public void InitializeProjectile(Vector3 mousePos, Camera mainCamera)
    {
        this.mousePos = mousePos;
        this.mainCamera = mainCamera;
    }
}
