using UnityEngine;

public class SwordHit : MonoBehaviour
{
      void OnTriggerEnter2D(Collider2D other)
    {
       
        if (other.gameObject.CompareTag("Enemy"))
        {
           
            Debug.Log("Sword hit and killed " + other.name);
            Destroy(other.gameObject);
        }
    }
}

