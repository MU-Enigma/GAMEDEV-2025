using UnityEngine;

public class ColDetBul : MonoBehaviour
{
    public SpriteRenderer spr;
    public GameObject Blood;
    public Transform Enemy;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pellet"))
        {
            Debug.Log("Enemy hit by bullet");

            // Change color to red
            if (spr != null)
                spr.color = Color.red;

            // Instantiate blood effect at enemy position
            if (Blood != null && Enemy != null)
                Instantiate(Blood, Enemy.position, Enemy.rotation);

            // Destroy enemy
            Destroy(gameObject);

            // Optionally destroy the bullet too
            Destroy(other.gameObject);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Pellet"))
        {
            Debug.Log("Bullet exited enemy collider");

            if (spr != null)
                spr.color = Color.white;
        }
    }
}
