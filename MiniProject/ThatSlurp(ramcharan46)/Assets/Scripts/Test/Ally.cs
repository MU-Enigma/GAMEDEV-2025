using UnityEngine;

public class Ally : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    public float stoppingDistance = 1.5f;
    public float wanderRadius = 3f;
    public float chargeForce = 6f;
    public float avoidanceDistance = 0.8f;
    
    [Header("Combat")]
    public int health = 3;
    public float knockbackForce = 8f;
    public float stunDuration = 1.2f;
    public float damageCooldown = 0.5f;
    
    [Header("AI Behavior")]
    public float detectionRange = 8f;
    public float wanderTime = 2f;
    public float idleTime = 1f;
    public float chargeCooldown = 3f;
    
    [Header("Top-Down Settings")]
    public LayerMask obstacleLayer = 1;
    public float pathfindingRange = 1f;
    
    private Transform player;
    private Rigidbody2D rb;
    private Vector2 moveDirection;
    
    private enum EnemyState { Idle, Wandering, Chasing, Stunned, Knockback, Charging }
    private EnemyState currentState = EnemyState.Idle;
    
    private float stunTimer;
    private float wanderTimer;
    private float idleTimer;
    private float damageTimer;
    private float knockbackTimer;
    private float chargeTimer;
    private float chargeDuration = 0.8f;
    private Vector2 chargeDirection;

    private Animator animator;

    private Vector2 wanderDirection = Vector2.right;
    private Vector2 originalPosition;
    private Vector2 targetWanderPosition;
    
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Enemy").transform;
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;
            
        originalPosition = transform.position;
        
        if (rb != null)
        {
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
        }
        
        SetRandomWanderTarget();
    }

    void Update()
    {
        UpdateTimers();
        UpdateAI();
        UpdateVisualFeedback();
    }
    
    void UpdateTimers()
    {
        if (stunTimer > 0) stunTimer -= Time.deltaTime;
        if (damageTimer > 0) damageTimer -= Time.deltaTime;
        if (knockbackTimer > 0) knockbackTimer -= Time.deltaTime;
        if (wanderTimer > 0) wanderTimer -= Time.deltaTime;
        if (idleTimer > 0) idleTimer -= Time.deltaTime;
        if (chargeTimer > 0) chargeTimer -= Time.deltaTime;
    }

    private string currentAnim;

    void PlayAnimation(string newAnim)
    {
        if (currentAnim == newAnim) return;
        if (animator != null)
        {
            animator.Play(newAnim);
            currentAnim = newAnim;
        }
    }

    bool IsPathBlocked(Vector2 direction, float distance)
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, distance, obstacleLayer);
        return hit.collider != null;
    }
    
    Vector2 GetAvoidanceDirection(Vector2 desiredDirection)
    {
        if (!IsPathBlocked(desiredDirection, pathfindingRange))
            return desiredDirection;

        Vector2[] alternativeDirections = {
            Quaternion.Euler(0, 0, 45) * desiredDirection,
            Quaternion.Euler(0, 0, -45) * desiredDirection,
            Quaternion.Euler(0, 0, 90) * desiredDirection,
            Quaternion.Euler(0, 0, -90) * desiredDirection,
            -desiredDirection 
        };

        foreach (Vector2 altDir in alternativeDirections)
        {
            if (!IsPathBlocked(altDir, pathfindingRange))
                return altDir.normalized;
        }

        return Vector2.zero;
    }
    
    void UpdateAI()
    {
        if (knockbackTimer > 0)
        {
            currentState = EnemyState.Knockback;
        }
        else if (stunTimer > 0)
        {
            currentState = EnemyState.Stunned;
        }
        else if (chargeTimer > 0 && currentState == EnemyState.Charging)
        {
        }
        else if (player != null && Vector2.Distance(transform.position, player.position) <= detectionRange)
        {
            currentState = EnemyState.Chasing;
        }
        else if (wanderTimer > 0)
        {
            currentState = EnemyState.Wandering;
        }
        else if (idleTimer > 0)
        {
            currentState = EnemyState.Idle;
        }
        else
        {
            SetRandomWanderTarget();
            wanderTimer = wanderTime;
            currentState = EnemyState.Wandering;
        }
        
        switch (currentState)
        {
            case EnemyState.Idle:
                moveDirection = Vector2.zero;
                PlayAnimation("S_idle");
                break;

            case EnemyState.Wandering:
                Wander();
                PlayAnimation("S_move");
                break;

            case EnemyState.Chasing:
                ChasePlayer();
                float distanceToPlayer = Vector2.Distance(transform.position, player.position);
                if (distanceToPlayer <= stoppingDistance + 0.5f) 
                    PlayAnimation("S_attack");
                else 
                    PlayAnimation("S_move");
                break;

            case EnemyState.Stunned:
                moveDirection = Vector2.zero;
                PlayAnimation("S_gothit");
                break;

            case EnemyState.Knockback:
                moveDirection = Vector2.zero;
                PlayAnimation("S_gothit");
                break;

            case EnemyState.Charging:
                PerformCharge();
                PlayAnimation("S_jump");
                break;
        }
        
        if (currentState != EnemyState.Knockback && currentState != EnemyState.Charging)
        {
            if (rb != null)
            {
                rb.linearVelocity = moveDirection * speed;
            }
        }
        
        if (moveDirection.x > 0.1f)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else if (moveDirection.x < -0.1f)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
    }
    
    void Wander()
    {
        Vector2 directionToTarget = (targetWanderPosition - (Vector2)transform.position).normalized;
        
        if (Vector2.Distance(transform.position, targetWanderPosition) < 0.5f || IsPathBlocked(directionToTarget, pathfindingRange))
        {
            SetRandomWanderTarget();
            directionToTarget = (targetWanderPosition - (Vector2)transform.position).normalized;
        }
        
        moveDirection = GetAvoidanceDirection(directionToTarget);
        
        if (wanderTimer <= 0)
        {
            idleTimer = idleTime;
            currentState = EnemyState.Idle;
        }
    }
    
    void ChasePlayer()
    {
        if (player == null) return;
        
        Vector2 directionToPlayer = ((Vector2)player.position - (Vector2)transform.position).normalized;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer < stoppingDistance)
        {
            moveDirection = -directionToPlayer * 0.5f;
        }
        else
        {
            moveDirection = GetAvoidanceDirection(directionToPlayer);
            
            if (distanceToPlayer <= detectionRange * 0.7f && chargeTimer <= 0)
            {
                TryCharge();
            }
        }
    }
    
    void TryCharge()
    {
        if (player == null || chargeTimer > 0) return;
        
        chargeDirection = ((Vector2)player.position - (Vector2)transform.position).normalized;
        chargeTimer = chargeDuration;
        currentState = EnemyState.Charging;
        
    }
    
    void PerformCharge()
    {
        if (rb != null)
        {
            rb.linearVelocity = chargeDirection * chargeForce;
        }
        
        chargeTimer -= Time.deltaTime;
        if (chargeTimer <= 0)
        {
            chargeTimer = chargeCooldown;
            currentState = EnemyState.Chasing;
        }
    }
    
    void SetRandomWanderTarget()
    {
        Vector2 randomDirection = new Vector2(
            Random.Range(-1f, 1f),
            Random.Range(-1f, 1f)
        ).normalized;
        
        float randomDistance = Random.Range(wanderRadius * 0.3f, wanderRadius);
        targetWanderPosition = originalPosition + randomDirection * randomDistance;
        
        wanderDirection = randomDirection;
    }
    
    void UpdateVisualFeedback()
    {
        if (spriteRenderer == null) return;
        
        if (stunTimer > 0)
        {
            float flashSpeed = 8f;
            float alpha = Mathf.PingPong(Time.time * flashSpeed, 1f);
            spriteRenderer.color = Color.Lerp(originalColor, Color.red, alpha * 0.7f);
        }
        else if (damageTimer > 0)
        {
            spriteRenderer.color = Color.Lerp(originalColor, Color.gray, 0.3f);
        }
        else
        {
            spriteRenderer.color = originalColor;
        }
    }

    public void TakeDamage(int dmg)
    {
        if (damageTimer > 0) return;
        
        health -= dmg;
        damageTimer = damageCooldown;
        
        if (player != null && rb != null)
        {
            Vector2 knockbackDir = ((Vector2)transform.position - (Vector2)player.position).normalized;
            rb.linearVelocity = knockbackDir * knockbackForce;
            knockbackTimer = 0.3f;
        }
        
        stunTimer = stunDuration;
        
        if (health <= 0)
        {
            PlayAnimation("S_death");
            Destroy(gameObject, 0.5f);
        }
    }

    
}