using UnityEngine;

public class blt : MonoBehaviour
{
    public GameObject bullet;
    public Transform spawnpos;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            Instantiate(bullet, spawnpos.position, spawnpos.rotation);
        }
    }
}