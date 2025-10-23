using UnityEngine;

public class BulletSpawning : MonoBehaviour
{
    [Header("Assets")]
    public GameObject bulletPrefab; // Reference to the bullet prefab
    public Transform firePoint; // The point from which the bullet will be fired

    [Header("Minigun Settings")]
    [Tooltip("How many bullets are fired per second.")]
    public float shotsPerSecond = 10f; 
    [Tooltip("The max random angle (in degrees) bullets can deviate.")]
    public float spreadAngle = 5f;

    private Camera mainCam;
    private float fireCooldownTimer;

    void Start()
    {
        mainCam = Camera.main;
        if (firePoint == null)
        {
            firePoint = transform;
        }
        fireCooldownTimer = 0f;
    }

    void Update()
    {
        // Decrement the cooldown timer
        if (fireCooldownTimer > 0)
        {
            fireCooldownTimer -= Time.deltaTime;
        }

        // --- MINIGUN LOGIC ---
        // Change to GetMouseButton for continuous fire
        if (Input.GetMouseButton(0) && fireCooldownTimer <= 0) 
        {
            if (bulletPrefab == null || mainCam == null)
            {
                Debug.LogWarning("BulletSpawning script is missing Bullet Prefab or Main Camera.");
                return;
            }

            // Reset the cooldown
            fireCooldownTimer = 1f / shotsPerSecond;

            // 1. Get Mouse Position
            Vector3 mouseWorldPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0f;

            // 2. Calculate *main* fire direction
            Vector2 mainDirection = (mouseWorldPosition - firePoint.position).normalized;

            // 3. Add random spread
            float randomSpread = Random.Range(-spreadAngle / 2f, spreadAngle / 2f);
            Vector2 fireDirection = Quaternion.Euler(0, 0, randomSpread) * mainDirection;

            // 4. Fire the single, slightly-inaccurate bullet
            FireBullet(fireDirection);
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
