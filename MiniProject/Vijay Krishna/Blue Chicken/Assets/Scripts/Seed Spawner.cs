using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeedSpawner : MonoBehaviour
{
    [Header("References")]
    [Tooltip("The player transform to spawn seeds around.")]
    public Transform player;
    [Tooltip("The seed prefab to spawn.")]
    public GameObject seedPrefab;
    [Tooltip("The main camera (auto-found if not set).")]
    public Camera mainCamera;

    [Header("Spawning Settings")]
    [Tooltip("Minimum seeds that should always be on screen.")]
    public int minActiveSeeds = 4;
    [Tooltip("Maximum seeds that can exist in the world at the same time.")]
    public int maxActiveSeeds = 8;
    [Tooltip("How often to attempt spawning new seeds (seconds).")]
    public float spawnFrequency = 2f;
    [Tooltip("Minimum distance from player to spawn seeds.")]
    public float minSpawnDistance = 5f;
    [Tooltip("Maximum distance from player to spawn seeds.")]
    public float maxSpawnDistance = 15f;
    
    [Header("Falling Animation")]
    [Tooltip("How high above the target position to spawn falling seeds.")]
    public float fallHeight = 8f;
    [Tooltip("How fast seeds fall (units per second).")]
    public float fallSpeed = 6f;
    [Tooltip("Random variation in fall speed.")]
    public float fallSpeedVariation = 2f;
    [Tooltip("Slight horizontal drift while falling.")]
    public float horizontalDrift = 1f;
    
    [Header("Directional Spawning")]
    [Tooltip("How much to favor spawning seeds ahead of player movement (0 = random, 1 = only ahead).")]
    [Range(0f, 1f)]
    public float forwardBias = 0.7f;
    [Tooltip("Angle range (degrees) for forward-biased spawning.")]
    public float forwardSpawnAngle = 120f;
    [Tooltip("Minimum player speed to apply directional bias.")]
    public float minSpeedForBias = 1f;
    
    [Header("Seed Distribution")]
    [Tooltip("Minimum distance between seeds to prevent clustering.")]
    public float minSeedDistance = 3f;
    [Tooltip("Maximum attempts to find a valid spawn position before giving up.")]
    public int maxSpawnAttempts = 10;
    
    [Header("Cleanup Settings")]
    [Tooltip("Distance from player at which seeds get removed/recycled.")]
    public float despawnDistance = 25f;
    
    [Header("Object Pooling")]
    [Tooltip("Total number of seed objects in the pool (should be >= maxActiveSeeds).")]
    public int poolSize = 30;

    // Private variables
    private List<GameObject> seedPool;
    private List<GameObject> activeSeeds;
    private List<SeedFallData> fallingSeeds;
    private float spawnTimer;
    private Rigidbody2D playerRigidbody;
    private Vector2 lastPlayerPosition;
    private Vector2 playerMovementDirection;
    
    // Data structure for falling seeds
    [System.Serializable]
    private class SeedFallData
    {
        public GameObject seed;
        public Vector3 targetPosition;
        public float fallSpeedCurrent;
        public Vector2 driftDirection;
        public bool isActive;
        
        public SeedFallData(GameObject seedObject, Vector3 target, float speed, Vector2 drift)
        {
            seed = seedObject;
            targetPosition = target;
            fallSpeedCurrent = speed;
            driftDirection = drift;
            isActive = true;
        }
    }
    
    void Start()
    {
        if (player == null)
        {
            Debug.LogError("SeedSpawner: Player reference not set!");
            return;
        }
        
        if (seedPrefab == null)
        {
            Debug.LogError("SeedSpawner: Seed prefab not set!");
            return;
        }
        
        // Auto-find camera if not set
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                mainCamera = FindFirstObjectByType<Camera>();
            }
        }

        // Get player rigidbody for velocity tracking
        playerRigidbody = player.GetComponent<Rigidbody2D>();
        lastPlayerPosition = player.position;
        playerMovementDirection = Vector2.right; // Default direction

        InitializePool();
        activeSeeds = new List<GameObject>();
        fallingSeeds = new List<SeedFallData>();
        spawnTimer = 0f;
        
        // Spawn initial seeds immediately to reach minimum
        for (int i = 0; i < minActiveSeeds; i++)
        {
            TrySpawnSeed();
        }
    }

    void Update()
    {
        if (player == null) return;

        UpdatePlayerMovementDirection();
        HandleFallingSeeds();
        HandleSpawning();
        HandleCleanup();
    }

    /// <summary>
    /// Handle the falling animation for seeds that are dropping from above
    /// </summary>
    void HandleFallingSeeds()
    {
        for (int i = fallingSeeds.Count - 1; i >= 0; i--)
        {
            SeedFallData fallData = fallingSeeds[i];
            
            if (!fallData.isActive || fallData.seed == null)
            {
                fallingSeeds.RemoveAt(i);
                continue;
            }
            
            // Move seed down and slightly drift horizontally
            Vector3 currentPos = fallData.seed.transform.position;
            Vector3 movement = Vector3.down * fallData.fallSpeedCurrent * Time.deltaTime;
            movement += (Vector3)fallData.driftDirection * horizontalDrift * Time.deltaTime;
            
            fallData.seed.transform.position = currentPos + movement;
            
            // Check if seed has reached target Y position
            if (fallData.seed.transform.position.y <= fallData.targetPosition.y)
            {
                // Snap to target position and stop falling
                fallData.seed.transform.position = fallData.targetPosition;
                fallData.isActive = false;
                fallingSeeds.RemoveAt(i);
                
                // Add to active seeds list
                activeSeeds.Add(fallData.seed);
                Debug.Log($"Seed finished falling. Active seeds: {activeSeeds.Count}");
            }
        }
    }

    /// <summary>
    /// Check if a world position is visible to the camera
    /// </summary>
    bool IsPositionOnScreen(Vector3 worldPosition)
    {
        if (mainCamera == null) return false;
        
        Vector3 screenPoint = mainCamera.WorldToViewportPoint(worldPosition);
        return screenPoint.x >= 0 && screenPoint.x <= 1 && 
               screenPoint.y >= 0 && screenPoint.y <= 1 && 
               screenPoint.z > 0;
    }

    /// <summary>
    /// Track the player's movement direction for biased spawning
    /// </summary>
    void UpdatePlayerMovementDirection()
    {
        Vector2 currentPosition = player.position;
        
        // Use rigidbody velocity if available, otherwise calculate from position change
        if (playerRigidbody != null && playerRigidbody.linearVelocity.magnitude > minSpeedForBias)
        {
            playerMovementDirection = playerRigidbody.linearVelocity.normalized;
        }
        else
        {
            Vector2 positionDelta = currentPosition - lastPlayerPosition;
            if (positionDelta.magnitude > 0.1f) // Only update if moved significantly
            {
                playerMovementDirection = positionDelta.normalized;
            }
        }
        
        lastPlayerPosition = currentPosition;
    }

    /// <summary>
    /// Initialize the object pool with inactive seed objects
    /// </summary>
    void InitializePool()
    {
        seedPool = new List<GameObject>();
        
        for (int i = 0; i < poolSize; i++)
        {
            GameObject seed = Instantiate(seedPrefab);
            seed.SetActive(false);
            seed.transform.SetParent(transform); // Organize under spawner in hierarchy
            seedPool.Add(seed);
        }
        
        Debug.Log($"SeedSpawner: Initialized pool with {poolSize} seeds");
    }

    /// <summary>
    /// Handle the spawning timer and spawn new seeds when needed
    /// </summary>
    void HandleSpawning()
    {
        spawnTimer -= Time.deltaTime;
        
        // Count total active seeds (including falling ones)
        int totalActiveSeeds = activeSeeds.Count + fallingSeeds.Count;
        
        // More aggressive spawning if below minimum
        bool needsMoreSeeds = totalActiveSeeds < minActiveSeeds;
        bool canSpawnMore = totalActiveSeeds < maxActiveSeeds;
        
        // Spawn immediately if below minimum, or on timer if can spawn more
        if (needsMoreSeeds || (spawnTimer <= 0f && canSpawnMore))
        {
            TrySpawnSeed();
            
            // Reset timer only if not urgently spawning
            if (!needsMoreSeeds)
            {
                spawnTimer = spawnFrequency;
            }
            else
            {
                spawnTimer = spawnFrequency * 0.3f; // Faster spawning when below minimum
            }
        }
    }

    /// <summary>
    /// Try to spawn a seed at a valid position
    /// </summary>
    void TrySpawnSeed()
    {
        GameObject seed = GetPooledSeed();
        if (seed == null) return; // No available seeds in pool

        Vector3 targetPosition = FindValidSpawnPosition();
        
        if (targetPosition != Vector3.zero) // Valid position found
        {
            // Check if the target position is on screen
            if (IsPositionOnScreen(targetPosition))
            {
                // Spawn above and make it fall
                SpawnFallingSeed(seed, targetPosition);
                Debug.Log($"Spawned falling seed above {targetPosition}");
            }
            else
            {
                // Spawn directly at position (off-screen)
                seed.transform.position = targetPosition;
                seed.SetActive(true);
                activeSeeds.Add(seed);
                Debug.Log($"Spawned seed off-screen at {targetPosition}. Active seeds: {activeSeeds.Count}");
            }
        }
        else
        {
            Debug.Log("Could not find valid spawn position for seed");
        }
    }

    /// <summary>
    /// Spawn a seed above the target position and make it fall
    /// </summary>
    void SpawnFallingSeed(GameObject seed, Vector3 targetPosition)
    {
        // Position the seed above the target
        Vector3 startPosition = targetPosition + Vector3.up * fallHeight;
        seed.transform.position = startPosition;
        seed.SetActive(true);
        
        // Calculate fall parameters
        float currentFallSpeed = fallSpeed + Random.Range(-fallSpeedVariation, fallSpeedVariation);
        Vector2 driftDir = new Vector2(Random.Range(-1f, 1f), 0f).normalized;
        
        // Add to falling seeds list
        SeedFallData fallData = new SeedFallData(seed, targetPosition, currentFallSpeed, driftDir);
        fallingSeeds.Add(fallData);
    }

    /// <summary>
    /// Find a valid spawn position that meets all criteria with directional bias
    /// </summary>
    Vector3 FindValidSpawnPosition()
    {
        Vector3 playerPos = player.position;
        float playerSpeed = playerRigidbody != null ? playerRigidbody.linearVelocity.magnitude : 0f;
        
        for (int attempts = 0; attempts < maxSpawnAttempts; attempts++)
        {
            float angle;
            
            // Apply directional bias if player is moving fast enough
            if (playerSpeed >= minSpeedForBias && forwardBias > 0f)
            {
                angle = GetBiasedSpawnAngle();
            }
            else
            {
                // Random angle if player is stationary or bias is disabled
                angle = Random.Range(0f, 360f) * Mathf.Deg2Rad;
            }
            
            // Generate distance
            float distance = Random.Range(minSpawnDistance, maxSpawnDistance);
            
            // Calculate potential spawn position
            Vector3 spawnPos = playerPos + new Vector3(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance,
                0f
            );
            
            // Check if position is valid (not too close to other seeds)
            if (IsValidSpawnPosition(spawnPos))
            {
                return spawnPos;
            }
        }
        
        return Vector3.zero; // Failed to find valid position
    }

    /// <summary>
    /// Get a spawn angle biased toward the player's movement direction
    /// </summary>
    float GetBiasedSpawnAngle()
    {
        // Get the angle of player's movement direction
        float movementAngle = Mathf.Atan2(playerMovementDirection.y, playerMovementDirection.x);
        
        // Decide whether to spawn ahead or randomly
        if (Random.value < forwardBias)
        {
            // Spawn ahead: within the forward cone
            float halfCone = forwardSpawnAngle * 0.5f * Mathf.Deg2Rad;
            float randomOffset = Random.Range(-halfCone, halfCone);
            return movementAngle + randomOffset;
        }
        else
        {
            // Spawn randomly for some variety
            return Random.Range(0f, 360f) * Mathf.Deg2Rad;
        }
    }

    /// <summary>
    /// Check if a spawn position is valid (not too close to existing seeds)
    /// </summary>
    bool IsValidSpawnPosition(Vector3 position)
    {
        // Check against active seeds
        foreach (GameObject activeSeed in activeSeeds)
        {
            if (activeSeed.activeInHierarchy)
            {
                float distance = Vector3.Distance(position, activeSeed.transform.position);
                if (distance < minSeedDistance)
                {
                    return false; // Too close to existing seed
                }
            }
        }
        
        // Check against falling seeds' target positions
        foreach (SeedFallData fallData in fallingSeeds)
        {
            if (fallData.isActive)
            {
                float distance = Vector3.Distance(position, fallData.targetPosition);
                if (distance < minSeedDistance)
                {
                    return false; // Too close to falling seed's target
                }
            }
        }
        
        return true; // Position is valid
    }

    /// <summary>
    /// Handle cleanup of seeds that are too far from the player
    /// </summary>
    void HandleCleanup()
    {
        Vector3 playerPos = player.position;
        
        // Check active seeds for cleanup (iterate backwards to safely remove)
        for (int i = activeSeeds.Count - 1; i >= 0; i--)
        {
            GameObject seed = activeSeeds[i];
            
            if (seed == null || !seed.activeInHierarchy)
            {
                // Seed was collected or destroyed, remove from active list
                activeSeeds.RemoveAt(i);
                continue;
            }
            
            float distance = Vector3.Distance(playerPos, seed.transform.position);
            
            if (distance > despawnDistance)
            {
                // Seed is too far, return to pool
                ReturnSeedToPool(seed);
                activeSeeds.RemoveAt(i);
                Debug.Log($"Despawned far seed. Active seeds: {activeSeeds.Count}");
            }
        }
        
        // Check falling seeds for cleanup
        for (int i = fallingSeeds.Count - 1; i >= 0; i--)
        {
            SeedFallData fallData = fallingSeeds[i];
            
            if (fallData.seed != null)
            {
                float distance = Vector3.Distance(playerPos, fallData.seed.transform.position);
                
                if (distance > despawnDistance)
                {
                    // Falling seed is too far, return to pool
                    ReturnSeedToPool(fallData.seed);
                    fallingSeeds.RemoveAt(i);
                    Debug.Log("Despawned far falling seed");
                }
            }
        }
    }

    /// <summary>
    /// Get an inactive seed from the pool
    /// </summary>
    GameObject GetPooledSeed()
    {
        foreach (GameObject seed in seedPool)
        {
            if (!seed.activeInHierarchy)
            {
                return seed;
            }
        }
        
        Debug.LogWarning("SeedSpawner: No available seeds in pool!");
        return null;
    }

    /// <summary>
    /// Return a seed to the pool (make it inactive and available for reuse)
    /// </summary>
    void ReturnSeedToPool(GameObject seed)
    {
        seed.SetActive(false);
        seed.transform.SetParent(transform); // Re-organize under spawner
    }

    /// <summary>
    /// Public method for seeds to call when they're collected
    /// </summary>
    public void OnSeedCollected(GameObject seed)
    {
        // Remove from active seeds
        if (activeSeeds.Contains(seed))
        {
            activeSeeds.Remove(seed);
            Debug.Log($"Seed collected. Active seeds: {activeSeeds.Count}");
        }
        
        // Remove from falling seeds if it was falling
        for (int i = fallingSeeds.Count - 1; i >= 0; i--)
        {
            if (fallingSeeds[i].seed == seed)
            {
                fallingSeeds.RemoveAt(i);
                Debug.Log("Collected falling seed");
                break;
            }
        }
    }

    /// <summary>
    /// Manually spawn a seed (useful for testing or special events)
    /// </summary>
    [ContextMenu("Force Spawn Seed")]
    public void ForceSpawnSeed()
    {
        int totalActiveSeeds = activeSeeds.Count + fallingSeeds.Count;
        if (totalActiveSeeds < maxActiveSeeds)
        {
            TrySpawnSeed();
        }
        else
        {
            Debug.Log("Cannot spawn seed: Max active seeds reached");
        }
    }

    // Debug visualization in Scene view
    void OnDrawGizmosSelected()
    {
        if (player == null) return;

        Vector3 playerPos = player.position;

        // Draw spawn range circles
        Gizmos.color = Color.green;
        DrawWireCircle2D(playerPos, minSpawnDistance);
        
        Gizmos.color = Color.yellow;
        DrawWireCircle2D(playerPos, maxSpawnDistance);
        
        // Draw despawn range
        Gizmos.color = Color.red;
        DrawWireCircle2D(playerPos, despawnDistance);
        
        // Draw movement direction and forward spawn cone
        if (Application.isPlaying)
        {
            Gizmos.color = Color.magenta;
            Vector3 directionEnd = playerPos + (Vector3)playerMovementDirection * (minSpawnDistance + 2f);
            Gizmos.DrawLine(playerPos, directionEnd);
            Gizmos.DrawSphere(directionEnd, 0.5f);
            
            // Draw forward spawn cone
            Gizmos.color = Color.cyan;
            float halfCone = forwardSpawnAngle * 0.5f * Mathf.Deg2Rad;
            float movementAngle = Mathf.Atan2(playerMovementDirection.y, playerMovementDirection.x);
            
            Vector3 coneLeft = playerPos + new Vector3(
                Mathf.Cos(movementAngle - halfCone) * maxSpawnDistance,
                Mathf.Sin(movementAngle - halfCone) * maxSpawnDistance,
                0f
            );
            
            Vector3 coneRight = playerPos + new Vector3(
                Mathf.Cos(movementAngle + halfCone) * maxSpawnDistance,
                Mathf.Sin(movementAngle + halfCone) * maxSpawnDistance,
                0f
            );
            
            Gizmos.DrawLine(playerPos, coneLeft);
            Gizmos.DrawLine(playerPos, coneRight);
            
            // Draw falling seed paths
            Gizmos.color = Color.white;
            foreach (SeedFallData fallData in fallingSeeds)
            {
                if (fallData.isActive && fallData.seed != null)
                {
                    Vector3 seedPos = fallData.seed.transform.position;
                    Gizmos.DrawLine(seedPos, fallData.targetPosition);
                    Gizmos.DrawSphere(fallData.targetPosition, 0.3f);
                }
            }
        }
        
        // Draw seed spacing circles
        Gizmos.color = Color.blue;
        if (activeSeeds != null)
        {
            foreach (GameObject seed in activeSeeds)
            {
                if (seed != null && seed.activeInHierarchy)
                {
                    DrawWireCircle2D(seed.transform.position, minSeedDistance);
                }
            }
        }
    }

    /// <summary>
    /// Helper method to draw a 2D wire circle using Gizmos
    /// </summary>
    void DrawWireCircle2D(Vector3 center, float radius)
    {
        int segments = 32;
        float angleStep = 360f / segments;
        
        Vector3 prevPoint = center + new Vector3(radius, 0, 0);
        
        for (int i = 1; i <= segments; i++)
        {
            float angle = i * angleStep * Mathf.Deg2Rad;
            Vector3 newPoint = center + new Vector3(
                Mathf.Cos(angle) * radius,
                Mathf.Sin(angle) * radius,
                0
            );
            
            Gizmos.DrawLine(prevPoint, newPoint);
            prevPoint = newPoint;
        }
    }
}
