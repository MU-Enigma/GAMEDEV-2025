using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    public GameObject particle;
    public Transform dmgpoint;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("bullet"))
        {
            Instantiate(particle, dmgpoint.position, dmgpoint.rotation);
            Destroy(other.gameObject);
            Destroy(gameObject);


        }

        if (other.CompareTag("Orchid") || other.CompareTag("ally"))
        {
            Instantiate(particle, dmgpoint.position, dmgpoint.rotation);
            Destroy(gameObject);


        }

    }
    
     private void OnCollisionEnter(Collision collision)
    {
        NukeMissile missile = collision.collider.GetComponent<NukeMissile>();
        if (missile != null)
        {
                Destroy(missile.gameObject);
        }
    }


}
