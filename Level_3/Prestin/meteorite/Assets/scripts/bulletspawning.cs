using UnityEngine;

public class bulletspawning : MonoBehaviour
{
    public GameObject bullet;
    public Transform spawnposition;
    public float bulletSpeed = 10f;

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
           
            GameObject newBullet = Instantiate(bullet, spawnposition.position, Quaternion.identity);
            
          
            Rigidbody2D rb2d = newBullet.GetComponent<Rigidbody2D>();
            if (rb2d != null)
            {
                rb2d.linearVelocity = Vector2.down * bulletSpeed;
            }
            else
            {
                Rigidbody rb3d = newBullet.GetComponent<Rigidbody>();
                if (rb3d != null)
                {
                    rb3d.linearVelocity = Vector3.down * bulletSpeed;
                }
                else
                {
                    Debug.LogWarning("Bullet prefab is missing a Rigidbody or Rigidbody2D component!");
                }
            }
        }
    }
}