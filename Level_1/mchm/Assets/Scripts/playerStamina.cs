using UnityEngine;

/// <summary>
/// the fun police. manages stamina so you cant just spam everything.
/// </summary>
public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina Settings")]
    [Tooltip("The maximum amount of stamina the player can have.")]
    [SerializeField] private float _maxStamina = 100f;
    [Tooltip("The rate at which stamina regenerates per second.")]
    [SerializeField] private float regenRate = 15f;
    [Tooltip("The delay in seconds before stamina begins to regenerate after being used.")]
    [SerializeField] private float regenDelay = 1.5f;
    
    public float maxStamina => _maxStamina;
    public float currentStamina { get; private set; } // other scripts can see it but only this one can change it.

    private float _regenTimer;

    private void Start() => currentStamina = _maxStamina;
    // start with full stamina duh
    // yes i am using a lambda expression sue me

    private void Update() => HandleRegeneration();
    
    /// <summary>
    /// this is what other scripts call to use stamina. returns true if they can afford it.
    /// </summary>
    public bool TryUseStamina(float cost)
    {
        if (currentStamina < cost) return false; // lol nope. broke ass.

        currentStamina -= cost;
        _regenTimer = regenDelay; // every time you use stamina, you have to wait to regen again.
        return true;
    }

    /// <summary>
    /// handles getting stamina back over time.
    /// </summary>
    private void HandleRegeneration()
    {
        // if the timer is running, just count it down. no regen for you.
        if (_regenTimer > 0f)
        {
            _regenTimer -= Time.deltaTime;
            return;
        }

        // ok timer is done, if you're not full, start regening
        if (currentStamina < _maxStamina)
        {
            currentStamina += regenRate * Time.deltaTime;
            currentStamina = Mathf.Clamp(currentStamina, 0, _maxStamina); // dont overheal
        }
    }

    /// <summary>
    /// for the UI guys. gives them a 0-1 value for the stamina bar.
    /// </summary>
    public float GetStaminaPercentage() => _maxStamina > 0 ? currentStamina / _maxStamina : 0f;
    // avoid dividing by zero because i dont trust anyone to set maxStamina correctly.
}

