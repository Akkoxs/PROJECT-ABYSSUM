using UnityEngine;

public class Shooter : MonoBehaviour
{
    [Header("Projectile and Target")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform target;

    [Header("Speed Values")]
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float shootRate;

    [Header("Camera Reference")]
    [SerializeField] private Camera mainCamera;

    private float shootTime;
    private bool canShoot;
    private void Start()
    {
        canShoot = false;
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        target.position = mousePos;

        if (canShoot)
        {
            Projectile projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity).GetComponent<Projectile>();
            canShoot = false;
        }
    }

    public void TriggerShot()
    {
        canShoot = true;
    }
}
