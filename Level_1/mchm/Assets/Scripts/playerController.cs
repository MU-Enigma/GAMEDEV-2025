using UnityEngine;
using System.Collections;

/// <summary>
/// ok this is the main player script. it handles moving and input and tells the ability script wtf is going on.
/// basically the brain of the operation.
/// </summary>
[RequireComponent(typeof(Rigidbody2D), typeof(PlayerAbilities))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float _moveSpeed = 25f;
    [SerializeField] private bool instantStop = true; // toggle this for that slidey ice physics feel if u want
    [SerializeField] private float speedBoostMultiplier = 3.0f;
    [SerializeField] private float speedBoostDuration = 1.5f;

    public float moveSpeed => _moveSpeed; // so other scripts can see the speed without changing it

    // Component References
    private Rigidbody2D _rb;
    private PlayerAbilities _abilities;
    private PlayerAnimationController _animationController; // this might be null who knows

    // State
    private Vector2 _moveInput;
    private Coroutine _speedBoostCoroutine; // gotta keep track of this thing
    private float _currentSpeedMultiplier = 1f;
    
    private void Awake()
    {
        // grab all the components we need so we're not calling GetComponent every frame like a damn caveman
        _rb = GetComponent<Rigidbody2D>();
        _abilities = GetComponent<PlayerAbilities>();
        _animationController = GetComponent<PlayerAnimationController>();

        // setting up the rigidbody for a top-down game. no gravity, no weird spinning.
        _rb.gravityScale = 0f;
        _rb.bodyType = RigidbodyType2D.Dynamic;
        _rb.linearDamping = 0f;
        _rb.angularDamping = 0f;

        if (_animationController == null)
            Debug.LogWarning("animator script is missing. animations are probably borked. not my problem.");
    }

    private void Update()
    {
        // --- Input Handling ---
        // just read the damn input. GetAxisRaw so its snappy.
        _moveInput.x = Input.GetAxisRaw("Horizontal");
        _moveInput.y = Input.GetAxisRaw("Vertical");
        _moveInput.Normalize(); // so you dont move faster diagonally. remember that bug? yeah. this fixes it.

        // shove the input over to the abilities script so it knows where to aim stuff
        _abilities.SetMoveInput(_moveInput);
    }

    private void FixedUpdate()
    {
        // physics stuff goes in here.
        // THE ABILITY SCRIPT GETS PRIORITY. if it's doing something fancy, this script shuts up and lets it.
        if (_abilities.IsAbilityActive()) return;
        // ABORT. let the other script cook.

        HandleMovement();
    }

    /// <summary>
    /// this just moves the player based on the input from Update()
    /// </summary>
    private void HandleMovement()
    {
        float effectiveMoveSpeed = moveSpeed * _currentSpeedMultiplier;

        if (_moveInput.magnitude > 0.1f)
            _rb.linearVelocity = _moveInput * effectiveMoveSpeed;

        // velocity is better than AddForce for this kind of movement. fight me.

        else
        {
            // stop moving when there's no input. groundbreaking i know.
            if (instantStop) _rb.linearVelocity = Vector2.zero;

            else
            {
                // the gradual stop. kinda feels like you're on ice.
                float stopSpeed = effectiveMoveSpeed * 4f;
                _rb.linearVelocity = Vector2.MoveTowards(_rb.linearVelocity, Vector2.zero, stopSpeed * Time.fixedDeltaTime);
            }
        }
    }

    /// <summary>
    /// turns on the zoomies.
    /// </summary>
    public void ApplySpeedBoost()
    {
        // if we're already boosting, stop that one and start a new one. resets the timer.
        if (_speedBoostCoroutine != null)
            StopCoroutine(_speedBoostCoroutine);

        _speedBoostCoroutine = StartCoroutine(SpeedBoostCoroutine());
    }

    /// <summary>
    /// the coroutine that handles the speed boost timer. because of course it's a coroutine.
    /// </summary>
    private IEnumerator SpeedBoostCoroutine()
    {
        _currentSpeedMultiplier = speedBoostMultiplier;
        _animationController?.SetIsBlue(true); // make him blue. sonic fast.

        yield return new WaitForSeconds(speedBoostDuration); // wait for a bit

        _currentSpeedMultiplier = 1f; // back to normal
        _animationController?.SetIsBlue(false); // not blue anymore :(
        _speedBoostCoroutine = null; // IMPORTANT: null this out so we know its done.
    }

    /// <summary>
    /// just checks if the boost is active.
    /// </summary>
    public bool IsSpeedBoosted() => _speedBoostCoroutine != null;
}

