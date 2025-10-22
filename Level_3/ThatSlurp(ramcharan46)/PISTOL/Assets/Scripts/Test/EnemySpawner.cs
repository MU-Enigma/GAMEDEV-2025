using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject enemyPrefab;
    public float spawnRate = 3f;
    public float spawnRadius = 8f;
    public int maxEnemies = 4;
    
    [Header("Top-Down Spawning")]
    public float minPlayerDistance = 4f;
    public float enemySpacing = 2f;
    public int maxSpawnAttempts = 20;
    
    [Header("Spawn Area Constraints")]
    public bool useSpawnBounds = true;
    public Vector2 spawnAreaSize = new Vector2(20f, 15f);
    public LayerMask obstacleLayer = 1;
    public float obstacleCheckRadius = 0.8f;
    
    [Header("Advanced Settings")]
    public bool spawnAroundPlayer = false;
    public bool avoidSpawningInView = true;
    public float viewAvoidanceRadius = 6f;
    
    private float timer;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private Transform player;
    private Camera playerCamera;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        if (player == null)
        {
            Debug.LogWarning("Player not found! Enemy spawner may not work correctly.");
        }
        
        playerCamera = Camera.main;
        if (playerCamera == null && player != null)
        {
            playerCamera = FindAnyObjectByType<Camera>();
        }
    }

    void Update()
    {
        spawnedEnemies = spawnedEnemies.Where(enemy => enemy != null).ToList();

        timer -= Time.deltaTime;
        if (timer <= 0)
        {
            if (spawnedEnemies.Count < maxEnemies)
            {
                SpawnEnemy();
            }
            timer = spawnRate;
        }
    }

    void SpawnEnemy()
    {
        Vector2? validSpawnPos = FindValidSpawnPosition();
        
        if (validSpawnPos.HasValue)
        {
            GameObject newEnemy = Instantiate(enemyPrefab, validSpawnPos.Value, Quaternion.identity);
            
            spawnedEnemies.Add(newEnemy);
            
            Debug.Log($"Enemy spawned at {validSpawnPos.Value}");
        }
        else
        {
            Debug.LogWarning("Could not find valid spawn position for enemy after " + maxSpawnAttempts + " attempts");
        }
    }
    
    Vector2? FindValidSpawnPosition()
    {
        Vector2 centerPoint = spawnAroundPlayer && player != null ? player.position : transform.position;
        
        for (int attempt = 0; attempt < maxSpawnAttempts; attempt++)
        {
            Vector2 randomDirection = Random.insideUnitCircle.normalized;
            float spawnDistance = Random.Range(minPlayerDistance, spawnRadius);
            Vector2 testPosition = centerPoint + randomDirection * spawnDistance;
            
            if (useSpawnBounds && !IsWithinSpawnBounds(testPosition))
            {
                continue;
            }
            
            if (player != null && Vector2.Distance(testPosition, player.position) < minPlayerDistance)
            {
                continue;
            }
            
            if (avoidSpawningInView && IsPositionInPlayerView(testPosition))
            {
                continue;
            }
            
            if (IsPositionObstructed(testPosition))
            {
                continue;
            }
            
            if (!IsPositionClearOfEnemies(testPosition, enemySpacing))
            {
                continue;
            }
            
            return testPosition;
        }
        
        return null;
    }
    
    bool IsWithinSpawnBounds(Vector2 position)
    {
        Vector2 spawnerPos = transform.position;
        Vector2 halfSize = spawnAreaSize * 0.5f;
        
        return position.x >= spawnerPos.x - halfSize.x && 
               position.x <= spawnerPos.x + halfSize.x &&
               position.y >= spawnerPos.y - halfSize.y && 
               position.y <= spawnerPos.y + halfSize.y;
    }
    
    bool IsPositionObstructed(Vector2 position)
    {
        Collider2D obstacleCheck = Physics2D.OverlapCircle(position, obstacleCheckRadius, obstacleLayer);
        return obstacleCheck != null;
    }
    
    bool IsPositionInPlayerView(Vector2 position)
    {
        if (player == null || playerCamera == null) return false;
        
        float distanceToPlayer = Vector2.Distance(position, player.position);
        return distanceToPlayer < viewAvoidanceRadius;
    }
    
    bool IsPositionClearOfEnemies(Vector2 position, float clearRadius)
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null && Vector2.Distance(position, enemy.transform.position) < clearRadius)
            {
                return false;
            }
        }
        return true;
    }
    
    void SpawnEnemyAtSpecificLocation(Vector2 position)
    {
        if (IsPositionObstructed(position) || !IsPositionClearOfEnemies(position, enemySpacing))
        {
            Debug.LogWarning($"Cannot spawn enemy at {position} - position is obstructed or too close to other enemies");
            return;
        }
        
        GameObject newEnemy = Instantiate(enemyPrefab, position, Quaternion.identity);
        spawnedEnemies.Add(newEnemy);
        Debug.Log($"Enemy manually spawned at {position}");
    }
    
    public void SpawnEnemyFormation(Vector2 centerPoint, int enemyCount, float formationRadius)
    {
        for (int i = 0; i < enemyCount && spawnedEnemies.Count < maxEnemies; i++)
        {
            float angle = (360f / enemyCount) * i * Mathf.Deg2Rad;
            Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * formationRadius;
            Vector2 spawnPos = centerPoint + offset;
            
            if (!IsPositionObstructed(spawnPos) && IsPositionClearOfEnemies(spawnPos, enemySpacing))
            {
                SpawnEnemyAtSpecificLocation(spawnPos);
            }
        }
    }

    public void LogSpawnStatistics()
    {
        Debug.Log($"Enemy Spawner Statistics:");
        Debug.Log($"- Active Enemies: {spawnedEnemies.Count}/{maxEnemies}");
        Debug.Log($"- Spawn Rate: {spawnRate}s");
        Debug.Log($"- Spawn Radius: {spawnRadius}");
        Debug.Log($"- Min Player Distance: {minPlayerDistance}");
    }
    
    public void ForceSpawnEnemy()
    {
        Vector2? validSpawnPos = FindValidSpawnPosition();
        
        if (validSpawnPos.HasValue)
        {
            GameObject newEnemy = Instantiate(enemyPrefab, validSpawnPos.Value, Quaternion.identity);
            spawnedEnemies.Add(newEnemy);
            Debug.Log($"Force spawned enemy at {validSpawnPos.Value}");
        }
    }
    
    public void ClearAllEnemies()
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();
    }
    
}