using UnityEngine;

public class DestroyOnTagCollision : MonoBehaviour
{
   
    public string targetTag;

    
    void OnCollisionEnter2D(Collision2D collision)
    {
        
        if (collision.gameObject.CompareTag(targetTag))
        {
           
            Destroy(gameObject);
        }
        
        
    }
}