using UnityEngine;
using System.Collections;

public class EnemyDomainEffect : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public bool IsDone { get; private set; } = false;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void StartFlashingAndExplode(float flashDuration, GameObject explosionVFX, AudioClip deathSFX, AudioSource audioSource)
    {
        StartCoroutine(FlashAndExplodeRoutine(flashDuration, explosionVFX, deathSFX, audioSource));
    }

    private IEnumerator FlashAndExplodeRoutine(float flashDuration, GameObject explosionVFX, AudioClip deathSFX, AudioSource audioSource)
    {
        Color[] colors = new Color[] { Color.red, Color.green, Color.blue, Color.yellow, Color.magenta };
        float timer = 0f;
        int colorIndex = 0;

        while (timer < flashDuration)
        {
            spriteRenderer.color = colors[colorIndex];
            colorIndex = (colorIndex + 1) % colors.Length;

            timer += 0.2f;
            yield return new WaitForSeconds(0.2f);
        }

        spriteRenderer.color = Color.white;

        // Play explosion VFX
        if (explosionVFX != null)
        {
            Instantiate(explosionVFX, transform.position, Quaternion.identity);
        }

        // Play death SFX
        if (deathSFX != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSFX);
        }

        // Hide enemy sprite so it looks destroyed
        spriteRenderer.enabled = false;

        // Wait for deathSFX length or fallback delay
        float waitTime = (deathSFX != null) ? deathSFX.length : 1f;
        yield return new WaitForSeconds(waitTime);

        IsDone = true;

        // Destroy enemy gameobject
        Destroy(gameObject);
    }
}
