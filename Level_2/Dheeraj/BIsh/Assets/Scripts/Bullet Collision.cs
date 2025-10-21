using UnityEngine;

public class BulletCollision : MonoBehaviour
{
    public Transform Hitbox;
    public GameObject Blood;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("Enemy Pierced");
            Instantiate(Blood, Hitbox.position, Hitbox.rotation);
        }
    }
}
