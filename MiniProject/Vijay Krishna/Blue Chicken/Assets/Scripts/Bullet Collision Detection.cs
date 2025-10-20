using UnityEngine;
using System.Collections; // Required for Coroutines

public class BulletCollisionDetection : MonoBehaviour
{
    [Header("Visuals & Damage")]
    public GameObject Blood; // Reference to the blood prefab
    public Transform firePoint; // The point from which the bullet will be fired
    public int damage = 0; // Damage value
    [Tooltip("Time in seconds the enemy stays visible after death before being destroyed.")]
    public float deathDelay = 0.5f; // New delay variable
    
    [Header("Audio")]
    [Tooltip("The sound clip to play when the enemy takes damage.")]
    public AudioClip damageSound;
    [Tooltip("The sound clip to play when the enemy dies.")]
    public AudioClip deathSound;
    [Tooltip("Volume multiplier for the death sound.")]
    [Range(0.0f, 3.0f)]
    public float deathSoundVolume = 1.0f; 
    
    // Component references
    private AudioSource audioSource;
    private Collider2D enemyCollider;
    private Renderer enemyRenderer;
    
    // Spawner reference for death notification
    private EnemySpawner enemySpawner; // Used to notify the spawner of death

    void Start()
    {
        // Find the spawner instance in the scene
        // NOTE: This assumes an EnemySpawner object is present in the scene.
        enemySpawner = FindFirstObjectByType<EnemySpawner>();

        // Get or add AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
        // Set the AudioSource to 2D (Spatial Blend = 0) for reliable damage sound playback
        audioSource.spatialBlend = 0f;

        // Get other necessary components
        enemyCollider = GetComponent<Collider2D>();
        enemyRenderer = GetComponent<Renderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("Bullet has entered the trigger zone.");
            
            // Instantiate the blood effect
            Instantiate(Blood, firePoint.position, firePoint.rotation);
            
            // Destroy the bullet on collision
            Destroy(other.gameObject); 
            
            damage += 1; // Increment damage by 1 on each hit
            
            // Check for death condition
            if (damage >= 3)
            {
                Debug.Log("Damage dealt: " + damage + ". Starting death sequence.");
                
                // Start the delayed destruction routine
                StartCoroutine(DeathSequence());
            }
            else
            {
                Debug.Log("Damage dealt: " + damage);
                
                // Play damage sound using the local 2D AudioSource
                if (audioSource != null && damageSound != null)
                {
                    audioSource.PlayOneShot(damageSound);
                }
            }
        }
    }
    
    // Coroutine for delayed destruction
    IEnumerator DeathSequence()
    {
        // 1. Play the death sound instantly and reliably (must happen before destruction)
        if (deathSound != null)
        {
            // Use the reliable PlayClip2D helper embedded in this class
            PlayClip2D(deathSound, deathSoundVolume);
        }

        // 2. Immediately stop collision and hide visuals/stop movement
        if (enemyCollider != null)
        {
            enemyCollider.enabled = false; // Prevents further hits
        }
        if (enemyRenderer != null)
        {
            enemyRenderer.enabled = false; // Hide the sprite
        }
        
        // 3. Notify the spawner that this enemy is "dead"
        if (enemySpawner != null)
        {
            enemySpawner.OnEnemyKilled();
        }

        // 4. Wait for the specified delay time
        yield return new WaitForSeconds(deathDelay);

        // 5. Final destruction
        Destroy(gameObject);
    }
    
    /// <summary>
    /// Creates a temporary, self-destroying GameObject to play a sound clip in 2D space.
    /// This is embedded here to guarantee the death sound works without a separate file.
    /// </summary>
    /// <param name="clip">The AudioClip to play.</param>
    /// <param name="volume">The volume level.</param>
    public static void PlayClip2D(AudioClip clip, float volume = 1f)
    {
        if (clip == null) return;

        // 1. Create a temporary GameObject
        GameObject tempGO = new GameObject("OneShotAudio_2D");
        
        // 2. Add an AudioSource component
        AudioSource tempAudioSource = tempGO.AddComponent<AudioSource>();
        
        // 3. Configure for reliable, full-volume 2D playback
        tempAudioSource.spatialBlend = 0f; // Force 2D sound
        tempAudioSource.volume = volume;
        tempAudioSource.playOnAwake = false;

        // 4. Assign the clip and play
        tempAudioSource.clip = clip;
        tempAudioSource.Play();

        // 5. Destroy the temporary GameObject after the clip finishes
        GameObject.Destroy(tempGO, clip.length);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("Bullet has exited the trigger zone.");
        }
    }
}
