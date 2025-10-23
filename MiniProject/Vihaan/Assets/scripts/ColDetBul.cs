using UnityEngine;
public class ColDetBul : MonoBehaviour
{
    public SpriteRenderer spr;
    public GameObject Blood;
    public Transform Enemy;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("Enemy enter");
            spr.color = Color.red;
            Instantiate(Blood, Enemy.position, Enemy.rotation);
            Destroy(gameObject);
           
        }
    }
        private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("Enemy Exit");
            spr.color = Color.white;
        }
    }
}