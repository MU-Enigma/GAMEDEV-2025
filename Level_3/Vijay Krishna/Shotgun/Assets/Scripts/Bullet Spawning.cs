using UnityEngine;
using System.Collections; // Required for Coroutines

/// <summary>
/// This single script controls a shotgun.
/// Attach this to the *parent* GameObject ("Shotgun").
/// This script handles rotating the gun to face the mouse,
/// firing a 5-bullet spread, and a single-gun recoil.
///
/// It now assumes a *separate* script (like a Player Controller)
/// is responsible for setting this parent object's 'transform.localScale.x'
/// (to flip it horizontally when the player turns).
///
/// This script assumes:
/// 1. The gun sprite faces RIGHT by default.
/// 2. The bullet prefab faces UP by default (hence the -90f rotation).
/// 3. Your 'Bullet' prefab has a 'BulletFiring' script with a 'SetDirection' method.
/// </summary>
public class ShotgunController : MonoBehaviour
{
    [Header("Assets")]
    public GameObject bulletPrefab; // Reference to the bullet prefab
    public Transform firePoint; // The point from which the bullet will be fired
    
    [Header("Audio")]
    [Tooltip("Assign an AudioSource component that will play the fire sound.")]
    public AudioSource fireSoundSource;

    [Header("Shotgun Settings")]
    [Tooltip("How many shots per second. 1.0 = 1 shot per second.")]
    public float shotsPerSecond = 1.2f;
    [Tooltip("The total angle (in degrees) of the bullet spread.")]
    public float spreadAngle = 30f;

    [Header("Recoil")]
    [Tooltip("How far back the gun kicks on its local X axis.")]
    public float recoilDistance = 0.2f; // More kick for a shotgun
    [Tooltip("How long the entire recoil animation takes.")]
    public float recoilDuration = 0.15f;

    // --- Internal State ---
    private Camera mainCam;
    private float originalYScale;
    private float fireCooldownTimer;

    // --- Recoil State ---
    private Vector3 originalPos;
    private Coroutine recoilCoroutine;

    void Start()
    {
        mainCam = Camera.main;
        
        // Store original scale and position for flipping/recoil
        originalYScale = transform.localScale.y;
        originalPos = transform.localPosition; // Store original pos

        if (firePoint == null)
        {
            firePoint = transform;
        }

        if (fireSoundSource == null)
        {
            Debug.LogWarning("ShotgunController: No fire sound source assigned.");
        }
    }

    void Update()
    {
        if (mainCam == null) return; // Safety check

        // Always count down the cooldown
        if (fireCooldownTimer > 0)
        {
            fireCooldownTimer -= Time.deltaTime;
        }

        // 1. Handle gun rotation and flipping
        HandleGunRotation();

        // 2. Handle firing logic
        if (Input.GetMouseButtonDown(0) && fireCooldownTimer <= 0) // Check for a single click
        {
            Fire();
        }
    }

    /// <summary>
    /// Rotates the gun transform to face the mouse.
    /// </summary>
    void HandleGunRotation()
    {
        // Get Mouse Position once
        Vector3 mouseWorldPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f; // Ensure it's on the 2D plane (z=0)
        
        // --- 1. Calculate Direction (from THIS gun to mouse) ---
        Vector3 direction = mouseWorldPosition - transform.position;

        // --- 2. Calculate Base Angle ---
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // --- 3. Calculate Final Angle ---
        // We add 180 degrees if the PARENT (this.transform) is flipped
        float finalAngle = baseAngle;
        if (transform.localScale.x < 0)
        {
            finalAngle += 180f;
        }

        // --- 4. Apply Rotation ---
        transform.rotation = Quaternion.Euler(0f, 0f, finalAngle);

        // --- 5. Handle Vertical Flipping (Keep Upright) ---
        // Use the 'baseAngle' to check if we're pointing "up" or "down"
        float currentXScale = transform.localScale.x;
        if (Mathf.Abs(baseAngle) > 90)
        {
            // Pointing "down"
            transform.localScale = new Vector3(currentXScale, -originalYScale, transform.localScale.z);
        }
        else
        {
            // Pointing "up"
            transform.localScale = new Vector3(currentXScale, originalYScale, transform.localScale.z);
        }
    }


    /// <summary>
    /// Fires the 5-bullet spread and triggers recoil.
    /// </summary>
    void Fire()
    {
        if (bulletPrefab == null) return;

        // Reset the cooldown
        fireCooldownTimer = 1f / shotsPerSecond;

        // Play the fire sound
        if (fireSoundSource != null && fireSoundSource.clip != null)
        {
            fireSoundSource.PlayOneShot(fireSoundSource.clip);
        }

        // --- Start Recoil ---
        if (recoilCoroutine != null) StopCoroutine(recoilCoroutine);
        recoilCoroutine = StartCoroutine(Recoil());
        
        // --- Fire 5 Bullets ---
        Vector3 mouseWorldPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f;
        Vector2 mainDirection = (mouseWorldPosition - firePoint.position).normalized;
            
        float halfSpread = spreadAngle / 2f;

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
    
    /// <summary>
    /// Coroutine to apply a simple backward-then-forward kickback.
    /// </summary>
    IEnumerator Recoil()
    {
        // --- 1. Kick Back ---
        float kickDuration = recoilDuration * 0.33f; // 1/3rd of the time to kick back
        // Kicks back along the gun's local left (assuming right-facing sprite)
        Vector3 targetPos = originalPos + (Vector3.left * recoilDistance);
        
        float timer = 0f;
        while(timer < kickDuration)
        {
            transform.localPosition = Vector3.Lerp(originalPos, targetPos, timer / kickDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = targetPos;
        
        // --- 2. Return to Original Position ---
        float returnDuration = recoilDuration * 0.67f; // 2/3rds of the time to return
        timer = 0f;
        while(timer < returnDuration)
        {
            transform.localPosition = Vector3.Lerp(targetPos, originalPos, timer / returnDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        transform.localPosition = originalPos;
    }


    /// <summary>
    /// Spawns a single bullet from the chosen firePoint.
    /// </summary>
    void FireBullet(Vector2 fireDirection)
    {
        // 1. Calculate Rotation (for the bullet prefab)
        float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;

        // 2. Instantiate the Bullet at the correct rotation
        // User confirmed -90f is correct for their UP-facing prefab
        GameObject newProjectile = Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle - 90f));

        // 3. Set the bullet's movement direction
        BulletFiring bulletScript = newProjectile.GetComponent<BulletFiring>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(fireDirection);
        }
        else
        {
            Debug.LogWarning("Bullet Prefab is missing the 'BulletFiring.cs' script.");
        }
    }
}

