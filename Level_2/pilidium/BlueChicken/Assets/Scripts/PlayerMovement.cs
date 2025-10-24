using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;

    [Header("Weapon Positioning")]
    private Transform weaponSlotTransform;
    private SpriteRenderer weaponSR;
    public Vector3 rightFacingPosition;
    public Vector3 leftFacingPosition;

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

    [Header("Misc")]
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private SpriteRenderer sr;
    private Vector3 originalScale;

    [Header("Afterimage Settings")]
    public GameObject afterImagePrefab;
    public float afterImageSpacing = 0.05f;
    private float afterImageTimer;

    private bool isFacingLeft = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;

        weaponSlotTransform = transform.Find("WeaponSlot");
        if (weaponSlotTransform == null)
        {
            Debug.LogWarning("PlayerMovement: Could not find child object named 'WeaponSlot'");
        }
        else
        {
            weaponSR = weaponSlotTransform.GetComponentInChildren<SpriteRenderer>();
        }

        if (weaponSlotTransform != null)
            weaponSlotTransform.localPosition = rightFacingPosition;
    }

    void Update()
    {
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        if (moveInput.x > 0.1f && isFacingLeft)
        {
            isFacingLeft = false;
            sr.flipX = false;

            if (weaponSlotTransform != null)
            {
                weaponSlotTransform.localPosition = rightFacingPosition;
                weaponSlotTransform.localRotation = Quaternion.Euler(0f, 0f, 0f);
            }

            if (weaponSR != null)
                weaponSR.flipX = false;
        }
        else if (moveInput.x < -0.1f && !isFacingLeft)
        {
            isFacingLeft = true;
            sr.flipX = true;

            if (weaponSlotTransform != null)
            {
                weaponSlotTransform.localPosition = leftFacingPosition;
                weaponSlotTransform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            }

            if (weaponSR != null)
                weaponSR.flipX = false;
        }


        if (Input.GetKeyDown(KeyCode.Space) && !isDashing && dashCooldownTimer <= 0f && moveInput.magnitude > 0.1f)
        {
            StartDash();
        }

        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                EndDash();
            }
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
            rb.velocity = moveInput * dashSpeed;
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
            rb.velocity = moveInput * moveSpeed;

            if (rb.velocity.magnitude > 0.1f)
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
    }

    void CreateAfterImage()
    {
        if (afterImagePrefab == null) return;

        GameObject ai = Instantiate(afterImagePrefab, transform.position, transform.rotation);
        ai.transform.localScale = transform.localScale;

        SpriteRenderer aisr = ai.GetComponent<SpriteRenderer>();
        if (aisr != null)
        {
            aisr.sprite = sr.sprite;
            aisr.flipX = sr.flipX;
        }
    }
}
