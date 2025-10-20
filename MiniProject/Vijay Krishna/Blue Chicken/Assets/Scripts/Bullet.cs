using UnityEngine;

public class BulletFiring : MonoBehaviour
{
    public float speed = 20f;
    public float lifeTime = 1f;
    private Vector2 direction;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Destroy the bullet after its lifetime expires
        Destroy(gameObject, lifeTime); 
    }

    // Call this method when the projectile is created or "fired"
    public void SetDirection(Vector2 newDirection)
    {
        direction = newDirection.normalized; // Normalize the vector to ensure consistent speed

        // --- SPRITE ROTATION LOGIC ---
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle - 90f));
        // --- END SPRITE ROTATION LOGIC ---
    }

    // Update is called once per frame
    void Update()
    {
        // Move the projectile in the stored direction
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }
}
