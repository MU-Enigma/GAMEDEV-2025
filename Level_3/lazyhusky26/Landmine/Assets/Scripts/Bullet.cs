using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 15f;
    public float lifeTime = 2f;
    public int damage = 1;

    Vector2 velocity;
    float spawnTime;

    // Configure the bullet when spawned
    public void Initialize(Vector2 direction, float speedMultiplier = 1f)
    {
        velocity = direction.normalized * speed * speedMultiplier;
        spawnTime = Time.time;
        gameObject.SetActive(true);
    }

    void OnEnable()
    {
        spawnTime = Time.time;
    }

    void Update()
    {
        // Simple kinematic movement
        transform.position += (Vector3)(velocity * Time.deltaTime);

        if (Time.time - spawnTime > lifeTime)
            gameObject.SetActive(false); // return to pool
    }

    // If using trigger for hits
    void OnTriggerEnter2D(Collider2D other)
    {
        // Example: if it hits an Enemy with "Enemy" tag -> deal damage, deactivate
        if (other.CompareTag("Enemy"))
        {
            var health = other.GetComponent<Health>(); // optional Health script
            if (health != null) health.TakeDamage(damage);
            gameObject.SetActive(false);
        }
        else if (!other.CompareTag("Player")) // don't collide with player
        {
            // Hit wall/other -> deactivate
            gameObject.SetActive(false);
        }
    }
}
