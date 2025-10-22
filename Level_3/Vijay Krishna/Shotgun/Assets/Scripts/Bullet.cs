using UnityEngine;

// This script should be named "BulletFiring.cs" if your file is named that.
// The class name must match the file name.
[RequireComponent(typeof(Rigidbody2D))]
public class BulletFiring : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 1f;

    // --- Reference to the Rigidbody ---
    private Rigidbody2D rb;
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
        // This is the fix.
        // Your 'BulletSpawning.cs' script (in the canvas) *already* // sets the correct rotation. We must not overwrite it here.

        // --- Set velocity ---
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0;
        }
        
        // This is the correct way to move a physics object.
        rb.linearVelocity = direction * speed;
    }

    // Optional: Add collision logic if needed
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Prevent the bullet from destroying itself on the player
        if (!collision.gameObject.CompareTag("Player"))
        {
            Destroy(gameObject);
        }
    }
}

