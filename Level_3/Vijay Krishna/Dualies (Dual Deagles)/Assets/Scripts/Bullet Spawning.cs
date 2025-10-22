using UnityEngine;

public class BulletSpawning : MonoBehaviour
{
    public GameObject bulletPrefab; // Reference to the bullet prefab
    public Transform firePoint; // The point from which the bullet will be fired

    // --- NEW: Reference to the main camera ---
    private Camera mainCam;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // --- NEW: Get the main camera ---
        mainCam = Camera.main;
        if (firePoint == null)
        {
            firePoint = transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // --- REMOVED: Keyboard direction tracking ---

        // Fire the projectile on button press
        if (Input.GetMouseButtonDown(0)) // Check if the left mouse button is pressed
        {
            if (bulletPrefab == null || mainCam == null)
            {
                Debug.LogWarning("BulletSpawning script is missing Bullet Prefab or Main Camera.");
                return;
            }

            // --- NEW: Mouse Aiming Logic ---
            // 1. Get Mouse Position in World Space
            Vector3 mouseWorldPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0f; // Ensure it's 2D

            // 2. Calculate Direction from fire point to mouse
            Vector2 fireDirection = (mouseWorldPosition - firePoint.position).normalized;

            // 3. Calculate Rotation for the bullet
            float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;

            // 4. Instantiate the Bullet at the correct rotation
            GameObject newProjectile = Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle-90f));
            
            // 5. Set the bullet's direction
            BulletFiring bulletScript = newProjectile.GetComponent<BulletFiring>();
            if (bulletScript != null)
            {
                bulletScript.SetDirection(fireDirection);
            }
            else
            {
                Debug.LogWarning("Bullet Prefab is missing the 'BulletFiring.cs' script.");
            }
            // --- END NEW LOGIC ---
        }
    }
}
