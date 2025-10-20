using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public GameObject particle;
    public Transform dmgpoint;

    public float life = 5f;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("bullet"))
        {
            Debug.Log("Enemy Enter");
            Instantiate(particle, dmgpoint.position, dmgpoint.rotation);
            Destroy(other.gameObject);
            if (life > 0f){
                life -= 1f;
            }
            else{
                Destroy(gameObject);
            }


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
