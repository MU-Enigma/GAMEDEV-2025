using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class NukeMissile : MonoBehaviour
{
    [Header("Nuke Movement")]
    public float fallSpeed = 10f;            // How fast the nuke drops
    public float stopDistance = 1f;          // Distance from center before exploding

    [Header("Explosion Settings")]
    public GameObject explosionPrefab;       // Explosion prefab (particles or animation)
    public float explosionLifetime = 3f;     // How long explosion lasts

    [Header("Impact Screen Effects")]
    public Image impactImage;                // Fullscreen white overlay (see setup below)
    public float flashDuration = 0.2f;       // How long the white flash stays
    public float fadeSpeed = 4f;             // How fast it fades

    [Header("Camera Shake")]
    public float shakeDuration = 0.5f;
    public float shakeMagnitude = 0.5f;

    [Header("Enemy Tag")]
    public string enemyTag = "Enemy";

    private bool exploded = false;
    private Transform camTransform;
    private Vector3 camOriginalPos;
    private Vector3 targetPosition;

    private void Start()
    {
        // Center of screen = camera's forward direction
        camTransform = Camera.main.transform;
        targetPosition = camTransform.position + camTransform.forward * 5f; // adjust distance as needed
    }

    private void Update()
    {
        if (exploded) return;

        // Move missile toward screen center
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, fallSpeed * Time.deltaTime);

        // When close enough, explode
        if (Vector3.Distance(transform.position, targetPosition) <= stopDistance)
        {
            StartCoroutine(Explode());
        }
    }

    private IEnumerator Explode()
    {
        exploded = true;

        // --- Spawn explosion ---
        if (explosionPrefab != null)
        {
            GameObject exp = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            Destroy(exp, explosionLifetime);
        }

        // --- Screen shake ---
        if (camTransform != null)
            StartCoroutine(ScreenShakeEffect());

        // --- Flash effect ---
        if (impactImage != null)
            StartCoroutine(ImpactFlashEffect());

        // --- Destroy all enemies ---
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        foreach (GameObject enemy in enemies)
        {
            Destroy(enemy);
        }

        // Optional delay before missile is removed
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }

    private IEnumerator ScreenShakeEffect()
    {
        camOriginalPos = camTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            float x = (Random.value * 2 - 1) * shakeMagnitude;
            float y = (Random.value * 2 - 1) * shakeMagnitude;
            camTransform.localPosition = camOriginalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        camTransform.localPosition = camOriginalPos;
    }

    private IEnumerator ImpactFlashEffect()
    {
        impactImage.gameObject.SetActive(true);
        Color c = impactImage.color;
        c.a = 1f;
        impactImage.color = c;

        yield return new WaitForSeconds(flashDuration);

        while (impactImage.color.a > 0f)
        {
            c.a -= Time.deltaTime * fadeSpeed;
            impactImage.color = c;
            yield return null;
        }

        impactImage.gameObject.SetActive(false);
    }
}
