using UnityEngine;

public class BulletSpawning : MonoBehaviour
{
    [Header("Bullet Setup")]
    public GameObject bulletPrefab; // Reference to the bullet prefab
    public Transform firePoint; // The point from which the bullet will be fired

    [Header("Audio")]
    [Tooltip("The sound clip to play when a bullet is successfully fired.")]
    public AudioClip fireSound;
    private AudioSource audioSource; // Component to play the sound

    // This is a simplified way to track direction, based on movement
    private Vector2 lastDirection = Vector2.up; // Default direction
    
    // Reference to the PlayerController for seed/ammo management
    private PlayerController playerController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get references on this GameObject
        playerController = GetComponent<PlayerController>();
        audioSource = GetComponent<AudioSource>();
        
        if (playerController == null)
        {
            Debug.LogError("BulletSpawning requires a PlayerController component on the same GameObject!");
        }
        
        if (audioSource == null)
        {
            // Auto-add AudioSource if none exists, to ensure sound capability
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (playerController == null) return;
        
        // Get player input
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        // Update the last known direction if there's movement
        if (moveX != 0 || moveY != 0)
        {
            lastDirection = new Vector2(moveX, moveY).normalized;
        }

        // Fire the projectile on button press
        if (Input.GetMouseButtonDown(0)) // Check if the left mouse button is pressed
        {
            // Check if we have seeds (ammo) and consume one
            if (playerController.TryConsumeSeed())
            {
                // 1. Play the firing sound
                if (audioSource != null && fireSound != null)
                {
                    audioSource.PlayOneShot(fireSound);
                }

                // 2. Instantiate the bullet since ammo check passed
                GameObject newProjectile = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
                
                // 3. Set the bullet's direction
                BulletFiring bulletFiring = newProjectile.GetComponent<BulletFiring>();
                if (bulletFiring != null)
                {
                    bulletFiring.SetDirection(lastDirection);
                }
                else
                {
                    Debug.LogWarning("Bullet prefab is missing the BulletFiring script!");
                }
            }
            else
            {
                Debug.Log("Out of ammo (seeds)!");
                // OPTIONAL: Play a "click" or "empty" sound here
            }
        }
    }
}
