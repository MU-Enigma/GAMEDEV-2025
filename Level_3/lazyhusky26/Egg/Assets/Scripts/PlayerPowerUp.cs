using UnityEngine;

public class PlayerPowerUp : MonoBehaviour
{
    [Header("Player Sprites")]
    public Sprite normalSprite;
    public Sprite poweredUpSprite;

    [Header("Power-Up Settings")]
    public float powerUpDuration = 5f;

    [Header("Weapons")]
    public GameObject swordWeapon;
    public GameObject gunWeapon; // ✅ Add this in the Inspector

    [Header("Audio")]
    public AudioClip powerUpSound;

    private AudioSource audioSource;
    private SpriteRenderer spriteRenderer;
    private bool isPoweredUp = false;
    private float powerUpTimer = 0f;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();

        // Set default sprite
        if (spriteRenderer != null && normalSprite != null)
            spriteRenderer.sprite = normalSprite;

        // Start with sword enabled, gun disabled
        if (swordWeapon != null) swordWeapon.SetActive(true);
        if (gunWeapon != null) gunWeapon.SetActive(false);

        if (audioSource != null)
        {
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.spatialBlend = 0f;
        }
    }

    void Update()
    {
        if (!isPoweredUp) return;

        powerUpTimer -= Time.deltaTime;

        if (powerUpTimer <= 0f)
            DeactivatePowerUp();
    }

    public void ActivatePowerUp()
    {
        if (isPoweredUp) return;

        isPoweredUp = true;
        powerUpTimer = powerUpDuration;

        // Change sprite to powered-up version
        if (spriteRenderer != null && poweredUpSprite != null)
            spriteRenderer.sprite = poweredUpSprite;

        // Switch to gun
        if (swordWeapon != null) swordWeapon.SetActive(false);
        if (gunWeapon != null) gunWeapon.SetActive(true);

        // Play power-up sound
        if (audioSource != null && powerUpSound != null)
        {
            audioSource.clip = powerUpSound;
            audioSource.Play();
        }
    }

    public void DeactivatePowerUp()
    {
        if (!isPoweredUp) return;

        isPoweredUp = false;

        // Revert sprite to normal
        if (spriteRenderer != null && normalSprite != null)
            spriteRenderer.sprite = normalSprite;

        // Switch back to sword
        if (swordWeapon != null) swordWeapon.SetActive(true);
        if (gunWeapon != null) gunWeapon.SetActive(false);

        // Stop power-up sound
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }
}
