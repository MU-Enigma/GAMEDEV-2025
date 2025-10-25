using UnityEngine;

public class bulletspawning : MonoBehaviour
{
    public GameObject bullet;
    public Transform spawnposition;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
           
            GameObject newBullet = Instantiate(bullet, spawnposition.position, spawnposition.rotation);

           
            if (transform.localScale.x < 0)
            {
                
                
                SpriteRenderer sr = newBullet.GetComponent<SpriteRenderer>();
                if (sr != null)
                {
                    
                    sr.flipX = true;
                }
                else
                {
                   
                    newBullet.transform.localScale = new Vector3(
                        newBullet.transform.localScale.x * -1, 
                        newBullet.transform.localScale.y, 
                        newBullet.transform.localScale.z
                    );
                }
            }
            
           
        }
    }
}