using UnityEngine;
using UnityEngine.UI;

public class GameUI : MonoBehaviour
{
    [Header("Feather UI")]
    [Tooltip("Array of 5 feather icon images (assign in order left to right).")]
    public Image[] featherIcons = new Image[5];
    [Tooltip("Color for available feathers.")]
    public Color availableFeatherColor = Color.white;
    [Tooltip("Color for used/unavailable feathers.")]
    public Color usedFeatherColor = Color.gray;

    [Header("Dash Chain UI")]
    [Tooltip("Parent panel for dash chain icons (should have Horizontal Layout Group).")]
    public Transform dashChainBarRoot;
    [Tooltip("Prefab for each dash chain link icon.")]
    public GameObject dashChainIconPrefab;

    [Header("Speed Boost UI")]
    [Tooltip("Speed boost indicator icon (shows when seed boost is active).")]
    public Image speedBoostIcon;
    [Tooltip("Optional: Pulsing animation speed for speed boost icon.")]
    public float speedBoostPulseSpeed = 3f;
    [Tooltip("Optional: Pulse scale range for speed boost icon.")]
    public float speedBoostPulseScale = 0.1f;

    private bool isSpeedBoostActive = false;
    private Vector3 originalSpeedIconScale;

    void Start()
    {
        // Subscribe to player events
        PlayerController.OnFeatherCountChanged += UpdateFeatherUI;
        PlayerController.OnChainDashLevelChanged += UpdateDashChainUI;
        PlayerController.OnSpeedBoostChanged += SetSpeedBoostActive;
        
        // Initialize speed boost icon
        if (speedBoostIcon != null)
        {
            originalSpeedIconScale = speedBoostIcon.transform.localScale;
            speedBoostIcon.gameObject.SetActive(false);
        }

        // Initialize UI with current player state
        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            UpdateFeatherUI(player.GetCurrentFeathers());
            UpdateDashChainUI(player.GetCurrentChainDashLevel());
        }
    }

    void Update()
    {
        // Animate speed boost icon with pulsing effect
        if (isSpeedBoostActive && speedBoostIcon != null)
        {
            float pulse = 1f + Mathf.Sin(Time.time * speedBoostPulseSpeed) * speedBoostPulseScale;
            speedBoostIcon.transform.localScale = originalSpeedIconScale * pulse;
        }
    }

    void OnDestroy()
    {
        // Unsubscribe to prevent errors
        PlayerController.OnFeatherCountChanged -= UpdateFeatherUI;
        PlayerController.OnChainDashLevelChanged -= UpdateDashChainUI;
        PlayerController.OnSpeedBoostChanged -= SetSpeedBoostActive;
    }

    // --- FEATHER UI ---
    public void UpdateFeatherUI(int currentFeathers)
    {
        for (int i = 0; i < featherIcons.Length; i++)
        {
            if (featherIcons[i] != null)
            {
                // Color available feathers normally, used feathers as gray
                if (i < currentFeathers)
                    featherIcons[i].color = availableFeatherColor;
                else
                    featherIcons[i].color = usedFeatherColor;
            }
        }
    }

    // --- DASH CHAIN UI ---
    public void UpdateDashChainUI(int chainLevel)
    {
        // Clear existing chain icons
        foreach (Transform child in dashChainBarRoot)
        {
            Destroy(child.gameObject);
        }

        // Show 1 coin per chain dash possible (1 coin for 1 dash, 2 coins for 2 dashes, etc.)
        int iconsToShow = Mathf.Max(0, chainLevel - 1);
        
        for (int i = 0; i < iconsToShow; i++)
        {
            if (dashChainIconPrefab != null)
            {
                Instantiate(dashChainIconPrefab, dashChainBarRoot);
            }
        }
    }

    // --- SPEED BOOST UI ---
    public void SetSpeedBoostActive(bool active)
    {
        isSpeedBoostActive = active;
        
        if (speedBoostIcon != null)
        {
            speedBoostIcon.gameObject.SetActive(active);
            if (!active)
            {
                // Reset scale when deactivating
                speedBoostIcon.transform.localScale = originalSpeedIconScale;
            }
        }
    }

    // --- PUBLIC METHODS FOR MANUAL UPDATES ---
    [ContextMenu("Test Feather UI")]
    public void TestFeatherUI()
    {
        UpdateFeatherUI(3); // Show 3 available, 2 used
    }

    [ContextMenu("Test Dash Chain UI")]
    public void TestDashChainUI()
    {
        UpdateDashChainUI(4); // Show 4 chain link coins
    }

    [ContextMenu("Toggle Speed Boost UI")]
    public void TestSpeedBoostUI()
    {
        SetSpeedBoostActive(!isSpeedBoostActive);
    }
}
