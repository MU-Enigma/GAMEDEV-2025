using UnityEngine;

public class TopDownPlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;

    [Header("Dash")]
    public float dashSpeed = 12f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool isDashing = false; // Fixed: was initialized to true
    private float dashTime;
    private float lastDashTime;

    [Header("Animation")]
    public Animator animator;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();   
    }
    
    void Update()
    {
        // Movement input
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        // Update animator parameters if you have animations
        if (animator != null)
        {
            animator.SetFloat("MoveX", moveInput.x);
            animator.SetFloat("MoveY", moveInput.y);
            animator.SetFloat("Speed", moveInput.sqrMagnitude);
        }

        // Dash input
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetButtonDown("Fire3_Controller") && Time.time >= lastDashTime + dashCooldown && !isDashing)
        {
            StartDash();
        }
    }

    void FixedUpdate()
    {
        if (isDashing)
        {
            rb.linearVelocity = moveInput * dashSpeed;
            if (Time.time >= dashTime)
            {
                isDashing = false;
                rb.linearVelocity = Vector2.zero;
            }
        }
        else
        {
            rb.linearVelocity = moveInput * moveSpeed;
        }
    }

    void StartDash()
    {
        isDashing = true;
        dashTime = Time.time + dashDuration;
        lastDashTime = Time.time;

        if (animator != null)
            animator.SetTrigger("Dash");
    }
}