using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PM : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    [Header("Environment Effects")]
    public float pondSpeedMultiplier = 0.5f;

    [Header("Water System")]
    public float maxWater = 100f;
    public float currentWater = 0f;
    public float refillRate = 10f;
    public Image waterBarFill;

    private Rigidbody2D rb;
    private Animator animator;
    [SerializeField] private TrailRenderer trail;

    private PlCo inputActions;
    private Vector2 moveInput;
    private bool isSuckingWater = false;
    private bool dashPressed = false;

    private bool isDashing = false;
    private float dashTimer;
    private float dashCooldownTimer;
    private bool isInPond = false;

    void Awake()
    {
        inputActions = new PlCo();
    }

    void OnEnable()
    {
        inputActions.Enable();

        inputActions.Player.Move.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        inputActions.Player.Move.canceled += ctx => moveInput = Vector2.zero;

        inputActions.Player.Dash.performed += ctx => dashPressed = true;

        inputActions.Player.SuckWater.performed += ctx => isSuckingWater = true;
        inputActions.Player.SuckWater.canceled += ctx => isSuckingWater = false;
    }

    void OnDisable()
    {
        inputActions.Disable();
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        rb.gravityScale = 0;
        if (trail != null) trail.enabled = false;
    }

    void Update()
{
    // Read sucking input continuously (works for held key)
    isSuckingWater = inputActions.Player.SuckWater.ReadValue<float>() > 0.1f;

    // Water refilling
    if (isInPond && isSuckingWater)
    {
        currentWater += refillRate * Time.deltaTime;
        currentWater = Mathf.Clamp(currentWater, 0, maxWater);
    }

    // --- Animation Logic ---
    if (!isSuckingWater && moveInput.magnitude > 0.1f)
    {
        animator.SetBool("IsWalk", true);
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);
        animator.SetFloat("LastX", moveInput.x);
        animator.SetFloat("LastY", moveInput.y);
    }
    else
    {
        animator.SetBool("IsWalk", false);
    }

    // Dash logic
    HandleDash();

    // Update water bar
    if (waterBarFill != null)
    {
        waterBarFill.fillAmount = currentWater / maxWater;
    }
}


    void FixedUpdate()
    {
        // 🔹 Prevent movement if sucking water
        if (isSuckingWater)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        float currentSpeed = moveSpeed;

        if (isInPond && !isDashing)
            currentSpeed *= pondSpeedMultiplier;

        if (isDashing)
            rb.linearVelocity = moveInput * dashSpeed;
        else
            rb.linearVelocity = moveInput * currentSpeed;
    }

    private void HandleDash()
    {
        // 🔹 Block dash while sucking water
        if (isSuckingWater)
        {
            dashPressed = false;
            return;
        }

        if (dashPressed && !isDashing && dashCooldownTimer <= 0f && moveInput.magnitude > 0.1f)
            StartDash();

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
                EndDash();
        }
        else if (dashCooldownTimer > 0f)
            dashCooldownTimer -= Time.deltaTime;

        dashPressed = false;
    }

    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        if (trail != null) trail.enabled = true;
    }

    void EndDash()
    {
        isDashing = false;
        if (trail != null) trail.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pond"))
        {
            isInPond = true;
            Debug.Log("Duck entered pond!");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Pond"))
        {
            isInPond = false;
            Debug.Log("Duck left pond!");
        }
    }
}