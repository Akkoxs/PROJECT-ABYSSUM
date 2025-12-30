using System;
using UnityEngine;

public class MouseAiming : MonoBehaviour
{
    [Header("Aiming Settings")]
    [SerializeField] private Transform reticle;
    [SerializeField] private float aimRadius = 3f;
    [SerializeField] private Animator harpoonAnimator;
    [SerializeField] private bool needHarpoonAnimator;

    [Header("Camera Reference")]
    [SerializeField] private Camera mainCamera;

    [Header("Shooter Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileTransform;
    [SerializeField] private float timeBetweenFiring;
    private float timer;
    private bool canFire;
    private bool shoot;
    private Vector3 mousePos;

    void Start()
    {
        Cursor.visible = false;

        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    void Update()
    {
        mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector2 direction = (mousePos - transform.position).normalized;
        Vector2 reticlePos = (Vector2)transform.position + (direction * aimRadius);
        if (reticle != null) { reticle.position = reticlePos; }

        if (!canFire)
        {
            timer += Time.deltaTime;
            if (timer > timeBetweenFiring)
            {
                canFire = true;
                timer = 0;
            }
        }

        if (canFire && shoot)
        {
            Projectile projectile = Instantiate(projectilePrefab, projectileTransform.position, Quaternion.identity).GetComponent<Projectile>();
            projectile.InitializeProjectile(mousePos, mainCamera);

            try
            {
                if (needHarpoonAnimator) harpoonAnimator.SetTrigger("shoot");
            }
            catch (NullReferenceException e)
            {
                Debug.Log("DONT NEED HARPOON GUN ANIMATOR ON SUBMARINE");
            }


            shoot = false;
            canFire = false;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aimRadius);
    }

    public Vector3 GetMousePos()
    {
        return mousePos;
    }

    public void TriggerShoot(bool shoot)
    {
        this.shoot = shoot;
    }
}
