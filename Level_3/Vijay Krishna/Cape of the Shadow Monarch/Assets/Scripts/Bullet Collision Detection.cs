using UnityEngine;

public class BulletCollisionDetection : MonoBehaviour
{
    public GameObject Blood; // Reference to the blood prefab
    public Transform firePoint; // The point from which the bullet will be fired
    public int damage = 0; // Damage value

    private void OnTriggerEnter2D(Collider2D other)
    {
        // --- THIS IS THE FIX ---
        // The check for "Shadow Ally" has been removed.
        // Now this script will ONLY react to objects tagged "Bullet".
        if (other.CompareTag("Bullet"))
        {
            Instantiate(Blood, firePoint.position, firePoint.rotation);
            Destroy(other.gameObject); // Destroy the bullet on collision
            
            // Apply damage from the bullet
            ApplyDamage(1);
        } else if (other.CompareTag("Shadow Ally"))
        {
            // If the bullet hits a shadow ally, apply damage
            Instantiate(Blood, firePoint.position, firePoint.rotation);
            
            // Apply damage from the bullet
            ApplyDamage(1);
        }
    }

    /// <summary>
    /// Public function that can be called by other scripts (like the Ally)
    /// </summary>
    public void TakeDamage(float amount)
    {
        // Apply damage from the ally's attack
        ApplyDamage((int)amount);
    }

    /// <summary>
    /// A central function to handle applying damage and checking for death.
    /// </summary>
    private void ApplyDamage(int amount)
    {
        damage += amount; 

        if (damage >= 2)
        {
            Debug.Log("Damage dealt: " + damage + ". Enemy killed!");
            
            // Tell the ScoreManager we got a kill
            if (ScoreManager.instance != null)
            {
                ScoreManager.instance.AddKill();
            }
            
            Destroy(gameObject); // Destroy this enemy
        }
        else
        {
            Debug.Log("Damage dealt: " + damage);
        }
    }
}

