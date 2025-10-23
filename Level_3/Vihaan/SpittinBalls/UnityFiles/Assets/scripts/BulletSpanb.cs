using UnityEngine;

public class BulletSpawn : MonoBehaviour
{
    public GameObject bullet;
    public Transform spawnposition;
    public PM WB;

    public float ammo = 10f;
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (WB.currentWater >= ammo)
            {
                Instantiate(bullet, spawnposition.position, spawnposition.rotation);
                WB.currentWater -= ammo;
            }
        }
    }
}
