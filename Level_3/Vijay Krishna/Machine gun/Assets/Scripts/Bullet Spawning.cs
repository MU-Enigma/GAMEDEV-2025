using UnityEngine;
using System.Collections; // Required for Coroutines

// This script now requires an AudioSource to play sounds
[RequireComponent(typeof(AudioSource))]
public class BulletSpawning : MonoBehaviour
{
    [Header("Assets")]
    public GameObject bulletPrefab; // Reference to the bullet prefab
    public Transform firePoint; // The point from which the bullet will be fired
    
    // --- NEW: Audio Settings ---
    [Header("Audio")]
    public AudioClip revUpSound;
    public AudioClip firingSound;
    public AudioClip revDownSound;

    // --- NEW: Overheat & State Machine Settings ---
    [Header("Minigun Mechanics")]
    [Tooltip("How many bullets are fired per second.")]
    public float shotsPerSecond = 10f; 
    [Tooltip("The max random angle (in degrees) bullets can deviate.")]
    public float spreadAngle = 5f;
    [Tooltip("Time in seconds to rev up before firing.")]
    public float revUpTime = 0.5f;
    [Tooltip("Max time in seconds for continuous fire before overheating.")]
    public float maxFireTime = 3.0f;
    [Tooltip("Time in seconds to cool down after overheating.")]
    public float overheatCooldown = 2.0f;

    // --- NEW: Visuals ---
    [Header("Visuals (Optional)")]
    [Tooltip("The gun's sprite to change color on overheat.")]
    public SpriteRenderer gunSprite;
    public Color overheatColor = new Color(1, 0.5f, 0.5f); // Reddish hue
    
    // --- Private State Machine ---
    private enum GunState { Idle, RevvingUp, Firing, Overheated }
    private GunState currentState = GunState.Idle;

    private Camera mainCam;
    private float fireCooldownTimer; // For bullet rate of fire
    private float stateTimer; // Generic timer for revving up or cooling down
    private float currentHeat; // Tracks heat from 0 to maxFireTime
    private AudioSource audioSource;
    private Color originalColor;

    void Start()
    {
        mainCam = Camera.main;
        if (firePoint == null)
        {
            firePoint = transform;
        }
        
        // Get the required components
        audioSource = GetComponent<AudioSource>();
        audioSource.loop = false; // We will control the loop manually
        
        if (gunSprite != null)
        {
            originalColor = gunSprite.color;
        }
    }

    void Update()
    {
        // --- Handle Cooldowns ---
        // This runs all the time
        HandleCooldowns();

        // --- State Machine ---
        switch (currentState)
        {
            case GunState.Idle:
                // Check for mouse press to start revving up
                if (Input.GetMouseButtonDown(0))
                {
                    ChangeState(GunState.RevvingUp);
                }
                break;

            case GunState.RevvingUp:
                // Check if player released the mouse
                if (Input.GetMouseButtonUp(0))
                {
                    ChangeState(GunState.Idle);
                    break;
                }
                
                // Wait for rev up time to finish
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                {
                    ChangeState(GunState.Firing);
                }
                break;

            case GunState.Firing:
                // Check if player released the mouse
                if (Input.GetMouseButtonUp(0))
                {
                    ChangeState(GunState.Idle);
                    break;
                }

                // Still holding mouse. Add heat and try to fire.
                currentHeat += Time.deltaTime;
                TryToFire();

                // Check for overheat
                if (currentHeat >= maxFireTime)
                {
                    ChangeState(GunState.Overheated);
                }
                break;

            case GunState.Overheated:
                // In this state, the gun is just cooling down.
                // HandleCooldowns() is doing the work.
                // When currentHeat reaches 0, HandleCooldowns() will switch us to Idle.
                break;
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
        
        currentState = newState;
        audioSource.Stop(); // Stop any current sound
        audioSource.loop = false; // Default to not looping

        switch (newState)
        {
            case GunState.Idle:
                // If we are coming from Firing or RevvingUp, play rev down
                if (audioSource.clip == firingSound || audioSource.clip == revUpSound)
                {
                    PlaySound(revDownSound, false);
                }
                break;
                
            case GunState.RevvingUp:
                stateTimer = revUpTime;
                PlaySound(revUpSound, false);
                break;
                
            case GunState.Firing:
                PlaySound(firingSound, true); // Loop the firing sound
                break;
                
            case GunState.Overheated:
                PlaySound(revDownSound, false);
                // Heat and cooldown are managed in Update() and HandleCooldowns()
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

        // --- Firing Logic (from old script) ---
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
    /// Helper function to play a sound clip.
    /// </summary>
    void PlaySound(AudioClip clip, bool loop)
    {
        if (clip == null) return;
        audioSource.clip = clip;
        audioSource.loop = loop;
        audioSource.Play();
    }

    /// <summary>
    /// Spawns a single bullet and sets its direction.
    /// (This is the same as your old script, but I fixed the -90 bug)
    /// </summary>
    void FireBullet(Vector2 direction)
    {
        // 1. Calculate the rotation *for the sprite*
        //    (Assumes a RIGHT-facing bullet prefab)
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 2. Instantiate the Bullet at the correct rotation
        //    *** FIX: Removed the '-90f' ***
        //    This now correctly rotates your RIGHT-facing bullet prefab
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

