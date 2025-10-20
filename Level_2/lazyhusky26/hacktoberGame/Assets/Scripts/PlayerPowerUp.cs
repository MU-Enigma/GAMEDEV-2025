using UnityEngine;

public class PlayerPowerUp : MonoBehaviour
{
    [Header("Player Sprites")]
    public Sprite normalSprite;
    public Sprite poweredUpSprite;

    [Header("Power-Up Settings")]
    public float powerUpDuration = 13f;

    [Header("Weapons")]
    public GameObject swordWeapon;

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

        if (spriteRenderer != null && normalSprite != null)
            spriteRenderer.sprite = normalSprite;

        if (swordWeapon != null) swordWeapon.SetActive(true);

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

        // Stop power-up sound
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.clip = null;
        }
    }
}
