using UnityEngine;

public class pt : MonoBehaviour
{
    public GameObject bullet;
    public Transform spawnpoint;
    void Update()
    {
        if(Input.GetKeyDown("b"))
        {
            Instantiate(bullet, spawnpoint.position, spawnpoint.rotation);
        }

    }
}
