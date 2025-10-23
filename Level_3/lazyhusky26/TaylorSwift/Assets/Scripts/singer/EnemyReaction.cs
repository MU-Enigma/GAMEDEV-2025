using UnityEngine;
using System.Collections;

public class EnemyReaction : MonoBehaviour
{
    public AudioClip shoutClip;         // assign the shout clip here (AudioClip)
    public float shakeDuration = 1f;
    public float shakeIntensity = 0.2f;
    public GameObject disappearVFX;     // assign VFX prefab

    private bool reacting = false;
    private Vector3 originalPos;

    public void OnHeardSong()
    {
        if (reacting) return;
        reacting = true;
        StartCoroutine(ReactAndDisappear());
    }

    IEnumerator ReactAndDisappear()
    {
        originalPos = transform.position;
        float elapsed = 0f;

        // Shake violently
        while (elapsed < shakeDuration)
        {
            float x = Random.Range(-1f, 1f) * shakeIntensity;
            float y = Random.Range(-1f, 1f) * shakeIntensity;
            transform.position = originalPos + new Vector3(x, y, 0);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // Restore position
        transform.position = originalPos;

        // Start playing shout sound 0.2 seconds before disappearing
        if (shoutClip != null)
            PlayShoutSound(shoutClip);

        // Wait 0.2 seconds so shout leads disappearance
        yield return new WaitForSeconds(0.2f);

        // Spawn VFX at current position
        if (disappearVFX != null)
            Instantiate(disappearVFX, transform.position, Quaternion.identity);

        // Destroy enemy immediately
        Destroy(gameObject);
    }

    void PlayShoutSound(AudioClip clip)
    {
        GameObject audioGO = new GameObject("ShoutAudio");
        audioGO.transform.position = transform.position;
        AudioSource source = audioGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.Play();
        Destroy(audioGO, clip.length);
    }
}
