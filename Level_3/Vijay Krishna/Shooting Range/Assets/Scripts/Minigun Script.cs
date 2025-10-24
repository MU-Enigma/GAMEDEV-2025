using UnityEngine;
using System.Collections; // Required for Coroutines

// This single script now handles both Gun Rotation and Bullet Spawning (Minigun)
public class MinigunController : MonoBehaviour
{
    // --- FROM BULLET SPAWNING ---
    [Header("Assets")]
    public GameObject bulletPrefab; // Reference to the bullet prefab
    public Transform firePoint; // The point from which the bullet will be fired
    
    [Header("Audio (Assign separate AudioSources)")]
    public AudioSource revUpSoundSource;
    public AudioSource firingSoundSource;
    public AudioSource revDownSoundSource;

    [Header("Minigun Mechanics")]
    [Tooltip("How many bullets are fired per second.")]
    public float shotsPerSecond = 10f; 
    [Tooltip("The max random angle (in degrees) bullets can deviate.")]
    public float spreadAngle = 5f;
    [Tooltip("Time in seconds to rev up before firing. (Used as a fallback if no rev-up clip is assigned)")]
    public float revUpTime = 0.5f;
    [Tooltip("Max time in seconds for continuous fire before overheating.")]
    public float maxFireTime = 3.0f;
    [Tooltip("Time in seconds to cool down after overheating.")]
    public float overheatCooldown = 2.0f;

    [Header("Visuals (Optional)")]
    [Tooltip("The gun's sprite to change color on overheat.")]
    public SpriteRenderer gunSprite;
    public Color overheatColor = new Color(1, 0.5f, 0.5f); // Reddish hue
    
    private enum GunState { Idle, RevvingUp, Firing, Overheated }
    private GunState currentState = GunState.Idle;

    private float fireCooldownTimer; // For bullet rate of fire
    private float currentHeat; // Tracks heat from 0 to maxFireTime
    private Color originalColor;
    private Coroutine fireSequenceCoroutine;
    
    // --- FROM GUN ROTATION ---
    [Header("Rotation Mechanics")]
    [Tooltip("This script assumes the gun sprite faces RIGHT by default.")]
    private float originalYScale;
    
    // --- SHARED ---
    private Camera mainCam;

    void Start()
    {
        // --- SHARED ---
        mainCam = Camera.main;
        
        // --- FROM BULLET SPAWNING ---
        if (firePoint == null)
        {
            firePoint = transform;
        }
        
        if (gunSprite != null)
        {
            originalColor = gunSprite.color;
        }
        
        // --- FROM GUN ROTATION ---
        // Store the original Y scale for flipping
        originalYScale = transform.localScale.y;
    }

    void Update()
    {
        // --- FROM BULLET SPAWNING (State Machine) ---
        // This runs all the time
        HandleCooldowns();

        // State machine logic is now in the coroutine
        switch (currentState)
        {
            case GunState.Idle:
                // Check for mouse press to start the rev-up/fire sequence
                if (Input.GetMouseButtonDown(0) && currentState != GunState.Overheated)
                {
                    fireSequenceCoroutine = StartCoroutine(RevAndFireSequence());
                }
                break;
                
            case GunState.Overheated:
                // We just wait in this state. HandleCooldowns() will
                // automatically switch us back to Idle when heat is 0.
                break;
        }
        
        // --- FROM GUN ROTATION (Always run this) ---
        HandleGunRotation();
    }

    // --- NEW: Coroutine to handle the entire firing sequence (FROM BULLET SPAWNING) ---
    IEnumerator RevAndFireSequence()
    {
        // --- 1. REV UP STAGE ---
        ChangeState(GunState.RevvingUp);
        
        // Use the *actual* clip length for perfect timing
        float duration = revUpTime; // Default
        if (revUpSoundSource != null && revUpSoundSource.clip != null)
        {
            duration = revUpSoundSource.clip.length;
        }

        float timer = 0f;
        while (timer < duration)
        {
            // Check if player released the mouse *during* rev up
            if (Input.GetMouseButtonUp(0))
            {
                ChangeState(GunState.Idle); // This plays revDown sound
                yield break; // Exit the coroutine
            }
            timer += Time.deltaTime;
            yield return null; // Wait for next frame
        }

        // --- 2. FIRING STAGE ---
        // Rev up finished, start firing
        ChangeState(GunState.Firing); // This plays the *looping* fire sound

        // This loop will run as long as we are in the Firing state
        while (currentState == GunState.Firing)
        {
            // Check if player released the mouse *during* firing
            if (Input.GetMouseButtonUp(0))
            {
                ChangeState(GunState.Idle); // Plays revDown
                yield break; // Exit coroutine
            }

            // Handle heat and firing
            currentHeat += Time.deltaTime;
            TryToFire(); // This handles subsequent bullets

            // Check for overheat
            if (currentHeat >= maxFireTime)
            {
                ChangeState(GunState.Overheated);
                yield break; // Exit coroutine (overheat state will be managed by HandleCooldowns)
            }

            yield return null; // Wait for next frame
        }
    }

    /// <summary>
    /// Manages all timers: bullet rate of fire and heat cooling. (FROM BULLET SPAWNING)
    /// </summary>
    void HandleCooldowns()
    {
        // Bullet firing rate
        if (fireCooldownTimer > 0)
        {
            fireCooldownTimer -= Time.deltaTime;
        }

        // Heat cooling
        // Only cool down if NOT firing and heat is above 0
        if (currentState != GunState.Firing && currentHeat > 0)
        {
            // Calculate cool rate. This will cool from max heat to 0 in 'overheatCooldown' seconds.
            float coolRate = maxFireTime / overheatCooldown;
            currentHeat -= coolRate * Time.deltaTime;
            currentHeat = Mathf.Max(0, currentHeat); // Clamp at 0

            // If we were overheated and are now cool, switch to Idle
            if (currentState == GunState.Overheated && currentHeat <= 0)
            {
                ChangeState(GunState.Idle);
            }
        }
        
        // Always update visuals
        UpdateOverheatVisuals();
    }
    
    /// <summary>
    /// Handles all visual rotation and flipping of the gun. (FROM GUN ROTATION)
    /// </summary>
    void HandleGunRotation()
    {
        if (mainCam == null) return; // Safety check

        // --- 1. Get Mouse Position ---
        Vector3 mouseWorldPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f; // Ensure it's on the 2D plane (z=0)

        // --- 2. Calculate Direction (from gun to mouse) ---
        Vector3 direction = mouseWorldPosition - transform.position;

        // --- 3. Calculate Base Angle ---
        // This is the angle *before* any player-flip adjustments
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // --- 4. Adjust Angle based on Gun's *Own* Flip ---
        // Start with the base angle
        float finalAngle = baseAngle;
        
        // We check if the gun itself is flipped horizontally.
        // We assume a player script is setting this.
        if (transform.localScale.x < 0)
        {
            // The gun is flipped (facing left).
            // We must add 180 degrees to the angle to make it point correctly.
            finalAngle += 180f;
        }

        // --- 5. Apply Rotation ---
        // Create a rotation quaternion based on our *final* angle
        transform.rotation = Quaternion.Euler(0f, 0f, finalAngle);

        // --- 6. Handle Sprite Flipping (Vertical) ---
        // This logic now correctly uses the 'baseAngle' to determine
        // if the gun is pointing "up" or "down".
        
        if (Mathf.Abs(baseAngle) > 90)
        {
            // Gun is rotated > 90 or < -90 degrees, flip Y to keep it upright
            // We use the *current* localScale.x and .z
            transform.localScale = new Vector3(transform.localScale.x, -originalYScale, transform.localScale.z);
        }
        else
        {
            // Gun is in the "upright" rotation range, use original scale Y
            // We use the *current* localScale.x and .z
            transform.localScale = new Vector3(transform.localScale.x, originalYScale, transform.localScale.z);
        }
    }

    /// <summary>
    /// Central hub for changing states and playing sounds. (FROM BULLET SPAWNING)
    /// </summary>
    void ChangeState(GunState newState)
    {
        if (currentState == newState) return;
        
        GunState previousState = currentState; // Store the old state
        currentState = newState;
        
        // Stop the coroutine if we're forced into Idle or Overheated
        if (fireSequenceCoroutine != null && (newState == GunState.Idle || newState == GunState.Overheated))
        {
            StopCoroutine(fireSequenceCoroutine);
            fireSequenceCoroutine = null;
        }
        
        // Stop any sound from the previous state that needs stopping
        if (previousState == GunState.Firing && firingSoundSource != null)
        {
            firingSoundSource.Stop();
        }
        if (previousState == GunState.RevvingUp && revUpSoundSource != null)
        {
            revUpSoundSource.Stop();
        }


        switch (newState)
        {
            case GunState.Idle:
                // If we are coming from Firing or RevvingUp, play rev down
                if ((previousState == GunState.Firing || previousState == GunState.RevvingUp) && revDownSoundSource != null)
                {
                    // Only play rev down if it's not already playing
                    if (!revDownSoundSource.isPlaying)
                    {
                        PlaySound(revDownSoundSource);
                    }
                }
                break;
                
            case GunState.RevvingUp:
                PlaySound(revUpSoundSource);
                break;
                
            case GunState.Firing:
                PlaySound(firingSoundSource); // Assumes 'Loop' is checked in Inspector
                break;
                
            case GunState.Overheated:
                // We were just firing, so the Stop() above handled it.
                PlaySound(revDownSoundSource);
                break;
        }
    }

    /// <summary>
    /// Checks the bullet fire rate cooldown and fires if ready. (FROM BULLET SPAWNING)
    /// </summary>
    void TryToFire()
    {
        if (fireCooldownTimer > 0) return; // Still on cooldown
        if (bulletPrefab == null || mainCam == null) return;

        // Ready to fire. Reset cooldown.
        fireCooldownTimer = 1f / shotsPerSecond;

        // --- Firing Logic ---
        Vector3 mouseWorldPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f;
        Vector2 mainDirection = (mouseWorldPosition - firePoint.position).normalized;
        float randomSpread = Random.Range(-spreadAngle, spreadAngle) * 0.5f;
        Vector2 fireDirection = Quaternion.Euler(0, 0, randomSpread) * mainDirection;

        FireBullet(fireDirection);
    }
    
    /// <summary>
    /// Updates the gun sprite's color based on current heat. (FROM BULLET SPAWNING)
    /// </summary>
    void UpdateOverheatVisuals()
    {
        if (gunSprite == null) return;
        
        // Calculate heat percentage (0.0 to 1.0)
        float heatPercent = currentHeat / maxFireTime;
        gunSprite.color = Color.Lerp(originalColor, overheatColor, heatPercent);
    }
    
    /// <summary>
    /// Helper function to play a sound clip from an AudioSource. (FROM BULLET SPAWNING)
    /// </summary>
    void PlaySound(AudioSource sourceToPlay)
    {
        if (sourceToPlay == null) return;
        sourceToPlay.Play();
    }

    /// <summary>
    /// Spawns a single bullet and sets its direction. (FROM BULLET SPAWNING)
    /// </summary>
    void FireBullet(Vector2 direction)
    {
        // 1. Calculate the rotation *for the sprite*
        //    (Assumes a RIGHT-facing bullet prefab)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 2. Instantiate the Bullet at the correct rotation
        // --- FIX: Removed the '- 90f'. This assumes a RIGHT-facing bullet. ---
        GameObject newProjectile = Instantiate(bulletPrefab, firePoint.position, Quaternion.Euler(0, 0, angle - 90f));

        // 3. Set the bullet's movement direction
        BulletFiring bulletScript = newProjectile.GetComponent<BulletFiring>();
        if (bulletScript != null)
        {
            bulletScript.SetDirection(direction);
        }
        else
        {
            Debug.LogWarning("Bullet Prefab is missing the 'BulletFiring.cs' script.");
        }
    }
}

