using UnityEngine;
using UnityEngine.InputSystem;

public class Test : MonoBehaviour{
    [Header("Movement Settings")]
    public float moveSpeed = 3f;
    [Header("Dodge/Roll Settings")]
    public float dodgeForce = 8f;
    public float dodgeDuration = 0.3f;
    public float dodgeCooldown = 0.5f;
    private bool isDodging;
    private float dodgeTimer;
    private float dodgeCooldownTimer;
    private float dodgeAnimDuration = 0.8f;
    private float dodgeAnimTimer;
    private Vector2 dodgeDirection;
    [Header("Attack Settings")]
    public float attackRadius = 1.5f;
    public float attackCooldown = 0.6f;
    private float attackTimer;
    private bool isAttacking;
    [Header("Animation")]
    private Animator animator;
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip dodgeSound;
    public AudioClip slashSound;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Vector2 lastMoveDirection;
    private BoxCollider2D playerCollider;
    private Camera cam;
    private float halfWidth;
    private float halfHeight;
    [Header("Crosshair Facing")]
    public CrosshairController crosshairController;
    private Vector2 mouseWorldPosition;

    void Start(){
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;
        rb.freezeRotation = true;

        playerCollider = GetComponent<BoxCollider2D>();
        animator = GetComponent<Animator>();

        if (audioSource == null){
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null){
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }

        if (dodgeSound == null){
            dodgeSound = Resources.Load<AudioClip>("dodge");
        }
        if (slashSound == null){
            slashSound = Resources.Load<AudioClip>("slash");
        }

        cam = Camera.main;
        halfHeight = cam.orthographicSize - 0.5f;
        halfWidth = halfHeight * cam.aspect - 0.2f;

        lastMoveDirection = Vector2.down;

        if (crosshairController == null){
            crosshairController = FindAnyObjectByType<CrosshairController>();
        }
    }

    void Update(){
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        if (moveInput.magnitude > 0.1f){
            lastMoveDirection = moveInput.normalized;
        }

        mouseWorldPosition = cam.ScreenToWorldPoint(Input.mousePosition);

        Vector2 directionToCrosshair = (mouseWorldPosition - (Vector2)transform.position).normalized;

        if (directionToCrosshair.x > 0.01f){
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (directionToCrosshair.x < -0.01f){
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        if (dodgeCooldownTimer > 0)
            dodgeCooldownTimer -= Time.deltaTime;
        if (dodgeAnimTimer > 0)
            dodgeAnimTimer -= Time.deltaTime;

        if ((Input.GetKeyDown(KeyCode.Space) || (Gamepad.current != null && Gamepad.current.bButton.wasPressedThisFrame)) && dodgeCooldownTimer <= 0){
            isDodging = true;
            dodgeTimer = dodgeDuration;
            dodgeCooldownTimer = dodgeCooldown;
            dodgeAnimTimer = dodgeAnimDuration;
            dodgeDirection = lastMoveDirection;

            if (animator != null)
                animator.Play("P_dash");

            if (audioSource != null && dodgeSound != null){
                audioSource.PlayOneShot(dodgeSound);
            }
        }

        if (attackTimer > 0){
            attackTimer -= Time.deltaTime;
            if (attackTimer <= 0)
                isAttacking = false;
        }

        if ((Input.GetMouseButtonDown(0) || (Gamepad.current != null && Gamepad.current.leftTrigger.wasPressedThisFrame)) && attackTimer <= 0){

            attackTimer = attackCooldown;
            isAttacking = true;

            if (audioSource != null && slashSound != null)
                audioSource.PlayOneShot(slashSound);
        

            if (animator != null)
                animator.Play("P_attack");
        }

        UpdateAnimationState();
    }

    void UpdateAnimationState(){
        if (animator == null) return;

        bool isMoving = moveInput.magnitude > 0.1f && !isDodging;
        bool shouldPlayDodgeAnim = dodgeAnimTimer > 0;

        AnimatorStateInfo currentState = animator.GetCurrentAnimatorStateInfo(0);

        if (isAttacking){
            return;
        }
        else if (shouldPlayDodgeAnim){
            if (!currentState.IsName("P_dash") && !animator.IsInTransition(0)){
                animator.Play("P_dash");
            }
        }
        else if (isMoving){
            if (!currentState.IsName("P_run") && !animator.IsInTransition(0)){
                animator.Play("P_run");
            }
        }
        else{
            if (!currentState.IsName("P_idle") && !animator.IsInTransition(0)){
                animator.Play("P_idle");
            }
        }
    }

    void FixedUpdate(){
        Vector2 targetVelocity = Vector2.zero;

        if (isDodging){
            targetVelocity = dodgeDirection * dodgeForce;

            dodgeTimer -= Time.fixedDeltaTime;
            if (dodgeTimer <= 0)
                isDodging = false;
        }else{
            targetVelocity = moveInput * moveSpeed;
        }
        rb.linearVelocity = targetVelocity;
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, -halfWidth, halfWidth);
        pos.y = Mathf.Clamp(pos.y, -halfHeight, halfHeight);
        transform.position = pos;
    }

}
