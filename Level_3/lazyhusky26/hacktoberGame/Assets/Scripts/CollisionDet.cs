using UnityEngine;

public class ColDetBul : MonoBehaviour
{
    public SpriteRenderer spr;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Pellet"))
        {
            Debug.Log("Enemy enter");
            spr.color = Color.red;
            Destroy(gameObject); // Destroys this GameObject
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Pellet"))
        {
            Debug.Log("Enemy Exit");
            spr.color = Color.white;
        }
    }
}
