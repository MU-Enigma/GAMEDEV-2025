using UnityEngine;

// This component is required for the fading effect.
[RequireComponent(typeof(SpriteRenderer))]
public class AfterImage : MonoBehaviour
{
    // --- Component References ---
    private SpriteRenderer sr;

    // --- Fading Variables ---
    [Header("Fading Effect")]
    [Tooltip("The initial transparency of the after-image when it appears.")]
    [Range(0, 1)]
    public float initialAlpha = 0.8f;
    [Tooltip("How quickly the after-image fades away.")]
    public float fadeSpeed = 2f;

    // FIX: Added public variables to control the sorting layer from the Inspector.
    [Header("Sorting")]
    [Tooltip("The name of the sorting layer for the after-image.")]
    public string sortingLayerName = "Default";
    [Tooltip("The order in that layer. Higher numbers are drawn on top.")]
    public int orderInLayer = 1;

    private Color currentColor;

    // This method is called once when the script instance is being loaded.
    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    // This method is called every frame.
    void Update()
    {
        // Continuously fade the sprite's alpha value over time.
        float newAlpha = currentColor.a - (fadeSpeed * Time.deltaTime);
        currentColor.a = Mathf.Clamp01(newAlpha); // Ensure alpha stays between 0 and 1.
        sr.color = currentColor;

        // When the image becomes fully transparent, deactivate it to return it to the pool.
        if (currentColor.a <= 0)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Called from the PlayerController to begin the fade-out process.
    /// It resets the sprite's appearance for its new position and rotation.
    /// </summary>
    public void StartFade()
    {
        // FIX: Set the sorting layer and order to ensure it draws on top of other sprites.
        sr.sortingLayerName = sortingLayerName;

        // FIX: More robustly set the initial color and alpha for the fade effect.
        // This prevents issues if the prefab's color was saved with an alpha of 0.
        currentColor = new Color(1f, 1f, 1f, initialAlpha);
        sr.color = currentColor;
    }
}
