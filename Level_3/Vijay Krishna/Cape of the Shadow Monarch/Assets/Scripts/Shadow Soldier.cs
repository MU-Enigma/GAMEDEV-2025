using UnityEngine;
using System; // Required for 'Action'
using System.Collections; // Required for Coroutines

[RequireComponent(typeof(Rigidbody2D))] // We need a Rigidbody for movement
public class Ally : MonoBehaviour
{
    // Event that the AllySpawner will listen for
    public static event Action OnAllyDied;

    [Header("Stats")]
    public int health = 3; // Example health for your ally

    [Header("AI Settings")]
    public float moveSpeed = 3f;
    public float attackRange = 1.0f;
    public float attackDamage = 1f;
    public float attackCooldown = 1.5f;
    
    [Header("Bash Attack Animation")]
    public float bashForce = 10f;
    public float bashWindup = 0.3f; // Time to move back
    public float bashDuration = 0.1f; // Time for the lunge

    private Transform targetEnemy;
    private Rigidbody2D rb;
    private bool isAttacking = false;

    // Simple state machine
    private enum AllyState { Idle, Seeking, Attacking }
    private AllyState currentState = AllyState.Idle;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0; // Assuming top-down
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void Start()
    {
        // Start the AI "brain" to find enemies
        StartCoroutine(FindNearestEnemyRoutine());
    }
    
    void Update()
    {
        if (isAttacking) return; // Don't do anything else if in an attack animation

        if (targetEnemy == null)
        {
            currentState = AllyState.Idle;
            return;
        }

        // Check distance to the target
        float distance = Vector2.Distance(transform.position, targetEnemy.position);

        if (distance <= attackRange)
        {
            // If in range and not already attacking, start the attack
            currentState = AllyState.Attacking;
            StartCoroutine(BashAttack(targetEnemy));
        }
        else
        {
            // If out of range, go back to seeking
            currentState = AllyState.Seeking;
        }
    }

    void FixedUpdate()
    {
        // --- THIS IS THE FIX ---
        // If we are currently in the bash animation,
        // let the BashAttack coroutine control the Rigidbody.
        if (isAttacking)
        {
            return;
        }
        // --- END FIX ---

        // Only move if we are in the "Seeking" state
        if (currentState == AllyState.Seeking && targetEnemy != null)
        {
            Vector2 direction = (targetEnemy.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero; // Stop moving
        }
    }

    /// <summary>
    /// Coroutine to find the nearest enemy. Runs every half-second.
    /// </summary>
    IEnumerator FindNearestEnemyRoutine()
    {
        while (true)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
            
            if (enemies.Length == 0)
            {
                targetEnemy = null;
                currentState = AllyState.Idle;
                yield return new WaitForSeconds(0.5f);
                continue;
            }

            Transform closest = null;
            float minDistance = Mathf.Infinity;
            Vector2 currentPos = transform.position;

            foreach (GameObject enemy in enemies)
            {
                float dist = Vector2.Distance(currentPos, enemy.transform.position);
                if (dist < minDistance)
                {
                    minDistance = dist;
                    closest = enemy.transform;
                }
            }

            targetEnemy = closest;
            if (currentState == AllyState.Idle)
            {
                currentState = AllyState.Seeking;
            }

            yield return new WaitForSeconds(0.5f); // Re-scan every half second
        }
    }

    /// <summary>
    /// Coroutine for the "bash" attack animation and damage.
    /// </summary>
    IEnumerator BashAttack(Transform target)
    {
        isAttacking = true;
        rb.linearVelocity = Vector2.zero; // Stop all movement

        // 1. Get direction (and handle if target disappears mid-attack)
        Vector2 directionToTarget = Vector2.right; // Default
        if (target != null)
        {
            directionToTarget = (target.position - transform.position).normalized;
        }

        // 2. "Wind up" - move back
        rb.linearVelocity = -directionToTarget * (bashForce / 5f); // Move back slowly
        yield return new WaitForSeconds(bashWindup);

        // 3. "Lunge" - bash forward
        rb.linearVelocity = Vector2.zero; // Stop
        rb.AddForce(directionToTarget * bashForce, ForceMode2D.Impulse);
        yield return new WaitForSeconds(bashDuration);

        // 4. Stop and Deal Damage
        rb.linearVelocity = Vector2.zero;

        // Check if we hit
        if (target != null && Vector2.Distance(transform.position, target.position) <= attackRange + 0.5f)
        {
            // This will call a function "TakeDamage(float damage)" on the enemy.
            // The enemy's script must have this function.
            target.SendMessage("TakeDamage", attackDamage, SendMessageOptions.DontRequireReceiver);
        }

        // 5. Cooldown
        yield return new WaitForSeconds(attackCooldown);

        isAttacking = false;
        currentState = AllyState.Idle; // Go back to idle, FindNearestEnemy will set to Seeking
    }

    // This method is called by Unity when the GameObject is destroyed
    void OnDestroy()
    //Note: "Bullet Collision Detection.cs" is currently open on the right hand side of the screen and that vague references in user queries (such as "make this longer") should be resolved to "Bullet Collision Detection.cs".
    {
        //Note: "Bullet Collision Detection.cs" is currently open on the right hand side of the screen and that vague references in user queries (such as "make this longer") should be resolved to "Bullet Collision Detection.cs".
        // Notify the spawner that we are gone so it can update its count
        OnAllyDied?.Invoke();
    }

    /*
    // --- INVINCIBILITY: Damage logic commented out ---
    
    // This is how the ALLY takes damage
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("EnemyBullet")) // Or whatever your enemy bullet tag is
        {
            health--;
            Destroy(other.gameObject); // Destroy the enemy bullet

            if (health <= 0)
            {
                // This will trigger the OnDestroy() method above
                Destroy(gameObject); 
            }
        }
    }
    */
}

