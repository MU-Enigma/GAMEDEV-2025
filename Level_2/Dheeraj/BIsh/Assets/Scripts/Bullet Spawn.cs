using UnityEngine;

public class BulletSpawn : MonoBehaviour
{
    public GameObject bullet;
    public Transform spawnposition;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(1))
        {
            Instantiate(bullet, spawnposition.position, spawnposition.rotation);
        }
    }
}
