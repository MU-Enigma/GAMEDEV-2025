// PlayerMovement.cs
using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float acceleration = 15f;
    public float deceleration = 20f;
    public float maxSpeed = 7f;
    
    [Header("Dash Settings")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;
    public float dashChargeTime = 0.5f;
    public float minDashSpeed = 10f;
    public float maxDashSpeed = 20f;
    
    [Header("Rotation Settings")]
    public float rotationSpeed = 10f;
    public float controllerDeadzone = 0.2f;
    
    [Header("Input Settings")]
    public KeyCode dashKey = KeyCode.Space;
    public string controllerDashButton = "L2"; // PS4 L2 or Xbox LT
    
    [Header("Visual Feedback")]
    public ParticleSystem dashParticles;
    public GameObject dashAfterimagePrefab;
    public float afterimageSpacing = 0.1f;
    
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private Vector2 moveInput;
    
    private bool isDashing = false;
    private bool isChargingDash = false;
    private bool isFacingRight = true;
    private float dashTimer;
    private float dashCooldownTimer;
    private float chargeTimer;
    private Vector2 dashDirection;
    private float afterimageTimer;
    private Vector2 lastAimDirection = Vector2.right;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        
        rb.gravityScale = 0;
        rb.linearDamping = 0;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Update()
    {
        GetInput();
        HandleDashInput();
        HandleDashTimers();
        HandleAfterimages();
        HandleRotation();
    }

    void FixedUpdate()
    {
        if (!isDashing && !isChargingDash)
        {
            HandleMovement();
        }
        else if (isDashing)
        {
            HandleDashMovement();
        }
        else if (isChargingDash)
        {
            HandleChargingMovement();
        }
    }

    private void GetInput()
    {
        // Left stick movement for controller
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        
        if (moveInput.magnitude > 1f)
        {
            moveInput.Normalize();
        }
    }

    private void HandleRotation()
    {
        Vector2 direction = Vector2.zero;
        
        if (MousePointerController.Instance.UsingController)
        {
            // Use pointer position for rotation with controller
            Vector3 pointerPos = MousePointerController.Instance.PointerWorldPosition;
            direction = new Vector2(pointerPos.x - transform.position.x, pointerPos.y - transform.position.y);
            
            if (direction.magnitude > 0.1f)
            {
                lastAimDirection = direction.normalized;
            }
        }
        else
        {
            // Mouse-based rotation
            if (mainCamera != null)
            {
                Vector3 mousePos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
                direction = new Vector2(mousePos.x - transform.position.x, mousePos.y - transform.position.y);
                if (direction.magnitude > 0.1f)
                {
                    lastAimDirection = direction.normalized;
                }
            }
        }
        
        if (direction.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            bool shouldFaceRight = angle > -89f && angle < 89f;
            
            if (shouldFaceRight != isFacingRight)
            {
                isFacingRight = shouldFaceRight;
                spriteRenderer.flipX = !isFacingRight;
            }

            if (!isFacingRight) angle += 180f;
            if (angle > 180f) angle -= 360f;

            Quaternion targetRotation = Quaternion.Euler(new Vector3(0, 0, angle));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    private void HandleMovement()
    {
        Vector2 targetVelocity = moveInput * moveSpeed;
        
        if (moveInput.magnitude > 0.1f)
        {
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        }
        else
        {
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }
        
        if (rb.linearVelocity.magnitude > maxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * maxSpeed;
        }
    }

    private void HandleDashInput()
    {
        if (dashCooldownTimer > 0f)
        {
            dashCooldownTimer -= Time.deltaTime;
        }
        
        bool dashInput = false;
        if (!MousePointerController.Instance.UsingController)
        {
            dashInput = Input.GetKeyDown(dashKey);
        }
        else
        {
            dashInput = Input.GetButtonDown(controllerDashButton);
        }
        
        if (dashInput && dashCooldownTimer <= 0f)
        {
            StartChargingDash();
        }
        
        if (isChargingDash)
        {
            bool stillHolding = false;
            if (!MousePointerController.Instance.UsingController)
            {
                stillHolding = Input.GetKey(dashKey);
            }
            else
            {
                stillHolding = Input.GetButton(controllerDashButton);
            }
            
            if (stillHolding)
            {
                ContinueChargingDash();
            }
            else
            {
                ReleaseDash();
            }
        }
        
        if (isDashing)
        {
            dashTimer -= Time.deltaTime;
            if (dashTimer <= 0f)
            {
                EndDash();
            }
        }
    }

    private void StartChargingDash()
    {
        isChargingDash = true;
        chargeTimer = 0f;
        
        if (dashParticles != null && !dashParticles.isPlaying)
        {
            dashParticles.Play();
        }
    }

    private void ContinueChargingDash()
    {
        chargeTimer += Time.deltaTime;
        if (chargeTimer > dashChargeTime)
        {
            chargeTimer = dashChargeTime;
        }
        
        float chargePercent = chargeTimer / dashChargeTime;
        if (spriteRenderer != null)
        {
            Color color = Color.Lerp(Color.white, Color.cyan, chargePercent);
            spriteRenderer.color = color;
        }
    }

    private void ReleaseDash()
    {
        isChargingDash = false;
        
        float chargePercent = Mathf.Clamp01(chargeTimer / dashChargeTime);
        float actualDashSpeed = Mathf.Lerp(minDashSpeed, maxDashSpeed, chargePercent);
        
        if (moveInput.magnitude > 0.1f)
        {
            dashDirection = moveInput.normalized;
        }
        else
        {
            dashDirection = lastAimDirection;
            if (dashDirection.magnitude < 0.1f)
            {
                dashDirection = Vector2.right;
            }
        }
        
        StartDash(actualDashSpeed);
        
        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
        
        if (dashParticles != null)
        {
            dashParticles.Stop();
        }
    }

    private void StartDash(float speed)
    {
        isDashing = true;
        dashTimer = dashDuration;
        dashCooldownTimer = dashCooldown;
        
        rb.linearVelocity = dashDirection * speed;
        CreateDashAfterimage();
    }

    private void HandleDashMovement()
    {
        // Maintain dash velocity during dash
    }

    private void HandleChargingMovement()
    {
        Vector2 targetVelocity = moveInput * moveSpeed * 0.5f;
        
        if (moveInput.magnitude > 0.1f)
        {
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, acceleration * 0.5f * Time.fixedDeltaTime);
        }
        else
        {
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
        }
    }

    private void EndDash()
    {
        isDashing = false;
        
        if (moveInput.magnitude > 0.1f)
        {
            rb.linearVelocity = moveInput.normalized * moveSpeed * 1.2f;
        }
        else
        {
            rb.linearVelocity = rb.linearVelocity * 0.7f;
        }
    }

    private void HandleDashTimers()
    {
        // Cooldown timer is handled in HandleDashInput
    }

    private void HandleAfterimages()
    {
        if (isDashing && afterimageTimer <= 0f)
        {
            CreateDashAfterimage();
            afterimageTimer = afterimageSpacing;
        }
        
        if (afterimageTimer > 0f)
        {
            afterimageTimer -= Time.deltaTime;
        }
    }

    private void CreateDashAfterimage()
    {
        if (dashAfterimagePrefab != null)
        {
            GameObject afterimage = Instantiate(dashAfterimagePrefab, transform.position, transform.rotation);
            SetupAfterimage(afterimage);
        }
    }

    private void SetupAfterimage(GameObject afterimage)
    {
        RemovePhysicsComponents(afterimage);
        
        SpriteRenderer afterimageSR = afterimage.GetComponent<SpriteRenderer>();
        if (afterimageSR != null && spriteRenderer != null)
        {
            afterimageSR.sprite = spriteRenderer.sprite;
            afterimageSR.flipX = spriteRenderer.flipX;
            
            Color color = afterimageSR.color;
            color.a = 0.5f;
            afterimageSR.color = color;
        }
        
        Destroy(afterimage, 0.5f);
    }

    private void RemovePhysicsComponents(GameObject obj)
    {
        Rigidbody2D rb2d = obj.GetComponent<Rigidbody2D>();
        if (rb2d != null) Destroy(rb2d);
        
        Collider2D collider = obj.GetComponent<Collider2D>();
        if (collider != null) Destroy(collider);
    }

    public bool IsDashing() => isDashing;
    public bool IsChargingDash() => isChargingDash;
    public Vector2 GetMoveInput() => moveInput;
    public Vector2 GetVelocity() => rb.linearVelocity;
    public float GetDashChargePercent() => Mathf.Clamp01(chargeTimer / dashChargeTime);
}