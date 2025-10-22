using UnityEngine;

public class SeedPowerUp : MonoBehaviour
{
    [Header("Seed Settings")]
    [Tooltip("Optional: Sound effect to play when collected.")]
    public AudioSource collectSound;
    
    [Tooltip("Optional: Particle effect to spawn when collected.")]
    public GameObject collectParticles;

    private SeedSpawner spawner;

    void Start()
    {
        // Find the spawner in the scene using the new Unity method
        spawner = FindFirstObjectByType<SeedSpawner>();
        
        if (spawner == null)
        {
            Debug.LogWarning("SeedPowerUp: Could not find SeedSpawner in scene!");
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            // Apply seed boost to player
            player.ApplySeedBoost();
            
            // Play sound effect if assigned
            if (collectSound != null)
            {
                collectSound.Play();
            }
            
            // Spawn particles if assigned
            if (collectParticles != null)
            {
                Instantiate(collectParticles, transform.position, transform.rotation);
            }
            
            // Notify spawner that this seed was collected
            if (spawner != null)
            {
                spawner.OnSeedCollected(gameObject);
            }
            
            // Deactivate instead of destroy (returns to pool)
            gameObject.SetActive(false);
        }
    }
}
