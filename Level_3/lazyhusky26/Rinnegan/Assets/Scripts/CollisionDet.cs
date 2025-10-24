using UnityEngine;

public class ColDetBul : MonoBehaviour
{
    public SpriteRenderer spr;
    public GameObject Blood;
    public GameObject KiBlastEffect; // New kill effect for ki_blast
    public Transform Enemy;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pellet") || other.name.Contains("ki_blast"))
        {
            Debug.Log("Enemy hit by bullet or ki_blast");

            // Change color to red
            if (spr != null)
                spr.color = Color.red;

            // Instantiate appropriate effect
            if (Enemy != null)
            {
                if (other.name.Contains("ki_blast") && KiBlastEffect != null)
                {
                    Instantiate(KiBlastEffect, Enemy.position, Enemy.rotation);
                }
                else if (Blood != null)
                {
                    Instantiate(Blood, Enemy.position, Enemy.rotation);
                }
            }

            // Destroy enemy
            Destroy(gameObject);

            // Destroy the projectile
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
