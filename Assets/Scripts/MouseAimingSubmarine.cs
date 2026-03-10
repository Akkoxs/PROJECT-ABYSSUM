using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseAimingSubmarine : MonoBehaviour
{
    [Header("Aiming Settings")]
    [SerializeField] private Transform reticle;
    [SerializeField] private float aimRadius = 3f;

    [Header("Gamepad Settings")]
    [SerializeField] private float stickDeadzone = 0.2f; // Minimum stick input to register

    [Header("Camera Reference")]
    [SerializeField] private Camera mainCamera;

    [Header("Shooter Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileTransform;
    [SerializeField] private float timeBetweenFiring;

    [Header("Ammo System")]
    [SerializeField] private int maxAmmo = 3; // Maximum torpedoes before reload
    [SerializeField] private float reloadTime = 3f; // Time to reload all ammo

    private float timer;
    private bool canFire;
    private bool shoot;
    private Vector3 aimPosition; // Used for gamepad aiming
    private Vector2 rightStickInput;
    private Vector2 currentAimDirection; // Persistent aim direction for gamepad

    // Ammo tracking
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

        // Initialize aim direction (default to right)
        currentAimDirection = Vector2.right;

        // Start with full ammo
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
        // Check if gamepad right stick is being used
        if (rightStickInput.magnitude > stickDeadzone)
        {
            // Update aim direction based on stick input
            currentAimDirection = rightStickInput.normalized;
        }
        // If stick is neutral, keep the last aimed direction

        // Calculate aim position on the circle
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
        // Handle reloading
        if (isReloading)
        {
            reloadTimer += Time.deltaTime;
            if (reloadTimer >= reloadTime)
            {
                // Reload complete
                currentAmmo = maxAmmo;
                isReloading = false;
                reloadTimer = 0f;
                canFire = true;
                Debug.Log("Reload complete! Ammo: " + currentAmmo);
            }
            return; // Don't allow firing while reloading
        }

        // Handle fire rate delay
        if (!canFire)
        {
            timer += Time.deltaTime;
            if (timer > timeBetweenFiring)
            {
                canFire = true;
                timer = 0;
            }
        }

        // Attempt to fire
        if (canFire && shoot)
        {
            if (currentAmmo > 0)
            {
                // Fire torpedo
                Torpedo projectile = Instantiate(projectilePrefab, projectileTransform.position, Quaternion.identity).GetComponent<Torpedo>();
                projectile.InitializeProjectile(currentAimDirection); // Changed this line
                currentAmmo--;

                Debug.Log("Torpedo fired! Ammo remaining: " + currentAmmo);

                shoot = false;
                canFire = false;

                // Start reload if out of ammo
                if (currentAmmo <= 0)
                {
                    isReloading = true;
                    reloadTimer = 0f;
                    Debug.Log("Out of ammo! Reloading...");
                }
            }
            else
            {
                // Out of ammo but somehow not reloading (safety check)
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

    // Called by Unity Input System
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

    // Public getters for UI or other systems
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