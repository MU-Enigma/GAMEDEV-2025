using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public GameObject particle;
    public Transform dmgpoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("bullet"))
        {
            Debug.Log("Enemy Enter");
            Instantiate(particle, dmgpoint.position, dmgpoint.rotation);
            Destroy(other.gameObject);
            Destroy(gameObject);


        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("bullet"))
        {
            Debug.Log("Enemy leave");

        } 
    }
}
