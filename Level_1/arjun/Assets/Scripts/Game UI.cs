using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [Header("Feather UI")]
    public Image[] featherIcons = new Image[5];
    public Color availableFeatherColor = Color.white;
    public Color usedFeatherColor = Color.gray;

    [Header("Dash Chain UI")]
    public Transform dashChainBarRoot;
    public GameObject dashChainIconPrefab;

    [Header("Seed Counter UI")]
    public TextMeshProUGUI seedCountText;   // e.g. "x N"
    public Image seedIcon;                  // your seed icon
    
    [Header("Seed Counter Animation")]
    public float popScale = 1.25f;
    public float popDuration = 0.2f;
    
    [Header("Speed Boost UI")]
    public Image speedBoostIcon;
    public float speedBoostPulseSpeed = 3f;
    public float speedBoostPulseScale = 0.1f;

    private bool isSpeedBoostActive = false;
    private Vector3 originalSpeedIconScale;
    private Coroutine seedPopRoutine;
    private Vector3 seedCounterOriginalScale;

    void Start()
    {
        PlayerController.OnFeatherCountChanged += UpdateFeatherUI;
        PlayerController.OnChainDashLevelChanged += UpdateDashChainUI;
        PlayerController.OnSpeedBoostChanged += SetSpeedBoostActive;
        PlayerController.OnSeedCollected += UpdateSeedCounterUI;

        if (speedBoostIcon != null)
        {
            originalSpeedIconScale = speedBoostIcon.transform.localScale;
            speedBoostIcon.gameObject.SetActive(false);
        }

        // Store original scale of the seed counter (the parent panel of icon + text)
        if (seedIcon != null)
            seedCounterOriginalScale = seedIcon.transform.parent.localScale;
        else if (seedCountText != null)
            seedCounterOriginalScale = seedCountText.transform.parent.localScale;
        else
            seedCounterOriginalScale = Vector3.one;

        PlayerController player = FindFirstObjectByType<PlayerController>();
        if (player != null)
        {
            UpdateFeatherUI(player.GetCurrentFeathers());
            UpdateDashChainUI(player.GetCurrentChainDashLevel());
            UpdateSeedCounterUI(player.GetSeedsCollected());
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
        PlayerController.OnFeatherCountChanged -= UpdateFeatherUI;
        PlayerController.OnChainDashLevelChanged -= UpdateDashChainUI;
        PlayerController.OnSpeedBoostChanged -= SetSpeedBoostActive;
        PlayerController.OnSeedCollected -= UpdateSeedCounterUI;
    }

    // --- FEATHER UI ---
    public void UpdateFeatherUI(int currentFeathers)
    {
        for (int i = 0; i < featherIcons.Length; i++)
        {
            if (featherIcons[i] != null)
            {
                featherIcons[i].color = i < currentFeathers ? availableFeatherColor : usedFeatherColor;
            }
        }
    }

    // --- DASH CHAIN UI ---
    public void UpdateDashChainUI(int chainLevel)
    {
        foreach (Transform child in dashChainBarRoot)
        {
            Destroy(child.gameObject);
        }

        int iconsToShow = Mathf.Max(0, chainLevel - 1); // 0 for base level, 1 for first upgrade, etc.
        for (int i = 0; i < iconsToShow; i++)
        {
            if (dashChainIconPrefab != null)
            {
                Instantiate(dashChainIconPrefab, dashChainBarRoot);
            }
        }
    }

    // --- SEED COUNTER UI ---
    public void UpdateSeedCounterUI(int seedCount)
    {
        if (seedCountText != null)
            seedCountText.text = $"x {seedCount}";
        if (seedIcon != null)
            seedIcon.gameObject.SetActive(true);

        // Pop animation
        if (seedIcon != null)
        {
            RectTransform popTarget = seedIcon.transform.parent.GetComponent<RectTransform>();
            if (seedPopRoutine != null)
                StopCoroutine(seedPopRoutine);
            seedPopRoutine = StartCoroutine(SeedCounterPopEffect(popTarget));
        }
    }

    private System.Collections.IEnumerator SeedCounterPopEffect(RectTransform target)
    {
        if (target == null)
            yield break;

        Vector3 original = seedCounterOriginalScale;
        Vector3 popped = original * popScale;

        // Pop up
        float elapsed = 0f;
        while (elapsed < popDuration * 0.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (popDuration * 0.5f);
            target.localScale = Vector3.Lerp(original, popped, t);
            yield return null;
        }

        // Pop down
        elapsed = 0f;
        while (elapsed < popDuration * 0.5f)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / (popDuration * 0.5f);
            target.localScale = Vector3.Lerp(popped, original, t);
            yield return null;
        }
        target.localScale = original;
    }

    // --- SPEED BOOST UI ---
    public void SetSpeedBoostActive(bool active)
    {
        isSpeedBoostActive = active;
        if (speedBoostIcon != null)
        {
            speedBoostIcon.gameObject.SetActive(active);
            if (!active) speedBoostIcon.transform.localScale = originalSpeedIconScale;
        }
    }

    // --- PUBLIC TESTS ---
    [ContextMenu("Test Feather UI")]
    public void TestFeatherUI() { UpdateFeatherUI(3); }

    [ContextMenu("Test Dash Chain UI")]
    public void TestDashChainUI() { UpdateDashChainUI(4); }

    [ContextMenu("Test Seed Counter UI")]
    public void TestSeedCounterUI() { UpdateSeedCounterUI(7); }

    [ContextMenu("Toggle Speed Boost UI")]
    public void TestSpeedBoostUI() { SetSpeedBoostActive(!isSpeedBoostActive); }
}
