// ScreenShake.cs
using UnityEngine;
using System.Collections;

public class ScreenShake : MonoBehaviour
{
    // Make this a singleton so it's easy to access from other scripts
    public static ScreenShake instance;

    private Vector3 originalPosition;
    
    void Awake()
    {
        // Set up the singleton instance
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    // Public method to be called from other scripts
    public void TriggerShake(float duration, float magnitude)
    {
        StartCoroutine(Shake(duration, magnitude));
    }

    private IEnumerator Shake(float duration, float magnitude)
    {
        float elapsed = 0.0f;

        while (elapsed < duration)
        {
            // Generate a random offset
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude;

            // Apply the offset to the camera's position
            transform.localPosition = new Vector3(originalPosition.x + x, originalPosition.y + y, originalPosition.z);

            elapsed += Time.deltaTime;

            // Wait until the next frame
            yield return null;
        }

        transform.localPosition = originalPosition;
    }
}