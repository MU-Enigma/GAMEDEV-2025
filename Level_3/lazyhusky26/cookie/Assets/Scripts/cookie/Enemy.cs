// Enemy.cs
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public GameObject deathVFXPrefab;
    public float deathVFXDuration = 1f;
    public bool destroyRoot = true; // if enemy parts under root, destroy root

    public void Die()
    {
        // instantiate VFX at center of sprite
        if (deathVFXPrefab != null)
        {
            GameObject v = Instantiate(deathVFXPrefab, transform.position, Quaternion.identity);
            // If the VFX is self-destructing via ParticleSystem duration, that's fine.
            Destroy(v, deathVFXDuration + 0.1f);
        }

        // Play death animation or sound here (optional)

        // Destroy or disable enemy
        if (destroyRoot && transform.parent != null)
            Destroy(transform.parent.gameObject);
        else
            Destroy(gameObject);
    }
}
