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
    [SerializeField] private float stickDeadzone = 0.2f;

    [Header("Camera Reference")]
    [SerializeField] private Camera mainCamera;

    [Header("Shooter Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileTransform;
    [SerializeField] private float timeBetweenFiring;

    private float timer;
    private bool canFire;
    private bool shoot;
    private Vector3 aimPosition;
    private Vector2 rightStickInput;
    private Vector2 currentAimDirection;

    void Start()
    {
        Cursor.visible = false;
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }

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
            projectile.InitializeProjectile(currentAimDirection);

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