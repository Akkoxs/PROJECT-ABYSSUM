using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Player Component Reference")]
    [SerializeField] Rigidbody2D rb;

    [Header("Player Settings")]
    [SerializeField] float speed;
    [SerializeField] float jumpingPower;

    [Header("Grounding")]
    [SerializeField] LayerMask groundLayer;
    [SerializeField] Transform groundCheck;

    private float horizontal;

    #region PLAYER_CONTROLS
    public void Move(InputAction.CallbackContext context)
    {
        horizontal = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpingPower);
        }
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCapsule(groundCheck.position, new Vector2(1f, 0.1f), CapsuleDirection2D.Horizontal, 0, groundLayer);
    }
    #endregion


    [Header("Underwater Physics")]
    [SerializeField] float jumpDrag = 0.95f; // floatiness when jumping
    [SerializeField] float horizontalAcceleration = 1.5f; // acceleration float
    [SerializeField] float horizontalDeceleration = 0.92f; // deceleration to cause a bit of slide
    [SerializeField] float buoyancy = 2f; // tried this out but idk if it works it kinda conflicts with the jumpdrag

    private void FixedUpdate()
    {
        float targetVelocityX = horizontal * speed;
        float currentVelocityX = rb.linearVelocity.x;

        //horizontal acceleration, decelerate if no input is detected
        if (horizontal != 0)
        {
            currentVelocityX = Mathf.Lerp(currentVelocityX, targetVelocityX, horizontalAcceleration * Time.fixedDeltaTime);
        }
        else
        {
            currentVelocityX *= horizontalDeceleration;
        }

        rb.linearVelocity = new Vector2(currentVelocityX, rb.linearVelocity.y);

        //jump resistance
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            rb.linearVelocity.y * jumpDrag
        );

        //wanted to make the player bounce a bit then fall when jumping like ur buoyant but idk doesn't really work
        if (!IsGrounded())
        {
            rb.AddForce(Vector2.up * buoyancy);
        }
    }
}