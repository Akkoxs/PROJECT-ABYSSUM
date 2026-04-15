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
    [SerializeField] private PlayerController playerController;

    [Header("Audio")]
    [SerializeField] private AudioClip shootSFX;
    [SerializeField] private AudioClip shootSFX2;
    [SerializeField] private AudioClip shootSFX3;

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
            // Mirror only X for Pepper's Ghost horizontal reflection
            currentAimDirection = new Vector2(-rightStickInput.x, rightStickInput.y).normalized;
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
            AudioEventBus.RequestSFX(new SFXEvent(shootSFX, volume: 0.5f));
            AudioEventBus.RequestSFX(new SFXEvent(shootSFX2, volume: 0.5f));
            AudioEventBus.RequestSFX(new SFXEvent(shootSFX3, volume: 0.5f));
            projectile.InitializeProjectile(currentAimDirection);
            if (playerController != null)
                playerController.ApplyKnockback(currentAimDirection);

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