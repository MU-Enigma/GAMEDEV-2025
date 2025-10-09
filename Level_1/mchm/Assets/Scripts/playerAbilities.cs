using UnityEngine;
using System.Collections;

// just an enum for the dropdown in the inspector.
public enum AbilityType
{
    ChargeDash,     // Index 0
    TeleportMarker, // Index 1
    ShadowEcho,     // Index 2
    Blink           // Index 3
}

/// <summary>
/// ok this is where the magic and nightmares happen. i had to merge like 4 different ability scripts into this one
/// and they were all written by different people. it's a miracle this thing even compiles.
/// this script is basically a state machine held together with duct tape and caffeine.
/// god and i are the only ones who knew how it works... and i forgot.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(PlayerStamina), typeof(PlayerController))]
public class PlayerAbilities : MonoBehaviour
{
    [Header("Ability Management")]
    [SerializeField] private AbilityType currentAbility = AbilityType.ChargeDash;
    // Public property so other scripts (like the UI) can SEE what ability is active, but can't CHANGE it directly.
    public AbilityType CurrentAbility => currentAbility;


    // --- Component References ---
    private Rigidbody2D _rb;
    private PlayerStamina _stamina;
    private PlayerController _controller;
    private PlayerAnimationController _animationController;
    private SpriteRenderer _spriteRenderer;
    private CameraFollow _followScript;

    // --- Ability State ---
    private Vector2 _moveInput; // PlayerController shoves the input here
    private bool _isAbilityActive = false; // THE BIG ONE. if this is true, PlayerController stops moving.
    private bool _isCharging = false;
    private float _currentChargeTime = 0f;

    // --- 1. Charge Dash ---
    [Header("1. Charge Dash Settings")]
    [SerializeField] private bool chargeDashEnabled = true;
    [SerializeField] private float maxChargeTime = 1.75f;
    [SerializeField] private float baseDashSpeed = 15f;
    [SerializeField] private float maxDashSpeedMultiplier = 3f;
    [SerializeField] private float baseDashDuration = 0.2f;
    [SerializeField] private float maxDurationIncrease = 0.2f;
    [SerializeField] private float minStaminaCost = 25f;
    [SerializeField] private float maxStaminaCost = 75f;
    [SerializeField, Range(0f, 1f)] private float chargeMoveSpeedMultiplier = 0.5f;

    // --- 2. Teleport Marker ---
    [Header("2. Jump Marker Settings")]
    // sure its called jump marker, but i am NOT updating all the internal variables
    // deal with it 
    [SerializeField] private bool teleportMarkerEnabled = true;
    [SerializeField] private GameObject teleportMarkerPrefab;
    [SerializeField] private float placeMarkerStaminaCost = 15f;
    [SerializeField] private float minTeleportStaminaCost = 20f;
    [SerializeField] private float maxTeleportDistance = 100f;
    [SerializeField] private float jumpHeight = 20f; // how high the "jump" arc goes
    [SerializeField] private float jumpDuration = 0.5f; // how long the jump takes
    [SerializeField] private float cameraSpeedReductionFactor = 3f;
    private GameObject _currentMarker;

    // --- 3. Shadow Echo ---
    [Header("3. Shadow Echo Settings")]
    [SerializeField] private bool shadowEchoEnabled = true;
    [SerializeField] private GameObject echoPrefab;
    [SerializeField] private float spawnEchoStaminaCost = 20f;
    [SerializeField] private float swapWithEchoStaminaCost = 10f;
    [SerializeField] private Color echoColor = new Color(0.2f, 0.2f, 0.2f, 0.6f);
    private GameObject _activeEcho;

    // --- 4. Blink ---
    [Header("4. Blink Settings")]
    [SerializeField] private bool blinkEnabled = true;
    [SerializeField] private float blinkDistance = 25f;
    [SerializeField] private float blinkStaminaCost = 35f;

    // --- Visuals ---
    [Header("Visuals")]
    [SerializeField] private GameObject afterimagePrefab;
    // TODO: make this use an object pool, instantiating is slow as hell but idc rn
    [SerializeField] private float afterimageInterval = 0.01f;
    [SerializeField] private float afterimageDuration = 0.5f;

    private void Awake()
    {
        // grab all the things
        _rb = GetComponent<Rigidbody2D>();
        _stamina = GetComponent<PlayerStamina>();
        _controller = GetComponent<PlayerController>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animationController = GetComponent<PlayerAnimationController>();
        _followScript = Camera.main.GetComponent<CameraFollow>();
    }

    private void Update()
    {
        if (_isAbilityActive) return; // if we're in the middle of a dash or something, dont read new input

        // this switch statement is the glorious mess that holds it all together.
        // it just checks which ability is selected and runs ITS input logic. that's it.
        switch (currentAbility)
        {
            case AbilityType.ChargeDash when chargeDashEnabled:
                HandleChargeDashInput();
                break;
            case AbilityType.TeleportMarker when teleportMarkerEnabled:
                HandleTeleportMarkerInput();
                break;
            case AbilityType.ShadowEcho when shadowEchoEnabled:
                HandleShadowEchoInput();
                break;
            case AbilityType.Blink when blinkEnabled:
                HandleBlinkInput();
                break;
        }
    }

    private void FixedUpdate()
    {
        // if you're charging the dash, you move slower. has to be in fixedupdate cuz physics.
        if (_isCharging)
        {
            float chargeMoveSpeed = _controller.moveSpeed * chargeMoveSpeedMultiplier;
            _rb.linearVelocity = _moveInput * chargeMoveSpeed;
        }
    }

    /// <summary>
    /// This is the new public-facing method that the UI script will call.
    /// It's the designated entry point for changing abilities from outside this script.
    /// Keep your grubby hands off the 'currentAbility' variable directly, use this instead.
    /// </summary>
    public void SetAbility(AbilityType newAbility)
    {
        // TODO: maybe add a sound effect or something here when you switch. for now, it just works.
        currentAbility = newAbility;
        Debug.Log("Switched to ability: " + newAbility);
    }
    
    public void SetMoveInput(Vector2 input)
    {
        _moveInput = input;
    }
    
    /// <summary>
    /// THE MOST IMPORTANT FUNCTION. this is the kill switch i made so the controller script
    /// doesnt try to move the player while they're mid-dash. stops them from fighting.
    /// basically, if this is true, PlayerController doesn't in fact control the player LLLLL
    /// </summary>
    public bool IsAbilityActive() => _isAbilityActive || _isCharging;

    // --- simple input wrappers so i dont have to type this shit out a million times ---
    private bool GetAbilityButtonDown() => Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.JoystickButton3);
    private bool GetAbilityButton() => Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.JoystickButton3);
    private bool GetAbilityButtonUp() => Input.GetKeyUp(KeyCode.Space) || Input.GetKeyUp(KeyCode.JoystickButton3);
    private bool GetPlaceMarkerButtonDown() => Input.GetMouseButtonDown(0);

    #region Charge Dash Logic
    // this one was pretty straightforward, thank god. just hold to charge, release to go.
    // i mean... this is my own code so if i got lost idk what i'm doing
    private void HandleChargeDashInput()
    {
        // start charging. check !_isCharging so this doesnt fire every frame
        if (GetAbilityButtonDown() && !_isCharging)
        {
            _isCharging = true;
            _currentChargeTime = 0f;
            _animationController?.SetChargingState(true); // <<< HEY DUMBASS, TELL THE ANIMATOR WE'RE CHARGING
        }

        // keep charging as long as the button is held
        if (GetAbilityButton() && _isCharging)
        {
            _currentChargeTime += Time.deltaTime;

            // also, while we're at it, lets tell the animator HOW MUCH we've charged
            float chargePercent = Mathf.Clamp01(_currentChargeTime / maxChargeTime);
            _animationController?.SetChargePercentage(chargePercent);
            _animationController?.SetFullyChargedState(chargePercent >= 1f);
        }

        // UNLEASH THE BEAST
        if (GetAbilityButtonUp() && _isCharging)
        {
            _isCharging = false;
            _animationController?.SetChargingState(false); // <<< AND TELL IT TO STOP
            _animationController?.SetChargePercentage(0f); // reset this shit too for good measure
            _animationController?.SetFullyChargedState(false);
            ExecuteChargedDash();
        }
    }

    private void ExecuteChargedDash()
    {
        // more charge = more speed, more duration, more cost. simple.
        float chargePercent = Mathf.Clamp01(_currentChargeTime / maxChargeTime);
        float cost = Mathf.Lerp(minStaminaCost, maxStaminaCost, chargePercent);

        if (_stamina.TryUseStamina(cost))
        {
            // if you're not moving, dash forward. otherwise dash in the direction you're moving.
            Vector2 dashDir = _moveInput.magnitude > 0.1f ? _moveInput : (Vector2)transform.up;
            float speed = baseDashSpeed * Mathf.Lerp(1f, maxDashSpeedMultiplier, chargePercent);
            float duration = baseDashDuration + (maxDurationIncrease * chargePercent);
            StartCoroutine(DashCoroutine(dashDir, speed, duration)); // go go go
        }
    }

    private IEnumerator DashCoroutine(Vector2 direction, float speed, float duration)
    {
        _isAbilityActive = true; // TAKE CONTROL
        if (afterimagePrefab) StartCoroutine(CreateAfterimages());

        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            _rb.linearVelocity = direction * speed;
            yield return null; // wait a frame
        }
        
        _isAbilityActive = false; // ok im done
    }
    #endregion

    #region Teleport Marker Logic
    // OKAY. THIS ISN'T A TELEPORT ANYMORE. IT'S A FUCKING JUMP.
    // AM I CHANGING INTERNAL VARIABLES?? FUCK NO
    private void HandleTeleportMarkerInput()
    {
        if (GetPlaceMarkerButtonDown()) PlaceMarker();
        if (GetAbilityButtonDown() && _currentMarker != null) JumpToMarker(); // Renamed for sanity
    }

    private void PlaceMarker()
    {
        if (!_stamina.TryUseStamina(placeMarkerStaminaCost)) return;
        if (_currentMarker != null) Destroy(_currentMarker); // get rid of the old one
        
        Vector2 markerPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition); // Assuming mouse input for placing
        _currentMarker = Instantiate(teleportMarkerPrefab, markerPosition, Quaternion.identity);
    }

    private void JumpToMarker()
    {
        float distance = Vector2.Distance(transform.position, _currentMarker.transform.position);
        float percent = Mathf.Clamp01(distance / maxTeleportDistance);
        float cost = Mathf.Lerp(minTeleportStaminaCost, _stamina.maxStamina, percent);

        if (_stamina.TryUseStamina(cost))
        {
            // DON'T just teleport. Start the jump coroutine instead.
            StartCoroutine(JumpToMarkerCoroutine(_currentMarker.transform.position));
            Destroy(_currentMarker); // clean up the marker object
            _currentMarker = null;
        }
    }

    // THE NEW HOTNESS. A COROUTINE THAT FAKES A JUMP ARC.
    private IEnumerator JumpToMarkerCoroutine(Vector3 targetPosition)
    {
        _followScript?.setTargetTemporary(targetPosition, 1/cameraSpeedReductionFactor);
        // slow down the camera while jumping
        _isAbilityActive = true; // PlayerController, SIT DOWN.
        _rb.simulated = false; // Turn off physics so we can have full control.

        Vector3 startPosition = transform.position;
        float timer = 0f;

        while (timer < jumpDuration)
        {
            timer += Time.deltaTime;
            float t = timer / jumpDuration;
            // This is some fancy math bullshit. Lerp moves it horizontally, the Sine wave makes it go up and down in an arc.
            // Trust the math. Do not question the math.
            float yOffset = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            transform.position = Vector3.Lerp(startPosition, targetPosition, t) + new Vector3(0, yOffset, 0);

            yield return null; // wait a frame
        }

        transform.position = targetPosition; // just to be sure we land perfectly.
        _rb.simulated = true; // Turn physics back on.
        _isAbilityActive = false; // OK, PlayerController, you can stand up now.    
    }
    #endregion

    #region Shadow Echo Logic
    // press once to drop a clone, press again to swap places. also simple. two for two.
    private void HandleShadowEchoInput()
    {
        if (GetAbilityButtonDown())
        {
            if (_activeEcho == null) SpawnEcho();
            else SwapWithEcho();
        }
    }

    private void SpawnEcho()
    {
        if (!_stamina.TryUseStamina(spawnEchoStaminaCost)) return;
        if (_activeEcho != null) Destroy(_activeEcho);

        _activeEcho = Instantiate(echoPrefab, transform.position, transform.rotation);
        // make the echo look like a spooky ghost version of the player
        if (_activeEcho.TryGetComponent<SpriteRenderer>(out var echoSr))
        {
            echoSr.sprite = _spriteRenderer.sprite;
            echoSr.color = echoColor;
        }
    }
    
    private void SwapWithEcho()
    {
        if (_stamina.TryUseStamina(swapWithEchoStaminaCost))
        {
            transform.position = _activeEcho.transform.position;
            Destroy(_activeEcho); // goodbye clone
            _activeEcho = null;
        }
    }
    #endregion

    #region Blink Logic
    // OH MY GOD THE BLINK. this one was the WORST. took me forever to get it to not feel like garbage.
    // the problem is if you just change the transform.position, the rigidbody's velocity from the PREVIOUS
    // frame is still there, so the player would jitter or slide after blinking. it was a nightmare.
    private void HandleBlinkInput()
    {
        if (GetAbilityButtonDown()) ExecuteBlink();
    }

    private void ExecuteBlink()
    {
        if (_moveInput.magnitude < 0.1f) return; // dont blink if you're standing still, dumbass

        if (_stamina.TryUseStamina(blinkStaminaCost))
        {
            transform.position += (Vector3)_moveInput * blinkDistance; // the actual teleport. easy part.

            // OKAY LISTEN UP. THIS IS THE VOODOO.
            // this tiny coroutine sets _isAbilityActive to true for ONE. SINGLE. FRAME.
            // this tells the PlayerController to skip its movement update for a frame, which STOPS
            // it from applying the old velocity after we teleport.
            // DO NOT TOUCH THIS. it took 3 cans of monster to figure this out.
            StartCoroutine(ResetAbilityFlagCoroutine());
        }
    }
    
    private IEnumerator ResetAbilityFlagCoroutine()
    {
        _isAbilityActive = true;
        yield return new WaitForEndOfFrame(); // wait until the literal end of the frame
        _isAbilityActive = false;
    }
    #endregion

    #region Afterimage Visuals
    // this whole section is just for the dash's visual trail. probably inefficient as hell.
    private IEnumerator CreateAfterimages()
    {
        while (_isAbilityActive)
        {
            // spawn a ghost and wait a tiny bit
            CreateAfterimageInstance();
            yield return new WaitForSeconds(afterimageInterval);
        }
    }

    private void CreateAfterimageInstance()
    {
        GameObject afterimage = Instantiate(afterimagePrefab, transform.position, transform.rotation);
        if (afterimage.TryGetComponent<SpriteRenderer>(out var afterimageRenderer))
            afterimageRenderer.sprite = _spriteRenderer.sprite;
            
        StartCoroutine(FadeAfterimageCoroutine(afterimage.GetComponent<SpriteRenderer>())); // tell it to fade away and die
    }

    private IEnumerator FadeAfterimageCoroutine(SpriteRenderer renderer)
    {
        float timer = 0f;
        Color startColor = renderer.color;
        
        while (timer < afterimageDuration)
        {
            // just fade out the alpha channel
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, timer / afterimageDuration);
            renderer.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        Destroy(renderer.gameObject); // ok you're faded, now get out of my hierarchy
    }
    #endregion
}

