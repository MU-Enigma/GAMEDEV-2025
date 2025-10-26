using UnityEngine;
using System.Collections;

public class TeleportController : MonoBehaviour
{
    public KeyCode teleportKey = KeyCode.E; 
    public AudioClip teleportSFX;
    public float teleportDuration = 0.1f;

    private AudioSource audioSource;
    private bool canTeleport = false;
    private bool isTeleporting = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (isTeleporting) return;

        if (Input.GetKeyDown(teleportKey))
        {
            canTeleport = true;
        }

        if (canTeleport && Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0;

            StartCoroutine(TeleportWithEffect(mousePos));

            canTeleport = false;
        }

        if (canTeleport && Input.GetMouseButtonDown(1))
        {
            canTeleport = false;
        }
    }

    private IEnumerator TeleportWithEffect(Vector3 targetPosition)
    {
        isTeleporting = true;

        Vector3 originalScale = transform.localScale;
        Vector3 shrinkScale = new Vector3(0f, originalScale.y, originalScale.z);

        float t = 0f;
        while (t < teleportDuration)
        {
            transform.localScale = Vector3.Lerp(originalScale, shrinkScale, t / teleportDuration);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localScale = shrinkScale;

        transform.position = targetPosition;

        if (teleportSFX != null)
            audioSource.PlayOneShot(teleportSFX);

        t = 0f;
        while (t < teleportDuration)
        {
            transform.localScale = Vector3.Lerp(shrinkScale, originalScale, t / teleportDuration);
            t += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;

        isTeleporting = false;
    }
}
