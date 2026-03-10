using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class MouseAiming : MonoBehaviour
{
    [Header("Aiming Settings")]
    [SerializeField] private Transform reticle;
    [SerializeField] private float aimRadius = 3f;
    [SerializeField] private Animator harpoonAnimator;
    [SerializeField] private bool needHarpoonAnimator;

    [Header("Gamepad Settings")]
    [SerializeField] private float stickDeadzone = 0.2f; // Minimum stick input to register

    [Header("Camera Reference")]
    [SerializeField] private Camera mainCamera;

    [Header("Shooter Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileTransform;
    [SerializeField] private float timeBetweenFiring;

    private float timer;
    private bool canFire;
    private bool shoot;
    private Vector3 aimPosition; // Used for both mouse and gamepad
    private Vector2 rightStickInput;
    private Vector2 currentAimDirection; // Persistent aim direction for gamepad

    void Start()
    {
        Cursor.visible = false;
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

        // Initialize aim direction (default to right)
        currentAimDirection = Vector2.right;
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
            projectile.InitializeProjectile(currentAimDirection); // Pass direction instead

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
}