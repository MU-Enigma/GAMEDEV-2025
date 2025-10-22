using UnityEngine;

public class blodspawn : MonoBehaviour
{
    public GameObject Blod;
    public Transform Enemy;
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Instantiate(Blod, Enemy.position, Enemy.rotation);
            Destroy(gameObject);
        }
    }
}
