using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    public float defaultShakeIntensity = 0.2f;
    public float defaultShakeDuration = 0.3f;
    
    private Vector3 originalPosition;
    private float shakeTimer = 0f;
    private float shakeIntensity = 0f;
    private SmoothCameraFollow cameraFollow;

    void Start()
    {
        cameraFollow = GetComponent<SmoothCameraFollow>();
        originalPosition = transform.localPosition;
    }

    void LateUpdate()
    {
        if (shakeTimer > 0)
        {
            // Calculate shake offset using Perlin noise for smooth random movement
            float shakeX = Mathf.PerlinNoise(Time.time * 30f, 0f) * 2f - 1f;
            float shakeY = Mathf.PerlinNoise(0f, Time.time * 30f) * 2f - 1f;
            
            Vector3 shakeOffset = new Vector3(shakeX, shakeY, 0f) * shakeIntensity;
            
            // Apply the shake offset to the camera's position
            // This works alongside the SmoothCameraFollow which runs in LateUpdate
            transform.localPosition = originalPosition + shakeOffset;
            
            shakeTimer -= Time.deltaTime;
        }
        else if (shakeTimer < 0)
        {
            shakeTimer = 0f;
            transform.localPosition = originalPosition;
        }
    }

    public void TriggerShake()
    {
        TriggerShake(defaultShakeIntensity, defaultShakeDuration);
    }

    public void TriggerShake(float intensity, float duration)
    {
        shakeIntensity = intensity;
        shakeTimer = duration;
        
        // Store the current position as the original position when shake starts
        // This ensures we always shake from the correct base position
        originalPosition = transform.localPosition;
    }
}