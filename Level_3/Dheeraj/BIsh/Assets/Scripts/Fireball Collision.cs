using UnityEngine;

public class FireballCollision : MonoBehaviour
{
    public Transform Hitbox;
    public GameObject Blood;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Fireball"))
        {
            Debug.Log("Enemy Hit");
            Instantiate(Blood, Hitbox.position, Hitbox.rotation);
        }
    }
}
