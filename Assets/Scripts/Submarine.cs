using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;

public class Submarine : MonoBehaviour
{
    [Header("Components Needed")]
    [SerializeField] private GameObject enterExitPoint;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject harpoonGun;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Animator subAnimator;
    [SerializeField] private GameObject lightContainer;
    [SerializeField] private Health subHealth;
    [SerializeField] private FlashHit flashHit;
    [SerializeField] private GameObject controlCue; 

    [Header("Submarine Movement")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private MouseAimingSubmarine mouseAiming;
    [SerializeField] private float speed = 5f;

    [Header("Submarine Tipping")]
    [SerializeField] private float maxTipAngle = 15f;
    [SerializeField] private float tipSpeed = 3f;
    [SerializeField] private float tipReturnSpeed = 2f;

    [Header("Underwater Physics")]
    [SerializeField] private float horizontalAcceleration = 1.5f;
    [SerializeField] private float horizontalDeceleration = 0.95f;
    [SerializeField] private float verticalAcceleration = 1.5f;
    [SerializeField] private float verticalDeceleration = 0.95f;
    [SerializeField] private float waterDrag = 0.97f;
    [SerializeField] private float minSpeedForDamage = 3f;
    [SerializeField] private float damageAmount = 10f;

    [Header("Audio")]
    [SerializeField] private AudioClip doorOpenSound;
    [SerializeField] private AudioClip doorCloseSound;


    private PlayerController playerController;
    private SpriteRenderer playerSprite;
    private EnterExitSubmarine ees;
    private MouseAiming playerMouseAiming;
    private ShadowCaster2D playerShadow;
    public bool playerInside = false;
    private float horizontal;
    private float vertical;
    private float currentTipAngle = 0f;
    public bool doorOpen;

    public bool PlayerInside => playerInside;

    public UnityEvent enteredSubmarine;
    public UnityEvent exitedSubmarine;

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        harpoonGun = player.GetComponentInChildren<HarpoonGunAiming>().gameObject;
        playerController = player.GetComponent<PlayerController>();
        playerSprite = player.GetComponent<SpriteRenderer>();
        playerMouseAiming = player.GetComponent<MouseAiming>();
        playerShadow = player.GetComponent<ShadowCaster2D>();
        ees = enterExitPoint.GetComponent<EnterExitSubmarine>();
        rb = this.GetComponent<Rigidbody2D>();
        subHealth = this.GetComponent<Health>();
        speed = 5f;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
    }

    private void Update()
    {
        //if (ees.playerInRange && !playerInside && )
        //    EnterSubmarine();
        //else if (playerInside && Keyboard.current[interactKey].wasPressedThisFrame)
        //    ExitSubmarine();

        if (SerialHandler.Instance.shoot)
        {
            mouseAiming.TriggerShoot(true);
        }

        if (playerController.inSubmarine)
        {
            playerController.transform.position = this.transform.position;
        }
    }

    #region SUBMARINE_CONTROLS
    public void Move(InputAction.CallbackContext context)
    {
        //Vector2 input = context.ReadValue<Vector2>();
        //horizontal = input.x;
        //vertical = input.y;
    }

    public void Fire(InputAction.CallbackContext context)
    {
        //if (context.performed && playerInside)
        //{
        //    mouseAiming.TriggerShoot(true);
        //}
    }

    public void Aim(InputAction.CallbackContext context)
    {
        //if (playerInside)
        //{
        //    mouseAiming.OnAim(context);
        //}
    }
    #endregion

    public void EnterSubmarine()
    {
        if (doorOpen)
        {
            playerInside = true;
            enteredSubmarine?.Invoke();
            playerController.enabled = false;
            playerSprite.enabled = false;
            playerMouseAiming.enabled = false;
            playerShadow.enabled = false;
            harpoonGun.SetActive(false);
            //mouseAiming.enabled = true;
            //Debug.Log("Entered submarine!");
        }
    }

    public void HandleSubmarineDoor()
    {
        if (SerialHandler.Instance.door)
        {
            if (!doorOpen)
            {
                subAnimator.SetBool("door", true);
                doorOpen = true;
                controlCue.SetActive(true);
                AudioEventBus.RequestSFX(new SFXEvent{Clip = doorOpenSound, Volume = 1f, Pitch = 1f});
            }
        } else if (!SerialHandler.Instance.door)
        {
           if (doorOpen)
            {
                subAnimator.SetBool("door", false);
                doorOpen = false;
                controlCue.SetActive(false);
                AudioEventBus.RequestSFX(new SFXEvent{Clip = doorCloseSound, Volume = 1f, Pitch = 1f});
            }

        }
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleTipping();
        HandleSubmarineDoor();
    }

    private void HandleMovement()
    {
        horizontal = -SerialHandler.Instance.joy1X; //change to positive to uninvert controls
        vertical = SerialHandler.Instance.joy1Y;

        float targetVelocityX = horizontal * speed;
        float currentVelocityX = rb.linearVelocity.x;

        if (horizontal != 0 && !doorOpen)
        {
            currentVelocityX = Mathf.Lerp(currentVelocityX, targetVelocityX, horizontalAcceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentVelocityX *= horizontalDeceleration;
        }

        float targetVelocityY = vertical * speed;
        float currentVelocityY = rb.linearVelocity.y;

        if (vertical != 0 && !doorOpen)
        {
            currentVelocityY = Mathf.Lerp(currentVelocityY, targetVelocityY, verticalAcceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentVelocityY *= verticalDeceleration;
        }

        rb.linearVelocity = new Vector2(
            currentVelocityX * waterDrag,
            currentVelocityY * waterDrag
        );

        if (horizontal < 0)
        { //I FLIPPED ALL LOGICAL OPERATORS IN THIS if/elseif STATEMENT APR 24 2026
            spriteRenderer.flipX = false; 
            FlipLightContainer(true);
        }
        else if (horizontal > 0)
        {
            spriteRenderer.flipX = true;
            FlipLightContainer(false);
        }

        subAnimator.SetInteger("horizontal", (int)horizontal);
        subAnimator.SetInteger("vertical", (int)vertical);
    }

    private void HandleTipping()
    {
        float targetTipAngle = 0f;

        if (horizontal != 0)
        {
            targetTipAngle = -horizontal * maxTipAngle;
        }

        float lerpSpeed = (horizontal != 0) ? tipSpeed : tipReturnSpeed;
        currentTipAngle = Mathf.Lerp(currentTipAngle, targetTipAngle, lerpSpeed * Time.fixedDeltaTime);

        transform.rotation = Quaternion.Euler(0f, 0f, currentTipAngle);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (LayerMask.LayerToName(collision.gameObject.layer) == "Terrain")
        {
            float inputMagnitude = new Vector2(horizontal, vertical).magnitude;
            float currentSpeed = inputMagnitude * speed;

            if (currentSpeed >= minSpeedForDamage)
            {
                subHealth.TakeDamage(damageAmount);
                flashHit.TriggerFlash();
                Debug.Log($"Sub hit terrain at speed {currentSpeed:F2}, took {damageAmount} damage");
            }
        }
    }

    public void ExitSubmarine()
    {
        if (!playerInside) return;
        if (doorOpen)
        {
            playerInside = false;
            exitedSubmarine?.Invoke();
            player.transform.position = enterExitPoint.transform.position;
            playerController.enabled = true;
            playerShadow.enabled = true;
            playerSprite.enabled = true;
            playerMouseAiming.enabled = true;
            //mouseAiming.enabled = false;
            harpoonGun.SetActive(true);
            rb.linearVelocity = Vector2.zero;
            currentTipAngle = 0f;
            transform.rotation = Quaternion.identity;
            //Debug.Log("Exited submarine!");
        }
    }

    public void SetMoveSpeed(float newSpeed)
    {
        speed = newSpeed;
    }

    private void FlipLightContainer(bool isFlipped)
    {
        Vector3 scale = lightContainer.transform.localScale;
        scale.x = isFlipped ? 1 : -1;
        lightContainer.transform.localScale = scale;
    }
}