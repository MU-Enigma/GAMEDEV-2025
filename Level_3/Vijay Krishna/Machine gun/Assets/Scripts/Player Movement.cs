using UnityEngine;
[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    private bool isDashing = false;
    private float dashTimer;
    private float dashCooldownTimer;
    [Header("Squash & Stretch")]
    public float moveSquashAmount = 0.9f;
    public float moveStretchAmount = 1.1f;
    public float dashSquash = 1.2f;
    public float dashStretch = 0.8f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private SpriteRenderer sr;
    private Vector3 originalScale;
    [Header("Afterimage Settings")]
    public GameObject afterImagePrefab;
    public float afterImageSpacing = 0.05f;
    private float afterImageTimer;

    // --- UPDATED DUAL GUN SETTINGS ---
    [Header("Gun Settings")]
    public Transform gunRight; // Assign the gun that starts in the right hand
    public Transform gunLeft;  // Assign the gun that starts in the left hand
    public Vector3 rightHandOffset = new Vector3(0.5f, 0f, 0f); // Offset for the right hand
    public Vector3 leftHandOffset = new Vector3(-0.5f, 0f, 0f); // Offset for the left hand
    private bool isFacingRight = true;
    // --- END UPDATED ---

    void Start()
    {
        // --- FIXED GetComponent calls to use generic version ---
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        sr = GetComponent<SpriteRenderer>();
        // --- END FIX ---
        originalScale = transform.localScale;

        // --- NEW: Set initial gun positions based on isFacingRight (defaults to true) ---
        if (gunRight != null)
        {
            gunRight.localPosition = rightHandOffset;
            gunRight.localScale = new Vector3(
                Mathf.Abs(gunRight.localScale.x), // Ensure positive X scale
                gunRight.localScale.y,
                gunRight.localScale.z
            );
        }
        if (gunLeft != null)
        {
            gunLeft.localPosition = leftHandOffset;
            gunLeft.localScale = new Vector3(
                -Mathf.Abs(gunLeft.localScale.x), // Ensure negative X scale
                gunLeft.localScale.y,
                gunLeft.localScale.z
            );
        }
        // --- END NEW ---
    }
    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        // --- UPDATED FLIP LOGIC ---
        // Check for horizontal movement to flip player and position gun
        if (moveInput.x > 0.1f && !isFacingRight)
        {
            Flip();
        }
        else if (moveInput.x < -0.1f && isFacingRight)
        {
            Flip();
        }
        // --- END UPDATED LOGIC ---

        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && dashCooldownTimer <= 0f && moveInput.magnitude > 0.1f)
        {
            StartDash();
        }
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f) EndDash();
        }
        else
        {
            dashCooldownTimer -= Time.deltaTime;
            if (dashCooldownTimer < 0f) dashCooldownTimer = 0f;
        }
    }

    // --- UPDATED Flip() METHOD FOR DUAL GUNS ---
    void Flip()
    {
        isFacingRight = !isFacingRight;

        // Flip the player's sprite
        sr.flipX = !isFacingRight;

        if (isFacingRight)
        {
            // Player is now facing RIGHT.
            // gunRight goes to right hand, gunLeft goes to left hand.
            if (gunRight != null)
            {
                gunRight.localPosition = rightHandOffset;
                gunRight.localScale = new Vector3(
                    Mathf.Abs(gunRight.localScale.x),
                    gunRight.localScale.y,
                    gunRight.localScale.z
                );
            }
            if (gunLeft != null)
            {
                gunLeft.localPosition = leftHandOffset;
                gunLeft.localScale = new Vector3(
                    -Mathf.Abs(gunLeft.localScale.x),
                    gunLeft.localScale.y,
                    gunLeft.localScale.z
                );
            }
        }
        else
        {
            // Player is now facing LEFT.
            // --- UPDATED: Guns stay in their respective hands, just flip scale ---
            if (gunRight != null)
            {
                gunRight.localPosition = rightHandOffset; // Stays in right hand
                gunRight.localScale = new Vector3(
                    -Mathf.Abs(gunRight.localScale.x), // Flip scale
                    gunRight.localScale.y,
                    gunRight.localScale.z
                );
            }
            if (gunLeft != null)
            {
                gunLeft.localPosition = leftHandOffset; // Stays in left hand
                gunLeft.localScale = new Vector3(
                    Mathf.Abs(gunLeft.localScale.x), // Flip scale
                    gunLeft.localScale.y,
                    gunLeft.localScale.z
                );
            }
        }
    }
    // --- END UPDATED ---

    void FixedUpdate()
    {
        if (isDashing)
        {
            rb.linearVelocity = moveInput * dashSpeed;
            afterImageTimer -= Time.fixedDeltaTime;
            if (afterImageTimer <= 0f)
            {
                CreateAfterImage();
                afterImageTimer = afterImageSpacing;
            }
            transform.localScale = new Vector3(originalScale.x * dashSquash, originalScale.y * dashStretch, originalScale.z);
        }
        else
        {
            rb.linearVelocity = moveInput * moveSpeed;
            if (rb.linearVelocity.magnitude > 0.1f)
            {
                transform.localScale = new Vector3(originalScale.x * moveSquashAmount, originalScale.y * moveStretchAmount, originalScale.z);
            }
            else
            {
                transform.localScale = originalScale;
            }
        }
    }
    void StartDash()
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        afterImageTimer = 0f;
    }
    void EndDash()
    {
        isDashing = false;
        transform.localScale = originalScale;
    }
    void CreateAfterImage()
    {
        if (afterImagePrefab == null) return;
        GameObject ai = Instantiate(afterImagePrefab, transform.position, transform.rotation);
        SpriteRenderer aisr = ai.GetComponent<SpriteRenderer>();
        if (aisr != null)
        {
            aisr.sprite = sr.sprite;
            aisr.flipX = sr.flipX;
        }
    }
}

