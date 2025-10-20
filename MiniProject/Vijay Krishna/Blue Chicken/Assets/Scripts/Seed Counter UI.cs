using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SeedCounterUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Text component to display seed count.")]
    public TextMeshProUGUI seedCountText;
    [Tooltip("Alternative UI Text component if not using TextMeshPro.")]
    public Text legacySeedCountText;
    [Tooltip("Image component to display seed sprite icon.")]
    public Image seedIcon;
    
    [Header("Display Settings")]
    [Tooltip("Format string for displaying seed count (use {0} for the number).")]
    public string displayFormat = "x {0}";
    [Tooltip("The seed sprite to display as an icon.")]
    public Sprite seedSprite;
    [Tooltip("Hide the icon when count is 0.")]
    public bool hideIconWhenZero = false;
    
    [Header("Animation (Optional)")]
    [Tooltip("Animate the counter when seeds are collected.")]
    public bool animateOnCollect = true;
    [Tooltip("Scale multiplier for collection animation.")]
    public float animationScale = 1.2f;
    [Tooltip("Duration of the collection animation.")]
    public float animationDuration = 0.3f;
    
    // NEW: Variable to hold the original scale of the ICON, not the parent transform
    private Vector3 originalIconScale;
    
    void Start()
    {
        // Subscribe to seed collection events
        PlayerController.OnSeedCollected += UpdateSeedDisplay;
        
        // Set up seed icon if provided
        if (seedIcon != null && seedSprite != null)
        {
            seedIcon.sprite = seedSprite;
            // NEW: Store the original scale of the ICON to animate it later
            originalIconScale = seedIcon.transform.localScale;
        }
        
        // Initialize display
        UpdateSeedDisplay(0);
    }
    
    void OnDestroy()
    {
        // Unsubscribe to prevent errors
        PlayerController.OnSeedCollected -= UpdateSeedDisplay;
    }
    
    /// <summary>
    /// Update the seed count display
    /// </summary>
    void UpdateSeedDisplay(int seedCount)
    {
        // Update text
        string displayText = string.Format(displayFormat, seedCount);
        
        if (seedCountText != null)
        {
            seedCountText.text = displayText;
        }
        else if (legacySeedCountText != null)
        {
            legacySeedCountText.text = displayText;
        }
        
        // Handle icon visibility
        if (seedIcon != null)
        {
            if (hideIconWhenZero && seedCount == 0)
            {
                seedIcon.gameObject.SetActive(false);
            }
            else
            {
                seedIcon.gameObject.SetActive(true);
            }
        }
        
        // Play collection animation
        // We only animate if the icon is present, since the animation now targets the icon.
        if (animateOnCollect && seedCount > 0 && seedIcon != null)
        {
            // Stop any previous animation coroutine to prevent overlap
            StopCoroutine(PlayCollectionAnimation()); 
            StartCoroutine(PlayCollectionAnimation());
        }
    }
    
    /// <summary>
    /// Play a small scale animation when seeds are collected
    /// </summary>
    System.Collections.IEnumerator PlayCollectionAnimation()
    {
        // TARGET THE ICON'S TRANSFORM, NOT THE PARENT TRANSFORM
        Transform targetTransform = seedIcon.transform;
        Vector3 originalScale = originalIconScale;
        Vector3 targetScale = originalScale * animationScale;
        
        // Scale up
        float elapsed = 0f;
        while (elapsed < animationDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (animationDuration * 0.5f);
            targetTransform.localScale = Vector3.Lerp(originalScale, targetScale, progress);
            yield return null;
        }
        
        // Scale back down
        elapsed = 0f;
        while (elapsed < animationDuration * 0.5f)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / (animationDuration * 0.5f);
            targetTransform.localScale = Vector3.Lerp(targetScale, originalScale, progress);
            yield return null;
        }
        
        // Ensure we end at original scale
        targetTransform.localScale = originalScale;
    }
    
    /// <summary>
    /// Manually set the seed sprite (useful if sprite changes based on upgrades)
    /// </summary>
    public void SetSeedSprite(Sprite newSprite)
    {
        seedSprite = newSprite;
        if (seedIcon != null)
        {
            seedIcon.sprite = newSprite;
        }
    }
}
