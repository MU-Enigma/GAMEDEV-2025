using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float runMultiplier = 1.75f;

    [Header("Dash Settings")]
    public float dashDuration = 0.2f;      // base duration (will scale)
    public float dashCooldown = 1f;

    [Header("Charged Dash Settings")]
    public float minDashSpeed = 8f;
    public float maxDashSpeed = 20f;
    public float maxChargeTime = 2f;               
    public float minDashAnimSpeed = 1f;
    public float maxDashAnimSpeed = 2f;
    public float maxDashDurationMultiplier = 1.5f;

    private bool isChargingDash = false;
    private float dashCharge = 0f;

    [Header("Roll Settings")]
    public float rollSpeed = 12f;
    public float rollDuration = 0.3f;
    public float rollCooldown = 1f;

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
    private bool canRoll = true;

    private Vector2 lastDirection = Vector2.up;
    private Color baseSpriteColor;

    private enum PlayerState { Normal, Dashing, Rolling, Jumping }
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
        controls.Player.Dash.started += ctx =>
        {
            if (canDash && currentState == PlayerState.Normal)
            {
                isChargingDash = true;
                dashCharge = 0f;
                if (moveInput.sqrMagnitude <= 0.01f && lastDirection.sqrMagnitude > 0.001f)
                {
                    spriteRenderer.flipX = lastDirection.x < -0.01f;
                }
            }
        };

        controls.Player.Dash.canceled += ctx =>
        {
            if (isChargingDash && currentState == PlayerState.Normal && canDash)
            {
                isChargingDash = false;
                float chargePercent = Mathf.Clamp01(dashCharge / maxChargeTime);
                float dashPower = Mathf.Lerp(minDashSpeed, maxDashSpeed, chargePercent);

                Vector2 dashDir = moveInput.sqrMagnitude > 0.01f ? moveInput.normalized : lastDirection;

                spriteRenderer.flipX = dashDir.x < -0.01f;
                animator.SetFloat("moveX", dashDir.x);
                animator.SetFloat("moveY", dashDir.y);

                StartCoroutine(Dash(dashDir, dashPower, chargePercent));
            }
        };

        // Roll input
        controls.Player.Roll.performed += ctx =>
        {
            if (canRoll && currentState == PlayerState.Normal && !isChargingDash)
            {
                Vector2 rollDir = moveInput.sqrMagnitude > 0.01f ? moveInput.normalized : lastDirection;
                spriteRenderer.flipX = rollDir.x < -0.01f;
                animator.SetFloat("moveX", rollDir.x);
                animator.SetFloat("moveY", rollDir.y);
                StartCoroutine(Roll(rollDir));
            }
        };

        // Jump input
        controls.Player.Jump.performed += ctx =>
        {
            if (currentState == PlayerState.Normal && !isChargingDash && zPosition <= 0f)
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

        if (isChargingDash)
        {
            dashCharge += Time.deltaTime;
            dashCharge = Mathf.Min(dashCharge, maxChargeTime);
            if (spriteRenderer != null)
            {
                float glowIntensity = Mathf.Lerp(1f, 2.2f, dashCharge / maxChargeTime);
                spriteRenderer.color = new Color(baseSpriteColor.r * glowIntensity,
                                                 baseSpriteColor.g * glowIntensity,
                                                 baseSpriteColor.b * glowIntensity,
                                                 baseSpriteColor.a);
            }
        }
        else
        {
            if (!isDashing() && spriteRenderer != null) spriteRenderer.color = baseSpriteColor;
        }

        if (currentState == PlayerState.Normal && !isChargingDash)
        {
            if (moveInput.x > 0.01f) spriteRenderer.flipX = false;
            else if (moveInput.x < -0.01f) spriteRenderer.flipX = true;
        }

        if (currentState == PlayerState.Normal)
        {
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
    private bool isRolling() => currentState == PlayerState.Rolling;

    private IEnumerator Dash(Vector2 direction, float dashPower, float chargePercent)
    {
        currentState = PlayerState.Dashing;
        canDash = false;

        animator.SetFloat("moveX", direction.x);
        animator.SetFloat("moveY", direction.y);
        animator.speed = Mathf.Lerp(minDashAnimSpeed, maxDashAnimSpeed, chargePercent);
        animator.SetTrigger("dash");

        float scaledDuration = dashDuration * Mathf.Lerp(1f, maxDashDurationMultiplier, chargePercent);
        rb.linearVelocity = direction * dashPower;

        yield return new WaitForSeconds(scaledDuration);

        rb.linearVelocity = Vector2.zero;
        currentState = PlayerState.Normal;
        animator.speed = 1f;
        if (spriteRenderer != null) spriteRenderer.color = baseSpriteColor;

        yield return new WaitForSeconds(Mathf.Max(0f, dashCooldown - scaledDuration));
        canDash = true;
    }

    private IEnumerator Roll(Vector2 direction)
    {
        currentState = PlayerState.Rolling;
        canRoll = false;

        animator.SetFloat("moveX", direction.x);
        animator.SetFloat("moveY", direction.y);
        animator.SetTrigger("roll");

        rb.linearVelocity = direction * rollSpeed;
        yield return new WaitForSeconds(rollDuration);

        rb.linearVelocity = Vector2.zero;
        currentState = PlayerState.Normal;

        yield return new WaitForSeconds(Mathf.Max(0f, rollCooldown - rollDuration));
        canRoll = true;
    }
}