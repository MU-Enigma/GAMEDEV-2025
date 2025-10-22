using UnityEngine;

// This script should be named "BulletFiring.cs" if your file is named that.
// The class name must match the file name.
[RequireComponent(typeof(Rigidbody2D))]
public class BulletFiring : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 1f;

    // --- NEW: Reference to the Rigidbody ---
    private Rigidbody2D rb;
    
    // --- FIX: Declared the missing 'direction' variable ---
    private Vector2 direction;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        // Make sure gravity doesn't affect the bullet
        if (rb != null)
        {
            rb.gravityScale = 0;
        }

        // Destroy the bullet after its lifetime expires
        Destroy(gameObject, lifeTime);
    }

    // This method is called by BulletSpawning.cs
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized; // Normalize the vector to ensure consistent speed

        // --- SPRITE ROTATION LOGIC (REMOVED) ---
        // Your 'BulletSpawning.cs' script *already* sets the correct rotation
        // when it instantiates the bullet. We don't need to do it again here.

        // --- NEW: Set velocity ---
        // We get the Rigidbody (if not already set in Awake)
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0;
        }
        
        // This is the correct way to move a physics object.
        // We set its velocity once, and the physics engine handles the movement.
        rb.linearVelocity = direction * speed;
    }

    // --- 'Update()' method removed ---
    // We no longer need Update() because the physics engine
    // is handling the movement for us via rb.velocity.

    // Optional: Add collision logic if needed
    // This will destroy the bullet if it hits *anything*
    // Note: 'Bullet Collision Detection.cs' handles the *enemy* side
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // To prevent the bullet from destroying itself *on the player*
        // when it spawns, add a tag check.
        if (!collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}

