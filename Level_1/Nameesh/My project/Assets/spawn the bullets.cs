using Unity.VisualScripting;
using UnityEngine;

public class pt : MonoBehaviour
{
    public GameObject bullet;
    public Transform spawnpoint;
    void Update()
    {
        if(Input.GetMouseButton(0))
        {
            Instantiate(bullet, spawnpoint.position, spawnpoint.rotation);
        }

    }
}