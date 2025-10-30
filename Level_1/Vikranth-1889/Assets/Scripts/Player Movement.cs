using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    // --- Component References ---
    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer; 
    
    [Header("Visuals Setup")]
    [Tooltip("Drag the child GameObject that holds the sprite here.")]
    public Transform spriteTransform;
    [Tooltip("Drag the child GameObject that holds the shadow sprite here.")]
    public Transform shadowTransform;

    [Header("Movement")]
    public float moveSpeed = 5f;
    public float acceleration = 50f;
    public float deceleration = 100f;
    private Vector2 moveInput;

    [Header("Seed Power-Up")]
    public float seedBoostDuration = 2.5f;
    public float seedBoostSpeedMultiplier = 1.5f;
    public float seedBurstStrength = 10f;
    [SerializeField] private int seedsCollected = 0;
    private float seedBoostTimer = 0f;
    private bool isSpeedBoosted = false;

    [Header("Flight System")]
    public int maxFeathers = 5;
    [SerializeField] private int currentFeathers = 5;
    public GameObject flightMarkerPrefab;
    public GameObject crosshairPrefab;
    public float flightUpSpeed = 20f;
    public float flightDownSpeed = 25f;
    public float flightHeight = 15f;
    public float cancelHoldTime = 1f;
    public float crosshairSpeed = 8f;
    public float crosshairMaxRange = 15f;
    private GameObject currentMarker;
    private GameObject crosshair;
    private Camera mainCamera;
    private Vector2 crosshairPosition;
    private bool isFlying = false;
    private bool canCollectPowerups = true;
    private float flyButtonHoldTime = 0f;
    private bool isFlyButtonHeld = false;
    private bool flyButtonPressed = false;
    private Vector2 lastStickInput = Vector2.zero;
    private bool crosshairActive = false;
    private const float STICK_DEADZONE = 0.3f;
    private const float STICK_MOVEMENT_THRESHOLD = 0.1f;
    private enum FlightState { None, FlyingUp, CameraTransition, FlyingDown, Landing }
    private FlightState flightState = FlightState.None;
    private bool markerCancelTriggered = false; // Prevents double-cancel

    // New variables for cursor anchoring
    private bool isStickInMotion = false;
    private Vector2 lockedRelativeOffset = Vector2.zero;  // Cursor position relative to player
    private bool isCursorLocked = false;  // Whether cursor moves with player
    private const float STICK_RELEASE_DELAY = 0.1f;  // Small delay before locking
    private float stickReleaseTimer = 0f;

    [Header("Feather Drop Animation")]
    [Tooltip("How high the dropped feather bounces.")]
    public float featherDropBounceHeight = 1f;
    [Tooltip("Duration of the bounce animation.")]
    public float featherDropBounceDuration = 0.5f;
    [Tooltip("Number of bounces before settling.")]
    public int featherDropBounceCount = 2;
    [Tooltip("How much each bounce decreases in height.")]
    [Range(0.1f, 0.9f)]
    public float featherBounceDamping = 0.6f;

    [Header("Debug Collectible Spawning")]
    [Tooltip("Feather collectible prefab to spawn with P+1 or Y+Left.")]
    public GameObject featherCollectiblePrefab;
    [Tooltip("Dash chain upgrade prefab to spawn with P+2 or Y+Right.")]
    public GameObject dashChainCollectiblePrefab;
    [Tooltip("Height above player to spawn collectibles.")]
    public float collectibleSpawnHeight = 1.5f;

    [Header("Dashing")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;
    public float maxChargeTime = 1f;
    public float chargedDashSpeedMultiplier = 1.5f;
    public float chargedDashDurationMultiplier = 1.5f;

    [Header("Dash Cancel & Chain")]
    public bool canCancelDash = true;
    public bool canChainDash = true;
    public int maxChainDashes = 2;
    public int absoluteMaxChainDashes = 4;
    public float chainDashSpeedMultiplier = 1.2f;
    public float chainDashDuration = 0.15f;
    public float dashBrakeRecoveryTime = 0.15f;

    [Header("Chain Dash Grace Period")]
    public float chainGracePeriod = 0.25f;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;
    private bool isChargingDash = false;
    private float dashChargeTimer = 0f;
    private Coroutine executeDashCoroutine;
    private int chainDashCount = 0;
    private bool isInChainGracePeriod = false;
    private float chainGraceTimer = 0f;

    [Header("Visuals & Pooling")]
    public GameObject afterImagePrefab;
    public float timeBetweenAfterImages = 0.05f;
    public int afterImagePoolSize = 20;
    public float afterImageScaleMultiplier = 1.0f;
    private List<GameObject> afterImagePool;

    [Header("Movement Animations")]
    public float bounceHeight = 0.2f;
    public float bounceSpeedMultiplier = 2f;
    public float bounceStretchX = 1.1f;
    public float bounceSquashY = 0.9f;
    public float bounceSquashX = 0.9f;
    public float bounceStretchY = 1.1f;
    public float flightLandSquashX = 0.6f;
    public float flightLandStretchY = 1.4f;
    public float flightLandAnimDuration = 0.2f;

    [Header("Idle Animations")]
    public float idleBreatheAmount = 0.03f;
    public float idleBreatheSpeed = 2f;
    public float idleSquashMultiplierX = 1.0f;
    public float idleStretchMultiplierY = 1.0f;

    [Header("Dash Charge Animations")]
    public float maxChargeSquash = 1.4f;
    public float maxChargeStretch = 0.6f;
    public float maxChargeVibration = 0.1f;
    public float maxChargeRotation = 2f;

    [Header("Dash Recovery Animations")]
    public float dashRecoveryStretch = 1.3f;
    public float dashRecoverySquash = 0.7f;
    public float dashBrakeSquashX = 0.8f;
    public float dashBrakeStretchY = 1.2f;

    [Header("Shadow")]
    public Vector3 shadowOffset = new Vector3(0, -0.3f, 0);
    public float shadowScaleMax = 1f;
    public float shadowScaleMin = 0.8f;

    [Header("Controller Rumble")]
    [Range(0,1)]
    public float dashLowRumble = 0.5f;
    [Range(0,1)]
    public float dashHighRumble = 0.5f;
    [Range(0,1)]
    public float maxChargeLowRumble = 0.25f;
    [Range(0,1)]
    public float maxChargeHighRumble = 0.75f;
    [Range(0,1)]
    public float flightLandRumble = 0.8f;
    public float flightLandRumbleDuration = 0.3f;

    private const float INPUT_THRESHOLD = 0.1f;
    private const float CHARGE_COMPLETE_THRESHOLD = 1.0f;
    private const float ANIMATION_LERP_SPEED = 10f;
    private const float CHARGE_LERP_SPEED = 8f;
    private const float ROTATION_LERP_SPEED = 15f;
    private const float VIBRATION_LERP_SPEED = 20f;
    private Vector3 originalScale;
    private Vector3 originalShadowScale;
    private float animationTimer = 0f;
    private bool isApplyingEffect = false;
    private float lastMoveDirX = 0f;
    private bool isRumbling = false;
    private float unscaledSpriteHeight;
    private Vector2 aimDirection;

    public static System.Action<int> OnSeedCollected;
    public static System.Action<int> OnFeatherCountChanged;
    public static System.Action<int> OnChainDashLevelChanged;
    public static System.Action<bool> OnSpeedBoostChanged;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        mainCamera = Camera.main;
        if (mainCamera == null) mainCamera = FindFirstObjectByType<Camera>();

        if (spriteTransform != null)
        {
            spriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
            if (spriteRenderer.sprite != null)
                unscaledSpriteHeight = spriteRenderer.sprite.bounds.size.y;
            AutoResizeSpriteToCollider();
        }
        else
        {
            Debug.LogError("Sprite Transform is not assigned in the PlayerController inspector!");
        }
        
        if (shadowTransform != null)
        {
            originalShadowScale = shadowTransform.localScale;
            Debug.Log($"Shadow initialized with scale: {originalShadowScale}");
        }
        else
        {
            Debug.LogWarning("Shadow Transform is not assigned in PlayerController!");
        }
    }
    
    void Start()
    {
        InitializeAfterImagePool();
        InitializeCrosshair();
        OnFeatherCountChanged?.Invoke(currentFeathers);
    }

    void Update()
    {
        if (!isFlying)
        {
            HandleInput();
            HandleSpriteFlip();
            HandleAnimations();
            HandleDashCooldown();
            HandleSeedBoost();
            HandleChainGracePeriod();
        }
        HandleFlightInput();
        UpdateCrosshair();
        HandleDebugCollectibleSpawn();
    }

    void FixedUpdate()
    {
        if (isFlying) return;
        if (isChargingDash)
            rb.linearVelocity = Vector2.zero;
        else if (!isDashing)
        {
            float effectiveSpeed = moveSpeed;
            if (isSpeedBoosted)
                effectiveSpeed *= seedBoostSpeedMultiplier;
            Vector2 targetVelocity = moveInput * effectiveSpeed;
            float accel = moveInput.magnitude > 0 ? acceleration : deceleration;
            rb.linearVelocity = Vector2.MoveTowards(rb.linearVelocity, targetVelocity, accel * Time.fixedDeltaTime);
        }
    }

    // --- Debug Collectible Spawning ---
    void HandleDebugCollectibleSpawn()
    {
        // Keyboard controls: Hold P and press 1 or 2
        if (Input.GetKey(KeyCode.P))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) && featherCollectiblePrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * collectibleSpawnHeight;
                Instantiate(featherCollectiblePrefab, spawnPos, Quaternion.identity);
                Debug.Log("Feather collectible spawned above player!");
            }
            if (Input.GetKeyDown(KeyCode.Alpha2) && dashChainCollectiblePrefab != null)
            {
                Vector3 spawnPos = transform.position + Vector3.up * collectibleSpawnHeight;
                Instantiate(dashChainCollectiblePrefab, spawnPos, Quaternion.identity);
                Debug.Log("Dash chain upgrade spawned above player!");
            }
        }

        // Controller controls: Hold Y and press left/right dpad
        if (Gamepad.current != null)
        {
            if (Gamepad.current.buttonNorth.isPressed)
            {
                if (Gamepad.current.dpad.left.wasPressedThisFrame && featherCollectiblePrefab != null)
                {
                    Vector3 spawnPos = transform.position + Vector3.up * collectibleSpawnHeight;
                    Instantiate(featherCollectiblePrefab, spawnPos, Quaternion.identity);
                    Debug.Log("Feather collectible spawned above player! (Controller)");
                }
                if (Gamepad.current.dpad.right.wasPressedThisFrame && dashChainCollectiblePrefab != null)
                {
                    Vector3 spawnPos = transform.position + Vector3.up * collectibleSpawnHeight;
                    Instantiate(dashChainCollectiblePrefab, spawnPos, Quaternion.identity);
                    Debug.Log("Dash chain upgrade spawned above player! (Controller)");
                }
            }
        }
    }

    // --- Flight System Methods ---

    void InitializeCrosshair()
    {
        if (crosshairPrefab != null)
        {
            crosshair = Instantiate(crosshairPrefab);
            crosshair.SetActive(false);
            crosshairPosition = transform.position;
        }
    }

    void HandleFlightInput()
    {
        bool flyButtonPressedThisFrame = false;
        bool flyButtonReleasedThisFrame = false;
        
        if (Gamepad.current != null)
        {
            Vector2 currentStickInput = Gamepad.current.rightStick.ReadValue();
            
            // Check if stick is in motion
            bool stickInMotionThisFrame = currentStickInput.magnitude > STICK_DEADZONE;
            
            // Handle right stick press for recentering
            if (Gamepad.current.buttonNorth.wasPressedThisFrame)
            {
                RecenterCursor();
            }
                            
            // Handle stick motion state changes
            if (stickInMotionThisFrame)
            {
                // Stick is moving - enter independent mode
                if (!isStickInMotion)
                {
                    isStickInMotion = true;
                    isCursorLocked = false;
                    stickReleaseTimer = 0f;
                    Debug.Log("Cursor: Independent mode");
                }
                
                // Update cursor position independently
                Vector2 stickDelta = currentStickInput - lastStickInput;
                if (stickDelta.magnitude > STICK_MOVEMENT_THRESHOLD || 
                    currentStickInput.magnitude > 0.8f || 
                    !crosshairActive)
                {
                    UpdateCrosshairPosition(currentStickInput);
                    if (!crosshairActive)
                    {
                        crosshairActive = true;
                        if (crosshair != null && !crosshair.activeInHierarchy)
                            crosshair.SetActive(true);
                    }
                }
            }
            else
            {
                // Stick released - start timer for locking
                if (isStickInMotion)
                {
                    isStickInMotion = false;
                    stickReleaseTimer = 0f;
                }
                
                // Count down to lock cursor
                if (!isCursorLocked)
                {
                    stickReleaseTimer += Time.deltaTime;
                    if (stickReleaseTimer >= STICK_RELEASE_DELAY)
                    {
                        LockCursorToPlayer();
                    }
                }
            }
            
            lastStickInput = currentStickInput;
            
            flyButtonPressedThisFrame = Gamepad.current.rightShoulder.wasPressedThisFrame;
            flyButtonReleasedThisFrame = Gamepad.current.rightShoulder.wasReleasedThisFrame;
            isFlyButtonHeld = Gamepad.current.rightShoulder.isPressed;
        }
        else
        {
            // Mouse input (always independent)
            Vector3 mousePos = Input.mousePosition;
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(mousePos.x, mousePos.y, mainCamera.nearClipPlane));
            crosshairPosition = new Vector2(worldPos.x, worldPos.y);
            isCursorLocked = false; // Mouse is always independent
            
            if (crosshair != null && !crosshair.activeInHierarchy)
                crosshair.SetActive(true);
            
            flyButtonPressedThisFrame = Input.GetKeyDown(KeyCode.X);
            flyButtonReleasedThisFrame = Input.GetKeyUp(KeyCode.X);
            isFlyButtonHeld = Input.GetKey(KeyCode.X);
            
            // Add keyboard recentering option
            if (Input.GetKeyDown(KeyCode.C))
            {
                RecenterCursor();
            }
        }
        
        // Track hold time while button is held
        if (isFlyButtonHeld)
        {
            flyButtonHoldTime += Time.deltaTime;
            
            // VISUAL FEEDBACK: Drop feather WHILE holding when cancel threshold reached
            if (flyButtonHoldTime >= cancelHoldTime && currentMarker != null && !markerCancelTriggered)
            {
                DropFeatherRefundAtMarker();
                markerCancelTriggered = true;
                // FIXED: Disable button press flag to prevent marker placement on release
                flyButtonPressed = false;
            }
        }
        
        // Handle button press (start of hold)
        if (flyButtonPressedThisFrame && !isFlying)
        {
            flyButtonPressed = true;
            flyButtonHoldTime = 0f;
            markerCancelTriggered = false; // Reset cancel trigger
        }
        
        // Handle button release (end of hold)
        if (flyButtonReleasedThisFrame && !isFlying && flyButtonPressed)
        {
            flyButtonPressed = false;
            
            if (markerCancelTriggered && currentMarker != null)
            {
                // Complete the cancellation (marker already dropped feather)
                CompleteMarkerCancellation();
            }
            else
            {
                // Normal flight behavior (only if consumable wasn't dropped)
                HandleFlyButtonPress();
            }
            
            flyButtonHoldTime = 0f;
        }
    }

    void LockCursorToPlayer()
    {
        if (!isCursorLocked)
        {
            // Calculate and store the relative offset from player
            lockedRelativeOffset = crosshairPosition - (Vector2)transform.position;
            
            // Clamp the offset to max range to prevent extreme positions
            if (lockedRelativeOffset.magnitude > crosshairMaxRange)
            {
                lockedRelativeOffset = lockedRelativeOffset.normalized * crosshairMaxRange;
            }
            
            isCursorLocked = true;
            Debug.Log($"Cursor locked at relative offset: {lockedRelativeOffset}");
        }
    }

    void RecenterCursor()
    {
        crosshairPosition = transform.position;
        lockedRelativeOffset = Vector2.zero;
        isCursorLocked = true;
        crosshairActive = false;  // Make cursor inactive so it's hidden
        
        // Hide the cursor when recentered
        if (crosshair != null)
            crosshair.SetActive(false);
            
        Debug.Log("Cursor recentered to player position (hidden)");
    }


    void DropFeatherRefundAtMarker()
    {
        if (currentMarker != null && featherCollectiblePrefab != null)
        {
            Vector3 markerPosition = currentMarker.transform.position;
            GameObject droppedFeather = Instantiate(featherCollectiblePrefab, markerPosition, Quaternion.identity);
            
            // Add bounce animation to the dropped feather
            StartCoroutine(AnimateFeatherDrop(droppedFeather));
            
            // FIXED: Remove marker immediately when feather is dropped
            RemoveFlightMarker();
            
            Debug.Log("Feather refund dropped at marker location! (Visual feedback)");
        }
    }

    void CompleteMarkerCancellation()
    {
        // Only destroy marker if it still exists (it might have been removed already)
        if (currentMarker != null)
        {
            Destroy(currentMarker);
            currentMarker = null;
            Debug.Log("Flight marker cancelled!");
        }
        markerCancelTriggered = false;
    }

    // NEW: Centralized marker removal method
    void RemoveFlightMarker()
    {
        if (currentMarker != null)
        {
            currentMarker.SetActive(false);
            Destroy(currentMarker);
            currentMarker = null;
        }
    }

    IEnumerator AnimateFeatherDrop(GameObject feather)
{
    if (feather == null) yield break;
    
    Vector3 startPos = feather.transform.position;
    float totalDuration = featherDropBounceDuration;
    float bounceInterval = totalDuration / featherDropBounceCount;
    
    for (int bounce = 0; bounce < featherDropBounceCount; bounce++)
    {
        // Check if feather was destroyed/collected
        if (feather == null)
        {
            Debug.Log("Feather was collected during bounce animation");
            yield break;
        }
        
        // Calculate bounce height (diminishing)
        float currentBounceHeight = featherDropBounceHeight * Mathf.Pow(featherBounceDamping, bounce);
        
        // Bounce up
        float halfInterval = bounceInterval * 0.5f;
        float elapsed = 0f;
        
        while (elapsed < halfInterval)
        {
            // Check if feather still exists before accessing transform
            if (feather == null)
            {
                Debug.Log("Feather collected during bounce up animation");
                yield break;
            }
            
            elapsed += Time.deltaTime;
            float t = elapsed / halfInterval;
            
            // Smooth curve up (ease out)
            float height = currentBounceHeight * (1f - (1f - t) * (1f - t));
            feather.transform.position = startPos + Vector3.up * height;
            
            yield return null;
        }
        
        // Bounce down
        elapsed = 0f;
        while (elapsed < halfInterval)
        {
            // Check if feather still exists before accessing transform
            if (feather == null)
            {
                Debug.Log("Feather collected during bounce down animation");
                yield break;
            }
            
            elapsed += Time.deltaTime;
            float t = elapsed / halfInterval;
            
            // Smooth curve down (ease in)
            float height = currentBounceHeight * (1f - t * t);
            feather.transform.position = startPos + Vector3.up * height;
            
            yield return null;
        }
    }
    
    // Final check and position set
    if (feather != null)
    {
        feather.transform.position = startPos;
    }
}


    void HandleFlyButtonPress()
    {
        // Execute flight from existing marker (feather already spent when marker was placed)
        if (currentMarker != null)
        {
            ExecuteFlight();
            return;
        }
        
        // Only check feathers when trying to PLACE a new marker
        if (currentFeathers <= 0)
        {
            Debug.Log("Cannot place flight marker: No feathers remaining!");
            return;
        }
        
        // Place new marker (this consumes a feather)
        PlaceFlightMarker();
    }

    void UpdateCrosshairPosition(Vector2 stickInput)
    {
        if (!crosshairActive)
            crosshairPosition = transform.position;
        Vector2 movement = stickInput * crosshairSpeed * Time.deltaTime;
        crosshairPosition += movement;
        Vector2 playerPos = transform.position;
        Vector2 directionFromPlayer = crosshairPosition - playerPos;
        if (directionFromPlayer.magnitude > crosshairMaxRange)
            crosshairPosition = playerPos + directionFromPlayer.normalized * crosshairMaxRange;
    }

    void UpdateCrosshair()
    {
        if (crosshair != null && crosshair.activeInHierarchy)
        {
            if (isCursorLocked)
            {
                // Move cursor with player using locked relative offset
                crosshairPosition = (Vector2)transform.position + lockedRelativeOffset;
            }
            
            crosshair.transform.position = crosshairPosition;
        }
    }

    void ResetCrosshairToPlayer()
    {
        crosshairPosition = transform.position;
        crosshairActive = false;
        lastStickInput = Vector2.zero;
        lockedRelativeOffset = Vector2.zero;
        isCursorLocked = false;
        isStickInMotion = false;
        stickReleaseTimer = 0f;
    }

    void PlaceFlightMarker()
    {
        if (currentFeathers <= 0) return;
        if (currentMarker != null)
            Destroy(currentMarker);
        currentFeathers--;
        OnFeatherCountChanged?.Invoke(currentFeathers);
        if (flightMarkerPrefab != null)
        {
            currentMarker = Instantiate(flightMarkerPrefab, crosshairPosition, Quaternion.identity);
            StartCoroutine(AnimateFeatherToMarker());
        }
    }

    void ExecuteFlight()
    {
        if (currentMarker == null) return;
        Vector2 targetPosition = currentMarker.transform.position;
        StartCoroutine(ExecuteFlightSequence(targetPosition));
    }

    IEnumerator AnimateFeatherToMarker()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = crosshairPosition;
        float duration = 0.5f;
        GameObject feather = new GameObject("Flying Feather");
        SpriteRenderer featherSprite = feather.AddComponent<SpriteRenderer>();
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / duration;
            feather.transform.position = Vector3.Lerp(startPos, endPos, progress);
            yield return null;
        }
        Destroy(feather);
    }

    IEnumerator ExecuteFlightSequence(Vector2 targetPosition)
    {
        isFlying = true;
        canCollectPowerups = false;
        flightState = FlightState.FlyingUp;
        SmoothCameraFollow cameraController = FindFirstObjectByType<SmoothCameraFollow>();
        if (cameraController != null)
            cameraController.StopFollowing();
        if (crosshair != null)
            crosshair.SetActive(false);
        crosshairActive = false;
        
        Vector3 startPosition = transform.position;
        Vector3 flyUpTarget = startPosition + Vector3.up * flightHeight;
        float flyUpTime = flightHeight / flightUpSpeed;
        float elapsed = 0f;
        while (elapsed < flyUpTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / flyUpTime;
            transform.position = Vector3.Lerp(startPosition, flyUpTarget, progress);
            yield return null;
        }
        
        flightState = FlightState.CameraTransition;
        if (cameraController != null)
            yield return StartCoroutine(cameraController.TransitionToPosition(targetPosition, 0.5f));
        else
            mainCamera.transform.position = new Vector3(targetPosition.x, targetPosition.y, mainCamera.transform.position.z);
        
        flightState = FlightState.FlyingDown;
        Vector3 flyDownStart = new Vector3(targetPosition.x, targetPosition.y + flightHeight, transform.position.z);
        Vector3 flyDownTarget = new Vector3(targetPosition.x, targetPosition.y, transform.position.z);
        transform.position = flyDownStart;
        float flyDownTime = flightHeight / flightDownSpeed;
        elapsed = 0f;
        while (elapsed < flyDownTime)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / flyDownTime;
            transform.position = Vector3.Lerp(flyDownStart, flyDownTarget, progress);
            yield return null;
        }
        
        flightState = FlightState.Landing;
        transform.position = flyDownTarget;
        if (currentMarker != null)
        {
            Destroy(currentMarker);
            currentMarker = null;
        }
        yield return StartCoroutine(FlightLandingEffects());
        isFlying = false;
        canCollectPowerups = true;
        flightState = FlightState.None;
        if (cameraController != null)
            cameraController.ResumeFollowing();
        ResetCrosshairToPlayer();
    }

    IEnumerator FlightLandingEffects()
    {
        SmoothCameraFollow cameraController = FindFirstObjectByType<SmoothCameraFollow>();
        if (cameraController != null)
            cameraController.ShakeCamera(0.5f, 0.3f);
        StartCoroutine(FlightLandingRumble());
        yield return StartCoroutine(ApplyScaleEffect(spriteTransform, flightLandSquashX, flightLandStretchY, flightLandAnimDuration));
    }

    IEnumerator FlightLandingRumble()
    {
        if (Gamepad.current != null)
        {
            Gamepad.current.SetMotorSpeeds(flightLandRumble, flightLandRumble);
            yield return new WaitForSeconds(flightLandRumbleDuration);
            Gamepad.current.SetMotorSpeeds(0f, 0f);
        }
    }

    // --- Power-Up Methods ---

    public void ApplySeedBoost()
    {
        if (!canCollectPowerups) return;
        seedsCollected++;
        if (moveInput.magnitude > INPUT_THRESHOLD)
            rb.linearVelocity = moveInput.normalized * seedBurstStrength;
        else
        {
            Vector2 burstDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
            rb.linearVelocity = burstDirection * seedBurstStrength;
        }
        isSpeedBoosted = true;
        seedBoostTimer = seedBoostDuration;
        OnSeedCollected?.Invoke(seedsCollected);
        OnSpeedBoostChanged?.Invoke(isSpeedBoosted);
    }

    public void UpgradeChainDash()
    {
        if (maxChainDashes < absoluteMaxChainDashes)
        {
            maxChainDashes++;
            OnChainDashLevelChanged?.Invoke(maxChainDashes);
            Debug.Log($"Chain Dash upgraded! New max: {maxChainDashes} dashes.");
        }
        else
        {
            Debug.Log("Chain Dash already at maximum level!");
        }
    }

    public void AddFeathers(int amount)
    {
        currentFeathers = Mathf.Min(currentFeathers + amount, maxFeathers);
        OnFeatherCountChanged?.Invoke(currentFeathers);
        Debug.Log($"Feathers added! Current: {currentFeathers}/{maxFeathers}");
    }

    // --- Getters ---
    public int GetSeedsCollected() => seedsCollected;
    public int GetCurrentChainDashLevel() => maxChainDashes;
    public bool CanUpgradeChainDash() => maxChainDashes < absoluteMaxChainDashes;
    public int GetCurrentFeathers() => currentFeathers;
    public int GetMaxFeathers() => maxFeathers;
    public bool CanCollectPowerups() => canCollectPowerups;

    public void ResetSeedCounter()
    {
        seedsCollected = 0;
        OnSeedCollected?.Invoke(seedsCollected);
    }

    public void SetSeedCount(int count)
    {
        seedsCollected = Mathf.Max(0, count);
        OnSeedCollected?.Invoke(seedsCollected);
    }

    void HandleSeedBoost()
    {
        bool wasSpeedBoosted = isSpeedBoosted;
        
        if (isSpeedBoosted)
        {
            seedBoostTimer -= Time.deltaTime;
            if (seedBoostTimer <= 0f)
            {
                isSpeedBoosted = false;
            }
        }
        
        // Notify UI when speed boost state changes
        if (wasSpeedBoosted != isSpeedBoosted)
        {
            OnSpeedBoostChanged?.Invoke(isSpeedBoosted);
        }
    }

    void HandleChainGracePeriod()
    {
        if (isInChainGracePeriod)
        {
            chainGraceTimer -= Time.deltaTime;
            if (chainGraceTimer <= 0f)
            {
                isInChainGracePeriod = false;
                StartDashCooldown();
            }
        }
    }

    void StartDashCooldown()
    {
        dashCooldownTimer = dashCooldown;
        chainDashCount = 0;
    }

    void HandleDashCooldown()
    {
        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;
    }

    void InitializeAfterImagePool()
    {
        if (afterImagePrefab == null) return;
        afterImagePool = new List<GameObject>();
        for (int i = 0; i < afterImagePoolSize; i++)
        {
            GameObject obj = Instantiate(afterImagePrefab);
            obj.SetActive(false);
            afterImagePool.Add(obj);
        }
    }

    void HandleInput()
    {
        if (Gamepad.current != null)
        {
            moveInput = Gamepad.current.leftStick.ReadValue();
            if (Gamepad.current.buttonSouth.wasPressedThisFrame)
                HandleDashInput();
            if (isChargingDash && Gamepad.current.buttonSouth.isPressed)
            {
                dashChargeTimer += Time.deltaTime;
                dashChargeTimer = Mathf.Clamp(dashChargeTimer, 0, maxChargeTime);
            }
            if (isChargingDash && Gamepad.current.buttonSouth.wasReleasedThisFrame)
            {
                isChargingDash = false;
                SetControllerRumble(0f, 0f);
                executeDashCoroutine = StartCoroutine(ExecuteDash());
            }
        }
        else
        {
            moveInput.x = Input.GetAxisRaw("Horizontal");
            moveInput.y = Input.GetAxisRaw("Vertical");
            moveInput.Normalize();
            if (Input.GetButtonDown("Dash"))
                HandleDashInput();
            if (isChargingDash && Input.GetButton("Dash"))
            {
                dashChargeTimer += Time.deltaTime;
                dashChargeTimer = Mathf.Clamp(dashChargeTimer, 0, maxChargeTime);
            }
            if (isChargingDash && Input.GetButtonUp("Dash"))
            {
                isChargingDash = false;
                executeDashCoroutine = StartCoroutine(ExecuteDash());
            }
        }
    }

    void HandleDashInput()
    {
        if (isDashing && canCancelDash)
            HandleDashCancel();
        else if (isInChainGracePeriod && canChainDash && chainDashCount > 0)
            HandleChainDashFromGrace();
        else if (!isChargingDash && !isDashing && dashCooldownTimer <= 0)
        {
            chainDashCount = 0;
            isChargingDash = true;
            dashChargeTimer = 0f;
            aimDirection = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        }
    }

    void HandleSpriteFlip()
    {
        if (isDashing || isChargingDash) return;
        if (moveInput.x > 0) spriteRenderer.flipX = false;
        else if (moveInput.x < 0) spriteRenderer.flipX = true;
        if (moveInput.x != 0) lastMoveDirX = moveInput.x;
    }

    void HandleAnimations()
    {
        if (isDashing || isApplyingEffect || spriteTransform == null) return;
        UpdateShadowPosition();
        if (isChargingDash)
            HandleChargeAnimation();
        else
        {
            ResetSpriteRotation();
            if (rb.linearVelocity.magnitude > INPUT_THRESHOLD)
                HandleMovementAnimation();
            else
                HandleIdleAnimation();
        }
    }

    void UpdateShadowPosition()
    {
        if (shadowTransform != null)
        {
            shadowTransform.localPosition = shadowOffset;
        }
    }

    void ResetSpriteRotation()
    {
        spriteTransform.localRotation = Quaternion.Lerp(
            spriteTransform.localRotation, 
            Quaternion.identity, 
            Time.deltaTime * ANIMATION_LERP_SPEED
        );
    }

    void HandleChargeAnimation()
    {
        float chargePercent = dashChargeTimer / maxChargeTime;
        ApplyChargeScale(chargePercent);
        ApplyChargeRotation();
        ApplyChargeEffects(chargePercent);
        ApplyChargeShadow();
    }

    void ApplyChargeScale(float chargePercent)
    {
        float scaleX = Mathf.Lerp(originalScale.x, originalScale.x * maxChargeSquash, chargePercent);
        float scaleY = Mathf.Lerp(originalScale.y, originalScale.y * maxChargeStretch, chargePercent);
        Vector3 targetScale = new Vector3(scaleX, scaleY, originalScale.z);

        spriteTransform.localScale = Vector3.Lerp(
            spriteTransform.localScale, 
            targetScale, 
            Time.deltaTime * CHARGE_LERP_SPEED
        );
    }

    void ApplyChargeRotation()
    {
        Quaternion targetRotation = spriteTransform.localRotation;
        if (moveInput.magnitude > INPUT_THRESHOLD)
        {
            aimDirection = moveInput.normalized;
            float angle = CalculateAimAngle(moveInput);
            targetRotation = Quaternion.Euler(0, 0, angle);
        }
        spriteTransform.localRotation = Quaternion.Slerp(
            spriteTransform.localRotation, 
            targetRotation, 
            Time.deltaTime * ROTATION_LERP_SPEED
        );
    }

    float CalculateAimAngle(Vector2 input)
    {
        if (input.x < 0)
        {
            spriteRenderer.flipX = true;
            return Mathf.Atan2(-input.y, -input.x) * Mathf.Rad2Deg;
        }
        else
        {
            spriteRenderer.flipX = false;
            return Mathf.Atan2(input.y, input.x) * Mathf.Rad2Deg;
        }
    }

    void ApplyChargeEffects(float chargePercent)
    {
        if (chargePercent >= CHARGE_COMPLETE_THRESHOLD)
        {
            float randomRotationOffset = Random.Range(-maxChargeRotation, maxChargeRotation);
            spriteTransform.localRotation *= Quaternion.Euler(0, 0, randomRotationOffset);

            float offsetX = Random.Range(-maxChargeVibration, maxChargeVibration);
            float offsetY = Random.Range(-maxChargeVibration, maxChargeVibration);
            Vector3 targetPosition = new Vector3(offsetX, offsetY, 0);
            spriteTransform.localPosition = Vector3.Lerp(
                spriteTransform.localPosition, 
                targetPosition, 
                Time.deltaTime * VIBRATION_LERP_SPEED
            );
        }
        else
        {
            spriteTransform.localPosition = Vector3.Lerp(
                spriteTransform.localPosition, 
                Vector3.zero, 
                Time.deltaTime * CHARGE_LERP_SPEED
            );
        }
        float currentLowRumble = Mathf.Lerp(0f, maxChargeLowRumble, chargePercent);
        float currentHighRumble = Mathf.Lerp(0f, maxChargeHighRumble, chargePercent);
        SetControllerRumble(currentLowRumble, currentHighRumble);
    }

    void ApplyChargeShadow()
    {
        if (shadowTransform != null && originalShadowScale != Vector3.zero)
        {
            Vector3 targetShadowScale = originalShadowScale * shadowScaleMax;
            shadowTransform.localScale = Vector3.Lerp(
                shadowTransform.localScale, 
                targetShadowScale, 
                Time.deltaTime * CHARGE_LERP_SPEED
            );
        }
    }

    void HandleMovementAnimation()
    {
        animationTimer += Time.deltaTime * rb.linearVelocity.magnitude * bounceSpeedMultiplier;
        float bounceLerp = Mathf.Abs(Mathf.Sin(animationTimer));
        ApplyBouncePosition(bounceLerp);
        ApplyBounceScale(bounceLerp);
        ApplyBounceShadow(bounceLerp);
    }

    void ApplyBouncePosition(float bounceLerp)
    {
        float yOffset = bounceLerp * bounceHeight;
        Vector3 targetPosition = new Vector3(0, yOffset, 0);
        spriteTransform.localPosition = Vector3.Lerp(
            spriteTransform.localPosition, 
            targetPosition, 
            Time.deltaTime * ANIMATION_LERP_SPEED
        );
    }

    void ApplyBounceScale(float bounceLerp)
    {
        float scaleX = Mathf.Lerp(originalScale.x * bounceSquashX, originalScale.x * bounceStretchX, bounceLerp);
        float scaleY = Mathf.Lerp(originalScale.y * bounceStretchY, originalScale.y * bounceSquashY, bounceLerp);
        Vector3 targetScale = new Vector3(scaleX, scaleY, originalScale.z);

        spriteTransform.localScale = Vector3.Lerp(
            spriteTransform.localScale, 
            targetScale, 
            Time.deltaTime * ANIMATION_LERP_SPEED
        );
    }

    void ApplyBounceShadow(float bounceLerp)
    {
        if (shadowTransform != null && originalShadowScale != Vector3.zero)
        {
            float shadowScale = Mathf.Lerp(shadowScaleMax, shadowScaleMin, bounceLerp);
            Vector3 targetShadowScale = originalShadowScale * shadowScale;
            shadowTransform.localScale = Vector3.Lerp(
                shadowTransform.localScale, 
                targetShadowScale, 
                Time.deltaTime * ANIMATION_LERP_SPEED
            );
        }
    }

    void HandleIdleAnimation()
    {
        animationTimer += Time.deltaTime * idleBreatheSpeed;
        float breatheAmount = Mathf.Sin(animationTimer) * idleBreatheAmount;
        ApplyIdleScale(breatheAmount);
        ApplyIdlePosition(breatheAmount);
        ApplyIdleShadow();
    }

    void ApplyIdleScale(float breatheAmount)
    {
        float scaleX = originalScale.x + (breatheAmount * idleSquashMultiplierX);
        float scaleY = originalScale.y - (breatheAmount * idleStretchMultiplierY);
        Vector3 targetScale = new Vector3(scaleX, scaleY, originalScale.z);

        spriteTransform.localScale = Vector3.Lerp(
            spriteTransform.localScale, 
            targetScale, 
            Time.deltaTime * ANIMATION_LERP_SPEED
        );
    }

    void ApplyIdlePosition(float breatheAmount)
    {
        float yOffset = -(unscaledSpriteHeight / 2f) * (originalScale.y - (originalScale.y - breatheAmount * idleStretchMultiplierY));
        Vector3 targetPosition = new Vector3(0, yOffset, 0);

        spriteTransform.localPosition = Vector3.Lerp(
            spriteTransform.localPosition, 
            targetPosition, 
            Time.deltaTime * ANIMATION_LERP_SPEED
        );
    }

    void ApplyIdleShadow()
    {
        if (shadowTransform != null && originalShadowScale != Vector3.zero)
        {
            shadowTransform.localScale = Vector3.Lerp(
                shadowTransform.localScale, 
                originalShadowScale * shadowScaleMax, 
                Time.deltaTime * ANIMATION_LERP_SPEED
            );
        }
    }
    
    void AutoResizeSpriteToCollider()
    {
        if (spriteRenderer == null || spriteRenderer.sprite == null) return;
        Vector2 colliderSize = Vector2.zero;
        bool colliderFound = false;
        BoxCollider2D boxCollider = GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            colliderSize = boxCollider.size;
            colliderFound = true;
        }
        else
        {
            CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();
            if (capsuleCollider != null)
            {
                colliderSize = capsuleCollider.size;
                colliderFound = true;
            }
            else
            {
                CircleCollider2D circleCollider = GetComponent<CircleCollider2D>();
                if (circleCollider != null)
                {
                    float diameter = circleCollider.radius * 2;
                    colliderSize = new Vector2(diameter, diameter);
                    colliderFound = true;
                }
            }
        }
        if (!colliderFound) return;
        Vector2 spriteSize = spriteRenderer.sprite.bounds.size;
        Vector2 newScale = new Vector2(colliderSize.x / spriteSize.x, colliderSize.y / spriteSize.y);
        spriteTransform.localScale = new Vector3(newScale.x, newScale.y, 1f);
        originalScale = spriteTransform.localScale;
    }

    void HandleDashCancel()
    {
        if (canChainDash && moveInput.magnitude > INPUT_THRESHOLD && chainDashCount < maxChainDashes)
        {
            if (executeDashCoroutine != null) StopCoroutine(executeDashCoroutine);
            executeDashCoroutine = StartCoroutine(ExecuteChainDash());
        }
        else 
        {
            if (executeDashCoroutine != null) StopCoroutine(executeDashCoroutine);
            StopDash();
            rb.linearVelocity = Vector2.zero;
            SetControllerRumble(0f, 0f);
            StartCoroutine(DashBrakeEffect());
        }
    }

    void HandleChainDashFromGrace()
    {
        if (canChainDash && moveInput.magnitude > INPUT_THRESHOLD && chainDashCount < maxChainDashes)
        {
            isInChainGracePeriod = false;
            executeDashCoroutine = StartCoroutine(ExecuteChainDash());
        }
    }

    void StartDash(float currentDashSpeed, Vector2 dashDirection)
    {
        isDashing = true;
        rb.linearVelocity = dashDirection * currentDashSpeed;
    }

    void StopDash()
    {
        isDashing = false;
        if (moveInput.magnitude < INPUT_THRESHOLD)
            rb.linearVelocity = Vector2.zero;
        if (chainDashCount > 0 && chainDashCount < maxChainDashes && canChainDash)
        {
            isInChainGracePeriod = true;
            chainGraceTimer = chainGracePeriod;
        }
        else
            StartDashCooldown();
    }

    GameObject GetPooledAfterImage()
    {
        for (int i = 0; i < afterImagePool.Count; i++)
            if (!afterImagePool[i].activeInHierarchy)
                return afterImagePool[i];
        return null; 
    }

    void SetControllerRumble(float lowFrequency, float highFrequency)
    {
        if (Gamepad.current == null) return;
        if (!isRumbling && (lowFrequency > 0 || highFrequency > 0))
        {
            isRumbling = true;
            Gamepad.current.PauseHaptics();
            Gamepad.current.ResumeHaptics();
        }
        else if (isRumbling && lowFrequency == 0 && highFrequency == 0)
        {
            isRumbling = false;
        }
        Gamepad.current.SetMotorSpeeds(lowFrequency, highFrequency);
    }

    IEnumerator SpawnDashTrail(float dashDuration)
    {
        float timer = 0f;
        while (timer < dashDuration)
        {
            GameObject afterImage = GetPooledAfterImage();
            if (afterImage != null)
            {
                afterImage.transform.position = transform.position;
                afterImage.transform.rotation = spriteTransform.rotation;
                afterImage.transform.localScale = spriteTransform.localScale * afterImageScaleMultiplier;
                afterImage.SetActive(true);
                SpriteRenderer afterImageRenderer = afterImage.GetComponent<SpriteRenderer>();
                afterImageRenderer.sprite = spriteRenderer.sprite;
                afterImageRenderer.flipX = spriteRenderer.flipX;
                AfterImage afterImageScript = afterImage.GetComponent<AfterImage>();
                if (afterImageScript != null)
                {
                    afterImageScript.StartFade();
                }
            }
            yield return new WaitForSeconds(timeBetweenAfterImages);
            timer += timeBetweenAfterImages;
        }
    }

    IEnumerator ApplyScaleEffect(Transform targetTransform, float xScale, float yScale, float duration)
    {
        isApplyingEffect = true;
        targetTransform.localScale = new Vector3(originalScale.x * xScale, originalScale.y * yScale, originalScale.z);
        yield return new WaitForSeconds(duration);
        targetTransform.localScale = originalScale;
        isApplyingEffect = false;
    }

    IEnumerator ExecuteDash()
    {
        chainDashCount = 1; 
        float chargePercentExecute = dashChargeTimer / maxChargeTime;
        float currentDashSpeed = Mathf.Lerp(dashSpeed, dashSpeed * chargedDashSpeedMultiplier, chargePercentExecute);
        float currentDashDuration = Mathf.Lerp(dashDuration, dashDuration * chargedDashDurationMultiplier, chargePercentExecute);
        Vector2 dashDirection = aimDirection;
        SetControllerRumble(dashLowRumble, dashHighRumble);
        StartCoroutine(SpawnDashTrail(currentDashDuration));
        StartDash(currentDashSpeed, dashDirection);
        yield return new WaitForSeconds(currentDashDuration);
        StopDash();
        SetControllerRumble(0f, 0f);
        StartCoroutine(ApplyScaleEffect(spriteTransform, dashRecoverySquash, dashRecoveryStretch, 0.1f));
    }

    IEnumerator ExecuteChainDash()
    {
        chainDashCount++;
        float currentDashSpeed = dashSpeed * chainDashSpeedMultiplier;
        float currentDashDuration = chainDashDuration;
        Vector2 dashDirection = moveInput.normalized;
        float angle = CalculateAimAngle(dashDirection);
        spriteTransform.localRotation = Quaternion.Euler(0, 0, angle);
        SetControllerRumble(dashLowRumble, dashHighRumble);
        StartCoroutine(SpawnDashTrail(currentDashDuration));
        StartDash(currentDashSpeed, dashDirection);
        yield return new WaitForSeconds(currentDashDuration);
        StopDash();
        SetControllerRumble(0f, 0f);
        StartCoroutine(ApplyScaleEffect(spriteTransform, dashRecoverySquash, dashRecoveryStretch, 0.1f));
    }

    IEnumerator DashBrakeEffect()
    {
        StartCoroutine(ApplyScaleEffect(spriteTransform, dashBrakeSquashX, dashBrakeStretchY, dashBrakeRecoveryTime));
        yield return new WaitForSeconds(dashBrakeRecoveryTime);
    }

    // Debug methods
    [ContextMenu("Debug Shadow Info")]
    void DebugShadowInfo()
    {
        Debug.Log($"Shadow Transform: {(shadowTransform != null ? "Found" : "NULL")}");
        if (shadowTransform != null)
        {
            Debug.Log($"Shadow Position: {shadowTransform.localPosition}");
            Debug.Log($"Shadow Scale: {shadowTransform.localScale}");
            Debug.Log($"Original Shadow Scale: {originalShadowScale}");
            Debug.Log($"Shadow Offset: {shadowOffset}");
        }
    }

    [ContextMenu("Test Crosshair Visibility")]
    void TestCrosshairVisibility()
    {
        if (crosshair != null)
        {
            crosshair.SetActive(!crosshair.activeInHierarchy);
            Debug.Log($"Crosshair active: {crosshair.activeInHierarchy}, Position: {crosshair.transform.position}");
        }
        else
        {
            Debug.Log("Crosshair prefab is null!");
        }
    }

    [ContextMenu("Test Marker Placement")]
    void TestMarkerPlacement()
    {
        if (flightMarkerPrefab != null)
        {
            Vector3 testPos = transform.position + Vector3.right * 3f;
            GameObject testMarker = Instantiate(flightMarkerPrefab, testPos, Quaternion.identity);
            Debug.Log($"Test marker created at: {testPos}");
        }
        else
        {
            Debug.Log("Flight marker prefab is null!");
        }
    }

    [ContextMenu("Log Flight Info")]
    public void LogFlightInfo()
    {
        Debug.Log($"Feathers: {currentFeathers}/{maxFeathers}");
        Debug.Log($"Crosshair Active: {crosshairActive}");
        Debug.Log($"Crosshair Position: {crosshairPosition}");
        Debug.Log($"Is Flying: {isFlying}");
    }
}
