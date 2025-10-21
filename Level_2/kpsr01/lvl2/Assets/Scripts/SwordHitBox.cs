using UnityEngine;

public class SwordHitbox : MonoBehaviour
{
    // This special Unity function is called automatically 
    // when our sword's trigger collider enters another collider.
    void OnTriggerEnter2D(Collider2D other)
    {
        // 'other' is the collider we hit.
        // We check if the object we hit has the "Enemy" tag.
        if (other.CompareTag("Enemy"))
        {
            // If it is an enemy, print a message to the console.
            Debug.Log("Hit the enemy: " + other.name);
            
            // --- This is where you would add JUICE (Task 4) ---
            // e.g., other.GetComponent<EnemyHealth>().TakeDamage(10);
            // e.g., Play a particle effect
            // e.g., Trigger screen shake
        }
    }
}