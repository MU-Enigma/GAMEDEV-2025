using UnityEngine;

public class bulletspawning : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject bullet;
    public Transform spawnposition;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Instantiate(bullet, spawnposition.position, spawnposition.rotation);
        }
        
    }
}
