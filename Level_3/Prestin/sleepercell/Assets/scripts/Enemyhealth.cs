using UnityEngine;

public class EnemyHealth2D : MonoBehaviour
{
    [Header("Health")]
    public float health = 50f;

    public void TakeDamage(float amount)
    {
        health -= amount;

        if (health <= 0f)
        {
            Die();
        }
        else
        {
            Debug.Log(gameObject.name + " took " + amount + " damage. " + health + " health remaining.");
          
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " has died!");
        
        
        
        Destroy(gameObject);
    }
}
