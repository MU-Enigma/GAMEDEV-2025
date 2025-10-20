using UnityEngine;
using System.Collections;
using System.Collections.Generic; // Required for List

/// <summary>
/// Handles continuous enemy spawning off-screen around the player, 
/// enforcing a maximum limit, and cleaning up distant enemies.
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The enemy prefab to spawn.")]
    public GameObject enemyPrefab;
    [Tooltip("The player's Transform to center spawning and despawning around.")]
    public Transform player;
    
    [Header("Spawning Limits")]
    [Tooltip("The maximum number of enemies allowed in the scene at once.")]
    public int maxActiveEnemies = 10;
    [Tooltip("The time delay between spawn attempts (seconds).")]
    public float spawnInterval = 3f;

    [Header("Off-Screen Placement")]
    [Tooltip("Minimum distance from the screen edge to spawn the enemy (ensures it's truly off-screen).")]
    public float minSpawnDistance = 2f;
    [Tooltip("Maximum distance from the player the enemy can spawn (used to limit how far off-screen it gets).")]
    public float maxSpawnDistance = 15f;
    
    [Header("Despawn Settings")]
    [Tooltip("Distance from the player at which an enemy is automatically destroyed.")]
    public float despawnRadius = 30f; 

    private Camera mainCamera;
    private int activeEnemyCount = 0;
    private List<GameObject> activeEnemies; // List to track all enemies for despawn checks

    void Start()
    {
        // Validate dependencies and try to find the player if not set
        if (enemyPrefab == null)
        {
            Debug.LogError("EnemySpawner: Enemy Prefab is not assigned!");
            enabled = false;
            return;
        }
        if (player == null)
        {
            PlayerController pc = FindFirstObjectByType<PlayerController>();
            if (pc != null)
                player = pc.transform;
            else
            {
                Debug.LogError("EnemySpawner: Player reference is not assigned and could not be auto-found!");
                enabled = false;
                return;
            }
        }
        
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("EnemySpawner: Main Camera not found!");
            enabled = false;
            return;
        }

        activeEnemies = new List<GameObject>();
        
        // Start the continuous spawn routine
        StartCoroutine(SpawnRoutine());
    }

    void Update()
    {
        HandleDespawnCheck();
    }
    
    /// <summary>
    /// Checks all active enemies for distance from the player and despawns them if too far.
    /// This runs every frame to ensure enemies outside the radius are cleaned up.
    /// </summary>
    void HandleDespawnCheck()
    {
        if (player == null) return;
        
        // Iterate backwards to safely remove items while looping
        for (int i = activeEnemies.Count - 1; i >= 0; i--)
        {
            GameObject enemy = activeEnemies[i];
            
            // Cleanup check in case the enemy was destroyed by a bullet mid-loop
            // or by BulletCollisionDetection.DeathSequence() which sets the count
            if (enemy == null)
            {
                activeEnemies.RemoveAt(i);
                continue;
            }

            float distance = Vector3.Distance(enemy.transform.position, player.position);

            if (distance > despawnRadius)
            {
                Debug.Log($"Enemy despawned by Spawner due to distance. Distance: {distance}");
                
                // Remove from tracking list and destroy the object
                activeEnemies.RemoveAt(i);
                Destroy(enemy);
                
                // Decrement the active count
                activeEnemyCount = Mathf.Max(0, activeEnemyCount - 1);
            }
        }
    }

    /// <summary>
    /// The main routine for continuous enemy spawning.
    /// </summary>
    IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            
            // Check if we are below the active enemy limit
            if (activeEnemyCount < maxActiveEnemies)
            {
                TrySpawnEnemy();
            }
        }
    }

    /// <summary>
    /// Attempts to find an off-screen position and spawn an enemy.
    /// </summary>
    void TrySpawnEnemy()
    {
        // 1. Determine a random angle and distance for the spawn point
        float angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
        float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
        
        // 2. Calculate the potential spawn position relative to the player
        Vector3 playerPos = player.position;
        Vector3 spawnPos = playerPos + new Vector3(
            Mathf.Cos(angle) * distance,
            Mathf.Sin(angle) * distance,
            0f
        );

        // 3. Ensure the calculated position is actually OFF-SCREEN
        if (IsPositionOffScreen(spawnPos))
        {
            // 4. Instantiate the enemy
            GameObject newEnemy = Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
            
            // 5. Track the new enemy
            activeEnemies.Add(newEnemy); 
            activeEnemyCount++;
            
            Debug.Log($"EnemySpawner: Spawned enemy at {spawnPos}. Active count: {activeEnemyCount}");
        }
    }
    
    /// <summary>
    /// Checks if a world position is outside the camera's view.
    /// </summary>
    bool IsPositionOffScreen(Vector3 worldPosition)
    {
        if (mainCamera == null) return false;
        
        Vector3 viewportPoint = mainCamera.WorldToViewportPoint(worldPosition);
        
        // Check if the point is outside the [0, 1] range in X or Y
        bool isOffX = viewportPoint.x < 0f || viewportPoint.x > 1f;
        bool isOffY = viewportPoint.y < 0f || viewportPoint.y > 1f;
        
        return isOffX || isOffY;
    }

    /// <summary>
    /// Called by the enemy (BulletCollisionDetection) when it is destroyed by the player.
    /// </summary>
    public void OnEnemyKilled()
    {
        // The enemy is destroyed by the player, so we only need to decrement the count.
        // The enemy object will be removed from the 'activeEnemies' list by the next
        // despawn check loop (HandleDespawnCheck) when it detects a 'null' reference.
        activeEnemyCount = Mathf.Max(0, activeEnemyCount - 1);
        Debug.Log($"EnemySpawner: Enemy killed by player. New active count: {activeEnemyCount}");
    }

    // Optional: Draw the spawning range in the editor for visualization
    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        // Draw the DESPAWN radius
        Gizmos.color = Color.gray;
        Gizmos.DrawWireSphere(player.position, despawnRadius);
        
        // Draw the MAX spawn range circle
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(player.position, maxSpawnDistance);
        
        // Draw the MIN spawn range circle (to avoid spawning too close)
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(player.position, minSpawnDistance);
    }
}
