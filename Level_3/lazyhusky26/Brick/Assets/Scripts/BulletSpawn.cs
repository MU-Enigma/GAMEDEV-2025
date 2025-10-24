using UnityEngine;

public class BulletSpawn : MonoBehaviour
{
    public GameObject bullet;       // The bullet prefab
    public Transform ShootPos;      // The position to shoot from

    void Update()
    {
        // Right-click
        if (Input.GetMouseButtonDown(1))
        {
            if (bullet != null && ShootPos != null)
            {
                // Get mouse position in world space
                Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                mouseWorldPos.z = 0f; // Ensure z is 0 for 2D

                // Calculate direction from shoot position to mouse
                Vector2 shootDir = (mouseWorldPos - ShootPos.position).normalized;

                // Instantiate the bullet
                GameObject newBullet = Instantiate(bullet, ShootPos.position, Quaternion.identity);

                // Set bullet movement direction
                newBullet.GetComponent<ki_blast>().SetDirection(shootDir);
            }
        }
    }
}
