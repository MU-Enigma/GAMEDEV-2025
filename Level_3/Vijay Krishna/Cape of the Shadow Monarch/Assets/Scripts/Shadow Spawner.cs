using UnityEngine;
using System;

public class AllySpawner : MonoBehaviour
{
    [Header("Spawning")]
    public GameObject allyPrefab; // The ally prefab to spawn
    public Transform spawnPoint;  // Where to spawn the ally
    public KeyCode spawnKey = KeyCode.E; // Key to press to spawn

    private int currentAllyCount = 0;
    private int maxAllies = 0; // This will be set by the ScoreManager

    void OnEnable()
    {
        // Subscribe to the kill count event from ScoreManager
        ScoreManager.OnKillsChanged += UpdateMaxAllies;
        
        // Subscribe to the ally death event from the Ally script
        Ally.OnAllyDied += DecrementAllyCount;
    }

    void OnDisable()
    {
        // Unsubscribe from events when this object is disabled/destroyed
        ScoreManager.OnKillsChanged -= UpdateMaxAllies;
        Ally.OnAllyDied -= DecrementAllyCount;
    }

    void Start()
    {
        // If the spawn point isn't set, default to this object's transform
        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }

        // Get the initial kill count just in case we started with kills
        if (ScoreManager.instance != null)
        {
            UpdateMaxAllies(ScoreManager.instance.GetKillCount());
        }
    }

    void Update()
    {
        // Check if the player presses the spawn key
        if (Input.GetKeyDown(spawnKey))
        {
            SpawnAlly();
        }
    }

    void SpawnAlly()
    {
        if (allyPrefab == null)
        {
            Debug.LogWarning("Ally Spawner: No Ally Prefab assigned!");
            return;
        }

        // Check if we are allowed to spawn another ally
        if (currentAllyCount < maxAllies)
        {
            // Spawn the ally at the spawn point
            Instantiate(allyPrefab, spawnPoint.position, spawnPoint.rotation);
            
            // Increment our local count
            currentAllyCount++;
            
            Debug.Log($"Ally spawned! Current allies: {currentAllyCount}/{maxAllies}");
        }
        else
        {
            Debug.Log($"Cannot spawn ally. Max allies reached: {currentAllyCount}/{maxAllies}");
        }
    }

    // This method is called automatically by the ScoreManager event
    void UpdateMaxAllies(int newKillCount)
    {
        maxAllies = newKillCount;
        Debug.Log("Max allies updated to: " + maxAllies);
    }

    // This method is called automatically by the Ally.OnAllyDied event
    void DecrementAllyCount()
    {
        currentAllyCount--;
        if (currentAllyCount < 0)
        {
            currentAllyCount = 0;
        }
        Debug.Log("An ally died. Current allies: " + currentAllyCount);
    }
}
