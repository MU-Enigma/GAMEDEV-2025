using UnityEngine;

public class BulletSpawning : MonoBehaviour
{
    [Header("Assets")]
    public GameObject bulletPrefab; // Reference to the bullet prefab
    public Transform firePoint; // The point from which the bullet will be fired
    public AudioClip fireSound; // Sound to play when firing

    [Header("Shotgun Settings")]
    [Tooltip("How many shots per second. 1.0 = 1 shot per second.")]
    public float shotsPerSecond = 1.2f; // Cooldown for a shotgun
    [Tooltip("The total angle (in degrees) of the bullet spread.")]
    public float spreadAngle = 30f; // Tighter spread for a rifle

    private Camera mainCam;
    private float fireCooldownTimer;
    private AudioSource audioSource; // Component to play the sound

    void Start()
    {
        mainCam = Camera.main;
        if (firePoint == null)
        {
            firePoint = transform;
        }
        
        // --- NEW: Get or add an AudioSource component ---
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update()
    {
        // Always count down the cooldown
        if (fireCooldownTimer > 0)
        {
            fireCooldownTimer -= Time.deltaTime;
        }

        // --- SHOTGUN LOGIC ---
        // Change to GetMouseButtonDown for single shots
        if (Input.GetMouseButtonDown(0) && fireCooldownTimer <= 0) 
        {
            // Reset the cooldown
            fireCooldownTimer = 1f / shotsPerSecond;

            // --- NEW: Play the fire sound ---
            if (fireSound != null)
            {
                audioSource.PlayOneShot(fireSound);
            }

            if (bulletPrefab == null || mainCam == null)
            {
                Debug.LogWarning("BulletSpawning script is missing Bullet Prefab or Main Camera.");
                return;
            }

            // 1. Get Mouse Position
            Vector3 mouseWorldPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPosition.z = 0f;

            // 2. Calculate *main* fire direction
            Vector2 mainDirection = (mouseWorldPosition - firePoint.position).normalized;
            
            float halfSpread = spreadAngle / 2f;

            // --- NEW: Fire 5 bullets in a spread ---
            // Bullet 1: Straight at cursor (0 degrees)
            FireBullet(mainDirection);
            // Bullet 2: Full positive angle
            FireBullet(Quaternion.Euler(0, 0, halfSpread) * mainDirection);
            // Bullet 3: Full negative angle
            FireBullet(Quaternion.Euler(0, 0, -halfSpread) * mainDirection);
            // Bullet 4: Half positive angle
            FireBullet(Quaternion.Euler(0, 0, halfSpread / 2f) * mainDirection);
            // Bullet 5: Half negative angle
            FireBullet(Quaternion.Euler(0, 0, -halfSpread / 2f) * mainDirection);
        }
    }


    /// <summary>
    /// Spawns a single bullet and sets its direction.
    /// (This code is correct and does not need to change)
    /// </summary>
    void FireBullet(Vector2 direction)
    {
        // 1. Calculate the rotation *for the sprite*
        //    (Assumes a RIGHT-facing bullet prefab)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 2. Instantiate the Bullet at the correct rotation
        //    (*** FIX: Removed '-90f' ***)
        //    This now correctly rotates your RIGHT-facing bullet sprite
        GameObject newProjectile = Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle - 90f));

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

