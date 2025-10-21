using UnityEngine;

public class SwordCollision : MonoBehaviour
{
    public Transform Hitbox;
    public GameObject Blood;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Sword"))
        {
            Debug.Log("Enemy Stabbed");
            Instantiate(Blood, Hitbox.position, Hitbox.rotation);
        }
    }
}
