using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseAimingSubmarine : MonoBehaviour
{
    [Header("Aiming Settings")]
    [SerializeField] private Transform reticle;
    [SerializeField] private float aimRadius = 3f;
    [SerializeField] private float stickDeadzone = 0.2f; // Minimum stick input to register

    [Header("Camera Reference")]
    [SerializeField] private Camera mainCamera;

    [Header("Shooter Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileTransform;
    [SerializeField] private float timeBetweenFiring;

    [Header("Ammo System")]
    [SerializeField] private int maxAmmo = 3;
    [SerializeField] private float reloadTime = 3f;

    private float timer;
    private bool canFire;
    private bool shoot;
    private Vector3 aimPosition;
    private Vector2 rightStickInput;
    private Vector2 currentAimDirection;

    private int currentAmmo;
    private bool isReloading = false;
    private float reloadTimer = 0f;

    void Start()
    {
        Cursor.visible = false;
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        currentAimDirection = Vector2.right;

        currentAmmo = maxAmmo;
        canFire = true;
    }

    void Update()
    {
        UpdateAimPosition();
        UpdateReticlePosition();
        HandleShooting();
    }

    void UpdateAimPosition()
    {
        if (rightStickInput.magnitude > stickDeadzone)
        {
            currentAimDirection = rightStickInput.normalized;
        }

        aimPosition = transform.position + (Vector3)currentAimDirection * aimRadius;
    }

    void UpdateReticlePosition()
    {
        if (reticle != null)
        {
            reticle.position = aimPosition;
        }
    }

    void HandleShooting()
    {
        if (isReloading)
        {
            reloadTimer += Time.deltaTime;
            if (reloadTimer >= reloadTime)
            {

                currentAmmo = maxAmmo;
                isReloading = false;
                reloadTimer = 0f;
                canFire = true;
                Debug.Log("Reload complete! Ammo: " + currentAmmo);
            }
            return;
        }

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
            if (currentAmmo > 0)
            {
                Torpedo projectile = Instantiate(projectilePrefab, projectileTransform.position, Quaternion.identity).GetComponent<Torpedo>();
                projectile.InitializeProjectile(currentAimDirection); // Changed this line
                currentAmmo--;

                Debug.Log("Torpedo fired! Ammo remaining: " + currentAmmo);

                shoot = false;
                canFire = false;

                if (currentAmmo <= 0)
                {
                    isReloading = true;
                    reloadTimer = 0f;
                    Debug.Log("Out of ammo! Reloading...");
                }
            }
            else
            {
                shoot = false;
                isReloading = true;
                reloadTimer = 0f;
            }
        }
        else if (!canFire && shoot)
        {
            shoot = false;
        }
    }

    public void OnAim(InputAction.CallbackContext context)
    {
        rightStickInput = context.ReadValue<Vector2>();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, aimRadius);
    }

    public Vector3 GetMousePos()
    {
        return aimPosition;
    }

    public void TriggerShoot(bool shoot)
    {
        this.shoot = shoot;
    }

    public void SetReloadSpeed(float newReloadSpeed)
    {
        timeBetweenFiring = newReloadSpeed;
    }

    public int GetCurrentAmmo()
    {
        return currentAmmo;
    }

    public int GetMaxAmmo()
    {
        return maxAmmo;
    }

    public bool IsReloading()
    {
        return isReloading;
    }

    public float GetReloadProgress()
    {
        if (!isReloading) return 1f;
        return reloadTimer / reloadTime;
    }
}