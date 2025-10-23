using UnityEngine;
using System.Collections; // Required for Coroutines

public class BulletSpawning : MonoBehaviour
{
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

    private Camera mainCam;
    private float fireCooldownTimer; // For bullet rate of fire
    // --- REMOVED: private float stateTimer; ---
    private float currentHeat; // Tracks heat from 0 to maxFireTime
    private Color originalColor;
    
    // --- NEW: Coroutine reference ---
    private Coroutine fireSequenceCoroutine;

    void Start()
    {
        mainCam = Camera.main;
        if (firePoint == null)
        {
            firePoint = transform;
        }
        
        if (gunSprite != null)
        {
            originalColor = gunSprite.color;
        }
    }

    void Update()
    {
        // This runs all the time
        HandleCooldowns();

        // --- SIMPLIFIED: State machine logic is now in the coroutine ---
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
    }

    // --- NEW: Coroutine to handle the entire firing sequence ---
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
    /// Manages all timers: bullet rate of fire and heat cooling.
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
    /// Central hub for changing states and playing sounds.
    /// </summary>
    void ChangeState(GunState newState)
    {
        if (currentState == newState) return;
        
        GunState previousState = currentState; // Store the old state
        currentState = newState;
        
        // --- NEW: Stop the coroutine if we're forced into Idle or Overheated ---
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
    /// Checks the bullet fire rate cooldown and fires if ready.
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
    /// Updates the gun sprite's color based on current heat.
    /// </summary>
    void UpdateOverheatVisuals()
    {
        if (gunSprite == null) return;
        
        // Calculate heat percentage (0.0 to 1.0)
        float heatPercent = currentHeat / maxFireTime;
        gunSprite.color = Color.Lerp(originalColor, overheatColor, heatPercent);
    }
    
    /// <summary>
    /// Helper function to play a sound clip from an AudioSource.
    /// </summary>
    void PlaySound(AudioSource sourceToPlay)
    {
        if (sourceToPlay == null) return;
        sourceToPlay.Play();
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

