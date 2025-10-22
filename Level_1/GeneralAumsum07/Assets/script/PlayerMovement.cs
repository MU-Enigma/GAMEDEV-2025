using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Jump Settings")]
    public float jumpForce = 5f;
    public float gravity = 9.8f;
    private float zPosition = 0f;
    private float verticalVelocity = 0f;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerControls controls;

    private Vector2 moveInput;

    private Vector2 lastDirection = Vector2.up;

    private enum PlayerState { Normal, Jumping }
    private PlayerState currentState = PlayerState.Normal;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
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
            bool isMoving = moveInput.sqrMagnitude > 0.01f;
            animator.SetBool("iswalking", isMoving);
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
            Vector2 input = moveInput;
            if (input.sqrMagnitude > 1f) input = input.normalized;
            rb.linearVelocity = input * moveSpeed;
        }
    }
}