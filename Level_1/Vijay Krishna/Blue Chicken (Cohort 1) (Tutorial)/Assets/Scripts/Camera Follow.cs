using UnityEngine;
using System.Collections;

/// <summary>
/// Makes a camera smoothly follow a target with offset, and supports advanced features
/// like pausing following, smooth transitions, and camera shake for special abilities.
/// </summary>
public class SmoothCameraFollow : MonoBehaviour
{
    [Tooltip("The Transform of the object the camera should follow (your player).")]
    public Transform target;
    [Tooltip("The time it takes for the camera to reach the target. A smaller value is faster.")]
    public float smoothTime = 0.2f;
    [Tooltip("The maximum speed the camera can move. Prevents jerky movement.")]
    public float maxSpeed = 10.0f;
    [Tooltip("The desired z-position of the camera. Crucial for a 2D game.")]
    public float zOffset = -10.0f;

    [Header("Camera Positioning")]
    [Tooltip("Offset the camera position relative to the target.")]
    public Vector2 cameraOffset = Vector2.zero;

    // --- ADVANCED ---
    private bool isFollowing = true;         // For flight, etc.
    private Vector3 velocity = Vector3.zero;
    private Vector3 originalPosition;
    private Coroutine shakeCoroutine;
    private Coroutine transitionCoroutine;


    void LateUpdate()
    {
        if (isFollowing && target != null)
        {
            Vector3 targetPosition = new Vector3(
                target.position.x + cameraOffset.x,
                target.position.y + cameraOffset.y,
                zOffset
            );
            transform.position = Vector3.SmoothDamp(
                transform.position,
                targetPosition,
                ref velocity,
                smoothTime,
                maxSpeed
            );
        }
        // Else, the camera stays where it is (useful for special moves)
    }

    // ------ PUBLIC API for PlayerController ------

    /// <summary>
    /// Temporarily stop automatic camera following.
    /// </summary>
    public void StopFollowing()
    {
        isFollowing = false;
    }

    /// <summary>
    /// Resume automatic camera following of the target.
    /// </summary>
    public void ResumeFollowing()
    {
        isFollowing = true;
    }

    /// <summary>
    /// Instantly move the camera to a world position (e.g., for starting flight).
    /// </summary>
    public void SetToPosition(Vector2 pos)
    {
        transform.position = new Vector3(pos.x, pos.y, zOffset);
        velocity = Vector3.zero;
    }

    /// <summary>
    /// Smoothly transition camera to a target world position over a given time.
    /// </summary>
    public IEnumerator TransitionToPosition(Vector2 targetPos, float duration)
    {
        StopFollowing();
        Vector3 start = transform.position;
        Vector3 end = new Vector3(targetPos.x, targetPos.y, zOffset);
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
        transform.position = end;
    }

    /// <summary>
    /// Triggers a camera shake effect for intensity & duration (e.g. for ground smash).
    /// </summary>
    public void ShakeCamera(float intensity, float duration)
    {
        if (shakeCoroutine != null)
            StopCoroutine(shakeCoroutine);
        shakeCoroutine = StartCoroutine(DoCameraShake(intensity, duration));
    }

    private IEnumerator DoCameraShake(float intensity, float duration)
    {
        originalPosition = transform.position;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;
            transform.position = originalPosition + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }
        transform.position = originalPosition;
    }
}
