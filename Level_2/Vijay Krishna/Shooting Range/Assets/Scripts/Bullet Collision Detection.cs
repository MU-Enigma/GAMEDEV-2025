using UnityEngine;

public class BulletCollisionDetection : MonoBehaviour
{
    public GameObject Blood; // Reference to the blood prefab
    public Transform firePoint; // The point from which the bullet will be fired
    public int damage = 0; // Damage value
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("Bullet has entered the trigger zone.");
            Instantiate(Blood, firePoint.position, firePoint.rotation);
            Destroy(other.gameObject); // Destroy the bullet on collision
            damage += 1; // Increment damage by 1 on each hit
            if (damage == 0)
            {
                Debug.Log("No damage dealt.");
            }
            else if (damage == 3)
            {
                Debug.Log("Damage dealt: " + damage);
                    Destroy(gameObject); // Destroy the target after 3 hits
                }
                else
                {
                    Debug.Log("Damage dealt: " + damage);
                }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("Bullet has exited the trigger zone.");
        }
    }
}