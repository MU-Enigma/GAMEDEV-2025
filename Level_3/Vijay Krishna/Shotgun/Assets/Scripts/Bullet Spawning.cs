using UnityEngine;

public class BulletSpawning : MonoBehaviour
{
    [Header("Assets")]
    public GameObject bulletPrefab; // Reference to the bullet prefab
    public Transform firePoint; // The point from which the bullet will be fired

    [Header("Shotgun Settings")]
    [Tooltip("The total angle of the spread in degrees.")]
    public float spreadAngle = 45f; // The angle you requested

    private Camera mainCam;

    void Start()
    {
        mainCam = Camera.main;
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // Check if the left mouse button is pressed
        {
            if (bulletPrefab == null || mainCam == null)
            {
                Debug.LogWarning("BulletSpawning script is missing Bullet Prefab or Main Camera.");
                return;
            }

            // 1. Get Mouse Position
            Vector3 mouseWorldPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0f;

            // 2. Calculate *main* fire direction (this is our 0 degrees)
            Vector2 mainDirection = (mouseWorldPosition - firePoint.position).normalized;

            // --- SHOTGUN LOGIC ---
            // We fire 5 bullets, rotating the main direction vector for each one.
            
            float halfSpread = spreadAngle / 2f;
            
            // Bullet 1: Straight at cursor (0 degrees)
            FireBullet(mainDirection);

            // Bullet 2: +45 degrees
            FireBullet(Quaternion.Euler(0, 0, spreadAngle) * mainDirection);

            // Bullet 3: +22.5 degrees (between 0 and 45)
            FireBullet(Quaternion.Euler(0, 0, halfSpread) * mainDirection);
            
            // Bullet 4: -45 degrees
            FireBullet(Quaternion.Euler(0, 0, -spreadAngle) * mainDirection);

            // Bullet 5: -22.5 degrees (between 0 and -45)
            FireBullet(Quaternion.Euler(0, 0, -halfSpread) * mainDirection);
            // --- END SHOTGUN LOGIC ---
        }
    }

    /// <summary>
    /// Spawns a single bullet and sets its direction.
    /// </summary>
    void FireBullet(Vector2 direction)
    {
        // 1. Calculate the rotation *for the sprite*
        //    (Assumes a RIGHT-facing bullet prefab)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 2. Instantiate the Bullet at the correct rotation
        //    (This line is CRITICAL - it makes the bullet *face* the direction it's going)
        GameObject newProjectile = Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle-90f));

        // 3. Set the bullet's movement direction
        BulletFiring bulletScript = newProjectile.GetComponent<BulletFiring>();
        if (bulletScript != null)
        {
            // Tell the bullet which way to travel
            bulletScript.SetDirection(direction);
        }
        else
        {
            Debug.LogWarning("Bullet Prefab is missing the 'BulletFiring.cs' script.");
        }
    }
}
