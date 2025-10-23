using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runMultiplier = 1.75f;

    [Header("Dash Settings")]
    public float dashSpeed = 12f;         
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public float gravity = 9.8f;
    private float zPosition = 0f;
    private float verticalVelocity = 0f;

    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private PlayerControls controls;

    private Vector2 moveInput;
    private bool isRunning;

    private bool canDash = true;

    private Vector2 lastDirection = Vector2.up;
    private Color baseSpriteColor;

    private enum PlayerState { Normal, Dashing, Jumping }
    private PlayerState currentState = PlayerState.Normal;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null) baseSpriteColor = spriteRenderer.color;
        else baseSpriteColor = Color.white;
        if (rb != null) rb.freezeRotation = true;

        controls = new PlayerControls();

        // Movement input
        controls.Player.Move.performed += ctx =>
        {
            moveInput = ctx.ReadValue<Vector2>();
            if (moveInput.sqrMagnitude > 0.01f)
                lastDirection = moveInput.normalized;
        };
        controls.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        // Run input
        controls.Player.Run.performed += ctx => isRunning = true;
        controls.Player.Run.canceled += ctx => isRunning = false;

        // Dash input
        controls.Player.Dash.performed += ctx =>
        {
            if (canDash && currentState == PlayerState.Normal)
            {
                Vector2 dashDir = moveInput.sqrMagnitude > 0.01f ? moveInput.normalized : lastDirection;
                spriteRenderer.flipX = dashDir.x < -0.01f;
                animator.SetFloat("moveX", dashDir.x);
                animator.SetFloat("moveY", dashDir.y);
                StartCoroutine(Dash(dashDir));
            }
        };

        // Jump input
        controls.Player.Jump.performed += ctx =>
        {
            if (currentState == PlayerState.Normal && zPosition <= 0f)
            {
                verticalVelocity = jumpForce;
                animator.SetTrigger("jump");
                currentState = PlayerState.Jumping;
            }
        };
    }

    private void OnEnable() => controls?.Enable();
    private void OnDisable() => controls?.Disable();

    private void Update()
    {
        verticalVelocity -= gravity * Time.deltaTime;
        zPosition += verticalVelocity * Time.deltaTime;
        if (zPosition < 0f)
        {
            zPosition = 0f;
            verticalVelocity = 0f;
            if (currentState == PlayerState.Jumping) currentState = PlayerState.Normal;
        }

        if (currentState == PlayerState.Normal)
        {
            if (moveInput.x > 0.01f) spriteRenderer.flipX = false;
            else if (moveInput.x < -0.01f) spriteRenderer.flipX = true;

            bool isMoving = moveInput.sqrMagnitude > 0.01f;
            animator.SetBool("iswalking", isMoving && !isRunning);
            animator.SetBool("isrunning", isMoving && isRunning);
            if (isMoving)
            {
                animator.SetFloat("moveX", moveInput.x);
                animator.SetFloat("moveY", moveInput.y);
            }
        }
    }

    private void FixedUpdate()
    {
        if (currentState == PlayerState.Normal)
        {
            float currentSpeed = isRunning ? moveSpeed * runMultiplier : moveSpeed;
            Vector2 input = moveInput;
            if (input.sqrMagnitude > 1f) input = input.normalized;
            rb.linearVelocity = input * currentSpeed;
        }
    }

    private bool isDashing() => currentState == PlayerState.Dashing;

    private IEnumerator Dash(Vector2 direction)
    {
        currentState = PlayerState.Dashing;
        canDash = false;

        animator.SetFloat("moveX", direction.x);
        animator.SetFloat("moveY", direction.y);
        animator.SetTrigger("dash");

        rb.linearVelocity = direction * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;
        currentState = PlayerState.Normal;
        animator.speed = 1f;
        if (spriteRenderer != null) spriteRenderer.color = baseSpriteColor;

        yield return new WaitForSeconds(Mathf.Max(0f, dashCooldown - dashDuration));
        canDash = true;
    }
}