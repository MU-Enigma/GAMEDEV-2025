using UnityEngine;

public class SnowballProjectile : MonoBehaviour
{
    public GameObject hitVFXPrefab;
    public float lifetime = 5f;

    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore hitting the player or other snowballs
        if (other.CompareTag("Player") || other.CompareTag("Projectile"))
            return;

        // Spawn hit effect
        if (hitVFXPrefab != null)
            Instantiate(hitVFXPrefab, transform.position, Quaternion.identity);

        // If we hit an enemy
        if (other.CompareTag("Enemy"))
        {
            Enemy e = other.GetComponent<Enemy>();
            if (e != null)
                e.Die();
        }

        Destroy(gameObject);
    }
}
