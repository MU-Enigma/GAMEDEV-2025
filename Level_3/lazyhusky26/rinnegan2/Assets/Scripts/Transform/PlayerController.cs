using UnityEngine;

public class PlayerTransform : MonoBehaviour
{
    [Header("Sprites")]
    public Sprite normalSprite;
    public Sprite transformedSprite;

    [Header("VFX & SFX")]
    public GameObject transformVFX;
    public AudioClip transformSFX;
    public float sfxVolume = 1f;

    private SpriteRenderer spriteRenderer;
    private AudioSource audioSource;
    private bool isTransformed = false;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Ensure there's an AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        spriteRenderer.sprite = normalSprite;
    }

    void Update()
    {
        // Press P to toggle transformation
        if (Input.GetKeyDown(KeyCode.P))
        {
            ToggleTransformation();
        }
    }

    private void ToggleTransformation()
    {
        isTransformed = !isTransformed;

        // Swap sprite
        spriteRenderer.sprite = isTransformed ? transformedSprite : normalSprite;

        // Play VFX
        if (transformVFX != null)
        {
            Instantiate(transformVFX, transform.position, Quaternion.identity);
        }

        // Play SFX
        if (transformSFX != null)
        {
            audioSource.PlayOneShot(transformSFX, sfxVolume);
        }
    }
}
