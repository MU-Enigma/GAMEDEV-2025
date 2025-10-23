using UnityEngine;
using System.Collections; // Required for Coroutines

/// <summary>
/// This single script controls a dual-wielding pistol setup.
/// Attach this to the *parent* GameObject ("Dualies").
/// This script handles rotating the *individual child guns* to face the mouse
/// and firing bullets alternately from two fire points.
///
/// It now assumes a *separate* script (like a Player Controller)
/// is responsible for setting this parent object's 'transform.localScale.x'
/// (to flip it horizontally when the player turns).
///
/// This script assumes:
/// 1. The gun sprites face RIGHT by default.
/// 2. You will assign the two "firePoint" transforms.
/// 3. You will assign the "gunRight" and "gunLeft" transforms.
/// 4. Your 'Bullet' prefab has a 'BulletFiring' script with a 'SetDirection' method.
/// </summary>
public class DualPistolController : MonoBehaviour
{
    [Header("Assets")]
    public GameObject bulletPrefab; // Reference to the bullet prefab

    [Header("Gun Transforms")]
    [Tooltip("The Transform of the right gun's sprite/object.")]
    public Transform gunRight;
    [Tooltip("The Transform of the left gun's sprite/object.")]
    public Transform gunLeft;
    [Tooltip("The transform where bullets spawn from the *right* pistol.")]
    public Transform firePointRight;
    [Tooltip("The transform where bullets spawn from the *left* pistol.")]
    public Transform firePointLeft;

    // --- NEW: Recoil Settings ---
    [Header("Recoil")]
    [Tooltip("How far back the gun kicks on its local X axis.")]
    public float recoilDistance = 0.1f;
    [Tooltip("How long the entire recoil animation takes.")]
    public float recoilDuration = 0.15f;

    // --- Internal State ---
    private Camera mainCam;
    private float originalYScaleRight;
    private float originalYScaleLeft;
    private bool fireFromRightGun = true; // For alternating fire
    
    // --- NEW: Recoil State ---
    private Vector3 originalPosRight;
    private Vector3 originalPosLeft;
    private Coroutine recoilCoroutineRight;
    private Coroutine recoilCoroutineLeft;


    void Start()
    {
        mainCam = Camera.main;
        
        // Store original scale and position for flipping/recoil
        if (gunRight != null)
        {
            originalYScaleRight = gunRight.localScale.y;
            originalPosRight = gunRight.localPosition; // Store original pos
        }
        if (gunLeft != null)
        {
            originalYScaleLeft = gunLeft.localScale.y;
            originalPosLeft = gunLeft.localPosition; // Store original pos
        }

        if (firePointRight == null || firePointLeft == null)
        {
            Debug.LogError("DualPistolController: Fire points are not assigned!");
        }
        if (gunRight == null || gunLeft == null)
        {
            Debug.LogError("DualPistolController: Gun Transforms are not assigned!");
        }
    }

    void Update()
    {
        if (mainCam == null) return; // Safety check

        // 1. Handle gun rotation and flipping
        HandleGunRotations();

        // 2. Handle firing logic
        if (Input.GetMouseButtonDown(0)) // Check for a single click
        {
            Fire();
        }
    }

    /// <summary>
    /// Gets mouse position and calls RotateGun for each gun.
    /// </summary>
    void HandleGunRotations()
    {
        // Get Mouse Position once
        Vector3 mouseWorldPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f; // Ensure it's on the 2D plane (z=0)
        
        // Rotate each gun individually
        RotateGun(gunRight, mouseWorldPosition, originalYScaleRight);
        RotateGun(gunLeft, mouseWorldPosition, originalYScaleLeft);
    }

    /// <summary>
    /// Rotates a single gun transform to face the mouse.
    /// </summary>
    void RotateGun(Transform gun, Vector3 mousePos, float originalYScale)
    {
        if (gun == null) return;

        // --- 1. Calculate Direction (from THIS gun to mouse) ---
        Vector3 direction = mousePos - gun.position;

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
        gun.rotation = Quaternion.Euler(0f, 0f, finalAngle);

        // --- 5. Handle Vertical Flipping (Keep Upright) ---
        // Use the 'baseAngle' to check if we're pointing "up" or "down"
        // We preserve the gun's current X scale (which might be flipped by the parent)
        float currentXScale = gun.localScale.x;
        if (Mathf.Abs(baseAngle) > 90)
        {
            // Pointing "down"
            gun.localScale = new Vector3(currentXScale, -originalYScale, gun.localScale.z);
        }
        else
        {
            // Pointing "up"
            gun.localScale = new Vector3(currentXScale, originalYScale, gun.localScale.z);
        }
    }


    /// <summary>
    /// Fires a bullet from the correct pistol and triggers recoil.
    /// </summary>
    void Fire()
    {
        if (bulletPrefab == null) return;

        if (fireFromRightGun)
        {
            // 1. Fire from right
            FireBullet(firePointRight);
            
            // 2. Start recoil on right
            if (gunRight != null)
            {
                // Stop any previous recoil on this gun
                if (recoilCoroutineRight != null) StopCoroutine(recoilCoroutineRight);
                recoilCoroutineRight = StartCoroutine(RecoilGun(gunRight, originalPosRight));
            }
        }
        else
        {
            // 1. Fire from left
            FireBullet(firePointLeft);
            
            // 2. Start recoil on left
            if (gunLeft != null)
            {
                // Stop any previous recoil on this gun
                if (recoilCoroutineLeft != null) StopCoroutine(recoilCoroutineLeft);
                recoilCoroutineLeft = StartCoroutine(RecoilGun(gunLeft, originalPosLeft));
            }
        }

        // 3. Toggle the boolean to alternate guns
        fireFromRightGun = !fireFromRightGun;
    }
    
    /// <summary>
    /// Coroutine to apply a simple backward-then-forward kickback.
    /// </summary>
    IEnumerator RecoilGun(Transform gun, Vector3 originalPos)
    {
        // --- 1. Kick Back ---
        float kickDuration = recoilDuration * 0.33f; // 1/3rd of the time to kick back
        // Kicks back along the gun's local left (assuming right-facing sprite)
        Vector3 targetPos = originalPos + (Vector3.left * recoilDistance);
        
        float timer = 0f;
        while(timer < kickDuration)
        {
            gun.localPosition = Vector3.Lerp(originalPos, targetPos, timer / kickDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        gun.localPosition = targetPos;
        
        // --- 2. Return to Original Position ---
        float returnDuration = recoilDuration * 0.67f; // 2/3rds of the time to return
        timer = 0f;
        while(timer < returnDuration)
        {
            gun.localPosition = Vector3.Lerp(targetPos, originalPos, timer / returnDuration);
            timer += Time.deltaTime;
            yield return null;
        }
        gun.localPosition = originalPos;
    }


    /// <summary>
    /// Spawns a single bullet from the chosen firePoint.
    /// </summary>
    void FireBullet(Transform firePoint)
    {
        if (firePoint == null)
        {
            Debug.LogError("FirePoint is not assigned for one of the guns!");
            return;
        }

        // 1. Get Mouse Position
        Vector3 mouseWorldPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f;

        // 2. Calculate Direction (from the *specific firePoint* to mouse)
        Vector2 fireDirection = (mouseWorldPosition - firePoint.position).normalized;

        // 3. Calculate Rotation (for the bullet prefab)
        // (Assumes a RIGHT-facing bullet prefab)
        float angle = Mathf.Atan2(fireDirection.y, fireDirection.x) * Mathf.Rad2Deg;

        // 4. Instantiate the Bullet at the correct rotation
        // This now correctly rotates your RIGHT-facing bullet prefab.
        GameObject newProjectile = Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle - 90f));

        // 5. Set the bullet's movement direction
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

