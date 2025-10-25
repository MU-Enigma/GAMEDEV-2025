using UnityEngine;

public class BulletCollisionDetection : MonoBehaviour
{
    public GameObject Blood; // Reference to the blood prefab
    public Transform firePoint; // The point from which the bullet will be fired
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("Bullet has entered the trigger zone.");
            Instantiate(Blood, firePoint.position, firePoint.rotation);
            Destroy(gameObject);
            Destroy(other.gameObject);
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