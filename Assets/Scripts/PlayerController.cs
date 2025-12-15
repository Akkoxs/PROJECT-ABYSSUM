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

    [Header("Underwater Physics")]
    [SerializeField] float waterDrag = 0.95f;
    [SerializeField] float horizontalAcceleration = 3f;
    [SerializeField] float horizontalDeceleration = 0.92f;
    [SerializeField] float verticalAcceleration = 3f;
    [SerializeField] float verticalDeceleration = 0.92f;

    [Header("Buoyancy Bobbing")]
    [SerializeField] float bobbingStrength = 0.5f;
    [SerializeField] float bobbingSpeed = 1f;
    [SerializeField] bool enableBobbing = true;

    private float horizontal;
    private float vertical;
    private float bobbingTimer = 0f;

    #region PLAYER_CONTROLS
    public void Move(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        horizontal = input.x;
        vertical = input.y;
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

    private void FixedUpdate()
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

        if (enableBobbing && horizontal == 0 && vertical == 0 && Mathf.Abs(rb.linearVelocity.magnitude) < 0.5f)
        {
            bobbingTimer += Time.fixedDeltaTime * bobbingSpeed;
            float bobbingForce = Mathf.Sin(bobbingTimer) * bobbingStrength;
            rb.AddForce(Vector2.up * bobbingForce, ForceMode2D.Force);
        }
    }
}