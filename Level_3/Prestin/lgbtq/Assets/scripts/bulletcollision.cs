using UnityEngine;


[RequireComponent(typeof(SpriteRenderer))]
public class Collisiondetection : MonoBehaviour
{
  
    public Sprite damagedSprite;


    private SpriteRenderer spriteRenderer;

    void Start()
    {
      
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
      
        if (other.CompareTag("Bullet"))
        {
     
            if (damagedSprite != null)
            {
          
                spriteRenderer.sprite = damagedSprite;
            }
            else
            {
                Debug.LogWarning("Damaged Sprite is not assigned in the Inspector!", this);
            }

          
            Destroy(other.gameObject);
        }
    }

   
}