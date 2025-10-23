using UnityEngine;
using System.Collections;

public class EnemyReaction : MonoBehaviour
{
    public AudioClip shoutClip;
    public float shakeDuration = 1f;
    public float shakeIntensity = 0.2f;
    public GameObject disappearVFX;

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

        transform.position = originalPos;

        if (shoutClip != null)
            PlayShoutSound(shoutClip);

        yield return new WaitForSeconds(0.2f);

        if (disappearVFX != null)
            Instantiate(disappearVFX, transform.position, Quaternion.identity);

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
