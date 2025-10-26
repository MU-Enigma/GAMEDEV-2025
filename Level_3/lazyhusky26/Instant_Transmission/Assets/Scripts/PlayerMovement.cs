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
    public float afterImageDistance = 0.5f;
    private Vector2 lastAfterImagePosition;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        lastAfterImagePosition = transform.position;
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        if (moveInput.x != 0)
        {
            sr.flipX = moveInput.x < 0;
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

            if (Vector2.Distance(transform.position, lastAfterImagePosition) >= afterImageDistance)
            {
                CreateAfterImage();
                lastAfterImagePosition = transform.position;
            }

            transform.localScale = new Vector3(
                originalScale.x * dashSquash,
                originalScale.y * dashStretch,
                originalScale.z
            );
        }
        else
        {
            rb.linearVelocity = moveInput * moveSpeed;

            if (rb.linearVelocity.magnitude > 0.1f)
            {
                transform.localScale = new Vector3(
                    originalScale.x * moveSquashAmount,
                    originalScale.y * moveStretchAmount,
                    originalScale.z
                );
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
        lastAfterImagePosition = transform.position; // reset position for accurate spacing
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
            aisr.color = sr.color;
            aisr.sortingLayerID = sr.sortingLayerID;
            aisr.sortingOrder = sr.sortingOrder - 1;
        }
    }
}
