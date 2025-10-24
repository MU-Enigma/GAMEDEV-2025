using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DeathNoteRitual : MonoBehaviour
{
    public AudioClip heartbeatClip;
    public AudioClip deathClip;
    public GameObject target;           // Enemy or player to kill
    public float heartbeatInterval = 1f;  // Seconds between heartbeats
    public int heartbeatCount = 3;         // Number of heartbeats before death
    public Color pulseColor = Color.red;   // Color to pulse to
    public float pulseFrequency = 4f;      // Pulses per second

    private SpriteRenderer[] spriteRenderers;
    private Color[] originalColors;

    bool running = false;

    public void Init(GameObject target, AudioClip heartbeat, AudioClip death, Color pulseColor, float pulseFrequency)
    {
        this.target = target;
        this.heartbeatClip = heartbeat;
        this.deathClip = death;
        this.pulseColor = pulseColor;
        this.pulseFrequency = pulseFrequency;

        // Cache all SpriteRenderers in the target for pulsing
        if (target != null)
        {
            spriteRenderers = target.GetComponentsInChildren<SpriteRenderer>();
            originalColors = new Color[spriteRenderers.Length];
            for (int i = 0; i < spriteRenderers.Length; i++)
                originalColors[i] = spriteRenderers[i].color;
        }

        StartCoroutine(RitualCoroutine());
    }

    IEnumerator RitualCoroutine()
    {
        running = true;

        float elapsed = 0f;
        int beatsPlayed = 0;

        while (beatsPlayed < heartbeatCount)
        {
            // Play heartbeat sound
            if (heartbeatClip != null)
                SoundUtils.PlayClipAtPoint(heartbeatClip, transform.position, 1f);

            // Pulse color for one heartbeat interval
            float timer = 0f;
            while (timer < heartbeatInterval)
            {
                timer += Time.deltaTime;
                elapsed += Time.deltaTime;

                if (spriteRenderers != null)
                {
                    float pulse = (Mathf.Sin(timer * pulseFrequency * Mathf.PI * 2f) + 1f) * 0.5f;
                    for (int i = 0; i < spriteRenderers.Length; i++)
                    {
                        if (spriteRenderers[i] != null)
                        {
                            spriteRenderers[i].color = Color.Lerp(originalColors[i], pulseColor, pulse);
                        }
                    }
                }

                yield return null;
            }

            beatsPlayed++;
        }

        // Restore original colors
        if (spriteRenderers != null)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                    spriteRenderers[i].color = originalColors[i];
            }
        }

        // Play death sound
        if (deathClip != null)
            SoundUtils.PlayClipAtPoint(deathClip, target.transform.position, 1f);

        // Kill target
        if (target != null)
            Destroy(target);

        Destroy(gameObject);
        running = false;
    }

    void OnDestroy()
    {
        // Safety restore in case ritual is interrupted
        if (spriteRenderers != null)
        {
            for (int i = 0; i < spriteRenderers.Length; i++)
            {
                if (spriteRenderers[i] != null)
                    spriteRenderers[i].color = originalColors[i];
            }
        }
    }
}
