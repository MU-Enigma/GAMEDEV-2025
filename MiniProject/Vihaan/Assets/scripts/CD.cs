using UnityEngine;

public class CD : MonoBehaviour


{
    public SpriteRenderer spr;
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("Enemy enter");
            spr.color = Color.red;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("Enemy leave");
            spr.color = Color.white;
        }
    }
}
