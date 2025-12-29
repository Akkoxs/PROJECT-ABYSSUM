using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class Submarine : MonoBehaviour
{
    [SerializeField] private GameObject enterExitPoint; // Where player spawns when exiting
    [SerializeField] private Key interactKey = Key.E;
    [SerializeField] private GameObject player;

    [Header("Submarine Movement")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private float moveSpeed = 3f;

    [Header("Underwater Physics")]
    [SerializeField] private float horizontalAcceleration = 2f;
    [SerializeField] private float horizontalDeceleration = 0.90f;
    [SerializeField] private float verticalAcceleration = 2f;
    [SerializeField] private float verticalDeceleration = 0.90f;
    [SerializeField] private float waterDrag = 0.95f; 

    private PlayerController playerController;
    private SpriteRenderer playerSprite;
    private EnterExitSubmarine ees;
    private bool playerInside = false;
    private float horizontalInput = 0f;
    private float verticalInput = 0f;

    //public read only vars
    public bool PlayerInside => playerInside;

    public UnityEvent enteredSubmarine;
    public UnityEvent exitedSubmarine;
    
    private void Start()
    {
        playerController = player.GetComponent<PlayerController>();
        playerSprite = player.GetComponent<SpriteRenderer>();
        ees = enterExitPoint.GetComponent<EnterExitSubmarine>();
        rb = this.GetComponent<Rigidbody2D>();

        rb.linearDamping = 0f; 
        rb.angularDamping = 0f;
    }

    private void Update()
    {
        if (ees.playerInRange && !playerInside && Keyboard.current[interactKey].wasPressedThisFrame)
            EnterSubmarine();

        else if (playerInside && Keyboard.current[interactKey].wasPressedThisFrame)
            ExitSubmarine();

        //if (playerInside)
        //{
        //    float vertical = 0f; 

        //    if (Keyboard.current.wKey.isPressed)
        //        vertical = 1f;

        //    else if (Keyboard.current.sKey.isPressed)
        //        vertical = -1f;

        //    transform.Translate(Vector2.up * vertical * moveSpeed * Time.deltaTime);
        //}

        if (playerInside)
        {
            horizontalInput = 0f;
            if (Keyboard.current.aKey.isPressed)
                horizontalInput = -1f;

            else if (Keyboard.current.dKey.isPressed)
                horizontalInput = 1f;

            verticalInput = 0f;
            if (Keyboard.current.wKey.isPressed)
                verticalInput = 1f;

            else if (Keyboard.current.sKey.isPressed)
                verticalInput = -1f;
        }
    }
    
    public void EnterSubmarine()
    {        
        playerInside = true;
        enteredSubmarine?.Invoke();
        playerController.enabled = false;
        playerSprite.enabled = false;
        Debug.Log("Entered submarine!");
    }

    private void FixedUpdate()
    {
        if (playerInside)
        {
            float targetVelocityX = horizontalInput * moveSpeed;
            float currentVelocityX = rb.linearVelocity.x;

            if (horizontalInput != 0)
            {
                currentVelocityX = Mathf.Lerp(currentVelocityX, targetVelocityX, horizontalAcceleration * Time.fixedDeltaTime);
            }
            else
            {
                currentVelocityX *= horizontalDeceleration;
            }

            float targetVelocityY = verticalInput * moveSpeed;
            float currentVelocityY = rb.linearVelocity.y;

            if (verticalInput != 0)
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
        playerSprite.enabled = true;
        rb.linearVelocity = Vector2.zero;
        Debug.Log("Exited submarine!");
    }


}