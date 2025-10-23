using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    private Rigidbody2D rb;
    private Animator animator;
    private PlayerControls controls;

    private Vector2 moveInput;
    private Vector2 lastDirection = Vector2.up;

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

        // Jump input removed per request.
    }

    private void OnEnable() => controls?.Enable();
    private void OnDisable() => controls?.Disable();

    private void Update()
    {
        bool isMoving = moveInput.sqrMagnitude > 0.01f;
        animator.SetBool("iswalking", isMoving);
        if (isMoving)
        {
            animator.SetFloat("moveX", moveInput.x);
            animator.SetFloat("moveY", moveInput.y);
        }
    }

    private void FixedUpdate()
    {
        Vector2 input = moveInput;
        if (input.sqrMagnitude > 1f) input = input.normalized;
        rb.linearVelocity = input * moveSpeed;
    }
}