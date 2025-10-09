using UnityEngine;
using System.Collections;

/// <summary>
/// This thing spawns the speed boost orbs. I refactored the old respawn logic because it was
/// giving an aneurysm. Instead of having every orb run its own "am i dead yet?" coroutine,
/// the orb now tells the spawner when it dies, and the spawner handles respawning.
/// </summary>
public class OrbSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public GameObject orbPrefab;
    public int maxOrbs = 10;
    public float spawnRadius = 20f;
    public Vector2 spawnCenter = Vector2.zero; // usually the center of the map
    public float respawnDelay = 5f;

    [Header("Background Constraint (Optional)")]
    public SpriteRenderer backgroundImage; // drag your background sprite here to keep orbs inside it
    public float edgePadding = 1f; // how far from the edge to spawn

    private int _currentOrbCount;
    private Bounds _backgroundBounds;

    void Start()
    {
        if (orbPrefab == null)
        {
            Debug.LogError("YOU FORGOT TO ASSIGN THE ORB PREFAB. THE SPAWNER IS USELESS. I QUIT.");
            // another beauty of a log message. i know, i'm the GOAT.
            return;
        }

        // if a background is assigned, calculate its boundaries.
        if (backgroundImage != null) _backgroundBounds = backgroundImage.bounds;

        // spawn the first batch of orbs
        for (int i = 0; i < maxOrbs; i++) SpawnOrb();
    }
    
    /// <summary>
    /// This is called by an orb right before it destroys itself.
    /// </summary>
    public void OnOrbCollected()
    {
        _currentOrbCount--;
        // start a timer to spawn a new one to replace the one that was just collected.
        StartCoroutine(RespawnOrbCoroutine());
    }

    private void SpawnOrb()
    {
        //uhh how the fuck did you achieve this??????
        if (_currentOrbCount >= maxOrbs) return;

        Vector2 spawnPosition;

        // if we have a background, find a random spot inside it. otherwise, just use the radius.
        if (backgroundImage != null)
        {
            float minX = _backgroundBounds.min.x + edgePadding;
            float maxX = _backgroundBounds.max.x - edgePadding;
            float minY = _backgroundBounds.min.y + edgePadding;
            float maxY = _backgroundBounds.max.y - edgePadding;
            spawnPosition = new Vector2(Random.Range(minX, maxX), Random.Range(minY, maxY));
        }
        else spawnPosition = spawnCenter + Random.insideUnitCircle * spawnRadius;

        // create the orb...
        GameObject orbGO = Instantiate(orbPrefab, spawnPosition, Quaternion.identity);
        // ...get its script...
        SpeedBoostOrb orbScript = orbGO.GetComponent<SpeedBoostOrb>();
        // ...and tell it that I AM ITS FATHER. (so it can report back to me when it dies)
        orbScript.SetSpawner(this);
        
        _currentOrbCount++;
    }

    // this is the new, much smarter respawn logic. one timer starts when an orb dies. that's it.
    private IEnumerator RespawnOrbCoroutine()
    {
        yield return new WaitForSeconds(respawnDelay);
        SpawnOrb();
    }
}
