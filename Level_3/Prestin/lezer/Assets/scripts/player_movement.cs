using UnityEngine;
[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    [Header("Dash Settings")]
    public float dashSpeed = 15f;

    public bool isFacingRight = true;
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
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
    }
    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();
       if (moveInput.x > 0 && !isFacingRight)
{
    Flip();
}
else if (moveInput.x < 0 && isFacingRight)
{
    Flip();
}
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
 void Flip()
{
    isFacingRight = !isFacingRight;

    // This is the key: we flip the "original" scale.
    originalScale.x *= -1;
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
            aisr.flipX = !isFacingRight;
        }
    } }