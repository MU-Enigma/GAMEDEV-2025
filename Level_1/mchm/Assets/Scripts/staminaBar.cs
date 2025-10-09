using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// This script just makes the stamina bar... a bar. It reads from PlayerStamina and updates the UI.
/// Also has some fancy color changing and fade-out logic because we're not animals.
/// </summary>
[RequireComponent(typeof(CanvasGroup))] // I'm adding this so the fade logic never breaks.
public class StaminaBar : MonoBehaviour
{
    [Header("UI References")]
    public Slider staminaSlider; // drag the slider here
    public Image fillImage; // drag the Fill image of the slider here
    
    [Header("Colors")]
    public Color fullStaminaColor = Color.green;
    public Color lowStaminaColor = Color.red;
    
    [Header("Settings")]
    public float lowStaminaThreshold = 0.3f; // At what % the bar starts turning red
    public bool hideWhenFull = true; // should it fade out when stamina is full?
    public float fadeSpeed = 3f;

    private PlayerStamina _staminaRef;
    private CanvasGroup _canvasGroup;
    
    void Start()
    {
        _canvasGroup = GetComponent<CanvasGroup>();

        // THE FIX IS HERE. LET'S CHECK FOR THE SLIDER.
        // First, see if you even dragged it in from the inspector.
        if (staminaSlider == null) staminaSlider = GetComponentInChildren<Slider>();
            // If not, I'll try to find it on a child object as a last resort.

        // NOW, if it's STILL null after all that...
        if (staminaSlider == null)
        {
            Debug.LogError("FATAL ERROR IN STAMINA BAR: The 'staminaSlider' reference is missing! Did you forget to drag the Slider UI element into the inspector slot? The StaminaBar is now self-destructing to prevent further errors. Godspeed.", gameObject);
            gameObject.SetActive(false); // SHUT IT DOWN.
            return; // ABORT.
        }

        // while we're at it, let's find the fill image if it's missing too
        if (fillImage == null && staminaSlider.fillRect != null)
            fillImage = staminaSlider.fillRect.GetComponent<Image>();

        
        // Try to find the player's stamina component in the scene.
        _staminaRef = FindFirstObjectByType<PlayerStamina>();
        if (_staminaRef == null)
        {
            Debug.LogError("STAMINA BAR CAN'T FIND THE PLAYER STAMINA SCRIPT! HIDE THE EVIDENCE!", gameObject);
            gameObject.SetActive(false); // just disable myself to prevent a million errors
        }
    }
    
    void Update()
    {
        // if we couldn't find the player, we would have already disabled ourselves in Start, but just in case...
        if (_staminaRef == null) return;
        
        UpdateStaminaVisuals();
        HandleVisibility();
    }
    
    private void UpdateStaminaVisuals()
    {
        float staminaPercent = _staminaRef.GetStaminaPercentage();
        
        // update the slider's value. this is the line that was breaking.
        staminaSlider.value = staminaPercent;
        
        // here's the color logic. if we're below the threshold, start blending from green to red.
        if (fillImage != null)
        {
            if (staminaPercent <= lowStaminaThreshold)
            {
                // this basically calculates how "deep" into the red zone we are
                float colorLerpFactor = staminaPercent / lowStaminaThreshold;
                fillImage.color = Color.Lerp(lowStaminaColor, fullStaminaColor, colorLerpFactor);
            }
            else fillImage.color = fullStaminaColor;
        }
    }
    
    private void HandleVisibility()
    {
        // if the option is turned off, just stay visible and do nothing.
        if (!hideWhenFull)
        {
            _canvasGroup.alpha = 1;
            return;
        }
        
        // if stamina is full, fade out. if not, fade in.
        float targetAlpha = (_staminaRef.currentStamina >= _staminaRef.maxStamina) ? 0f : 1f;
        
        // lerp the alpha so it fades smoothly instead of just popping in and out of existence.
        _canvasGroup.alpha = Mathf.Lerp(_canvasGroup.alpha, targetAlpha, fadeSpeed * Time.deltaTime);
    }
}

