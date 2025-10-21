using UnityEngine;

public class BloodSpawn : MonoBehaviour
{
    public GameObject Blood;
    public Transform Enemy;
    public float Timedestroy;
    
    void Start()
    {
        Destroy(gameObject, Timedestroy);    
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Bullet"))
        {
            Debug.Log("Enemy Pierced");
        }
    }
}
