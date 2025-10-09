using System.Collections;
using UnityEngine;

/// <summary>
/// The camera script. Dear god, I've seen so many bad camera scripts. This one *shouldn't* suck.
/// It uses LateUpdate so it moves AFTER the player has moved, which prevents that god-awful jittery effect.
/// </summary>
public class CameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target; // drag the player here. if you forget, it will break. obviously.
    private Transform _originalTarget;
    private float _initialSpeed;
    private GameObject _tempTarget; // used for temporary targets

    [Header("Follow Settings")]
    public Vector3 offset = new Vector3(0, 0, -10f); // z should be -10 for a 2D camera but you do you
    [Tooltip("How fast the camera tries to catch up. Higher value = tighter follow.")]
    public float followSpeed = 12f;

    [Header("Anti-Jitter for Pixel Art")]
    public bool snapToPixels = false; // turn this on if you're making a pixel art game
    public float pixelsPerUnit = 16f; // match this to your sprites' PPU
    void Start()
    {
        _originalTarget = target;
        _initialSpeed = followSpeed;
    }
    void LateUpdate()
    {
        if (target == null) return; // if there's no target just... dont do anything.

        // 1. Figure out where we WANT to be.
        Vector3 desiredPosition = target.position + offset;

        // 2. Smoothly move from where we ARE to where we WANT to be.
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, followSpeed * Time.deltaTime);

        // 3. The pixel snapping voodoo.
        // If this is on, it rounds the camera's position to the nearest "pixel" based on your PPU.
        // This stops sprites from looking like they're shimmering or tearing when the camera moves.
        if (snapToPixels)
        {
            smoothedPosition.x = (Mathf.Round(smoothedPosition.x * pixelsPerUnit) / pixelsPerUnit);
            smoothedPosition.y = (Mathf.Round(smoothedPosition.y * pixelsPerUnit) / pixelsPerUnit);
        }

        // 4. Actually move the damn camera.
        transform.position = smoothedPosition;
    }

    /// <summary>
    /// Call this if you need to instantly snap the camera to the player, like on a scene load or respawn.
    /// </summary>
    public void SnapToTarget()
    {
        if (target == null) return;
        transform.position = target.position + offset;
    }

    public void setTargetTemporary(Vector3 newTarget, float speedFactor)
    {
        _tempTarget = new GameObject("TempCameraTarget");
        _tempTarget.transform.position = newTarget;
        target = _tempTarget.transform;
        followSpeed *= speedFactor;
        StartCoroutine(resetTargetUponReaching());
    }

    IEnumerator resetTargetUponReaching()
    {
        yield return new WaitUntil(() => Vector3.Distance(transform.position, target.position + offset) < 0.1f);
        //magic line of code, waits until the camera is close enough to the temp target
        //then poof, we're following the player again.
        target = _originalTarget;
        followSpeed = _initialSpeed;
        if(_tempTarget) Destroy(_tempTarget);
    }

}
