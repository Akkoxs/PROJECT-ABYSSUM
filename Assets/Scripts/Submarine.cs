using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class Submarine : MonoBehaviour
{
    [SerializeField] private GameObject enterExitPoint; // Where player spawns when exiting
    [SerializeField] private Key interactKey = Key.E;
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject harpoonGun;

    [Header("Submarine Movement")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private MouseAiming mouseAiming;
    [SerializeField] private float speed = 3f;
    [SerializeField] private PlayerInput submarineInput;
    [SerializeField] private PlayerInput playerInput;

    [Header("Underwater Physics")]
    [SerializeField] private float horizontalAcceleration = 2f;
    [SerializeField] private float horizontalDeceleration = 0.90f;
    [SerializeField] private float verticalAcceleration = 2f;
    [SerializeField] private float verticalDeceleration = 0.90f;
    [SerializeField] private float waterDrag = 0.95f;

    private PlayerController playerController;
    private SpriteRenderer playerSprite;
    private EnterExitSubmarine ees;
    private MouseAiming playerMouseAiming;
    private bool playerInside = false;
    private float horizontal;
    private float vertical;

    //public read only vars
    public bool PlayerInside => playerInside;

    public UnityEvent enteredSubmarine;
    public UnityEvent exitedSubmarine;

    private void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        playerSprite = player.GetComponent<SpriteRenderer>();
        playerMouseAiming = player.GetComponent<MouseAiming>();
        ees = enterExitPoint.GetComponent<EnterExitSubmarine>();
        rb = this.GetComponent<Rigidbody2D>();
        mouseAiming.enabled = false;
        submarineInput.enabled = false;
        rb.linearDamping = 0f;
        rb.angularDamping = 0f;
    }

    private void Update()
    {
        if (ees.playerInRange && !playerInside && Keyboard.current[interactKey].wasPressedThisFrame)
            EnterSubmarine();

        else if (playerInside && Keyboard.current[interactKey].wasPressedThisFrame)
            ExitSubmarine();
    }

    #region SUBMARINE_CONTROLS
    public void Move(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        horizontal = input.x;
        vertical = input.y;
        Debug.Log("submarine moves!!!");
    }

    public void Fire(InputAction.CallbackContext context)
    {
        if (context.performed && playerInside)
        {
            mouseAiming.TriggerShoot(true);
            Debug.Log("submarine shoots!!!");
        }
    }
    #endregion

    public void EnterSubmarine()
    {
        playerInside = true;
        enteredSubmarine?.Invoke();
        playerController.enabled = false;
        playerSprite.enabled = false;
        playerMouseAiming.enabled = false;
        playerInput.enabled = false;
        harpoonGun.SetActive(false);
        submarineInput.enabled = true;
        mouseAiming.enabled = true;

        Debug.Log("Entered submarine!");
    }

    private void FixedUpdate()
    {
        if (playerInside)
        {
            float targetVelocityX = horizontal * speed;
            float currentVelocityX = rb.linearVelocity.x;

            if (horizontal != 0)
            {
                currentVelocityX = Mathf.Lerp(currentVelocityX, targetVelocityX, horizontalAcceleration * Time.fixedDeltaTime);
            }
            else
            {
                currentVelocityX *= horizontalDeceleration;
            }

            float targetVelocityY = vertical * speed;
            float currentVelocityY = rb.linearVelocity.y;

            if (vertical != 0)
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
        }
    }

    public void ExitSubmarine()
    {
        if (!playerInside) return;

        playerInside = false;
        exitedSubmarine?.Invoke();
        player.transform.position = enterExitPoint.transform.position;
        playerController.enabled = true;
        submarineInput.enabled = false;
        playerInput.enabled = true;
        playerSprite.enabled = true;
        playerMouseAiming.enabled = true;
        mouseAiming.enabled = false;
        harpoonGun.SetActive(true);
        rb.linearVelocity = Vector2.zero;
        Debug.Log("Exited submarine!");
    }


}