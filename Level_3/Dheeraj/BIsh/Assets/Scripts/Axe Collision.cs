using UnityEngine;

public class AxeCollision : MonoBehaviour
{
    public Transform Hitbox;
    public GameObject Blood;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Axe"))
        {
            Debug.Log("Enemy Stabbed");
            Instantiate(Blood, Hitbox.position, Hitbox.rotation);
        }
    }
}
