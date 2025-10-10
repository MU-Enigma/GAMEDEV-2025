using UnityEngine;

public class ChainDashUpgrade : MonoBehaviour
{
    [Header("Chain Dash Upgrade Settings")]
    [Tooltip("Optional: Sound effect to play when collected.")]
    public AudioSource collectSound;
    
    [Tooltip("Optional: Particle effect to spawn when collected.")]
    public GameObject collectParticles;

    [Tooltip("Should this upgrade be destroyed even if player is at max level?")]
    public bool destroyEvenIfMaxLevel = true;

    [Header("Bobbing Settings")]
    [Tooltip("How fast the upgrade bobs up and down.")]
    public float bobSpeed = 2.5f;
    [Tooltip("Height of the bobbing animation.")]
    public float bobHeight = 0.3f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        // Bobbing animation
        float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPosition + new Vector3(0, yOffset, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            // Check if player can still upgrade
            if (player.CanUpgradeChainDash())
            {
                // Apply chain dash upgrade to player
                player.UpgradeChainDash();
                
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
                
                // Remove upgrade from world
                Destroy(gameObject);
            }
            else
            {
                // Player is already at max level
                Debug.Log("Chain Dash already at maximum level!");
                
                if (destroyEvenIfMaxLevel)
                {
                    // Optional: Still play sound/particles for feedback
                    if (collectSound != null)
                    {
                        collectSound.Play();
                    }
                    
                    Destroy(gameObject);
                }
            }
        }
    }
}
