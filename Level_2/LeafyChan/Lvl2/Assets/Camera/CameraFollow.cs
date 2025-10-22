// SmoothCameraFollow.cs
using UnityEngine;

public class SmoothCameraFollow : MonoBehaviour
{
    [Header("Target")]
    public Transform target;
    
    [Header("Follow Settings")]
    public float smoothSpeed = 5f;
    public Vector3 offset = new Vector3(0f, 0f, -10f);
    
    [Header("Advanced Settings")]
    public bool useBounds = false;
    public Vector2 minBounds;
    public Vector2 maxBounds;
    public float lookAheadFactor = 0.5f;
    public float lookAheadSmooth = 5f;
    
    private Vector3 currentVelocity;
    private Vector3 lookAheadTarget;

    void LateUpdate()
    {
        if (target == null)
            return;
        
        // Calculate target position with look ahead
        Vector3 targetPosition = target.position + offset;
        
        // Add look ahead based on movement direction
        if (target.TryGetComponent<Rigidbody2D>(out var rb))
        {
            Vector3 moveDirection = rb.linearVelocity.normalized;
            lookAheadTarget = Vector3.Lerp(lookAheadTarget, moveDirection * lookAheadFactor, lookAheadSmooth * Time.deltaTime);
            targetPosition += lookAheadTarget;
        }
        
        // Apply bounds if enabled
        if (useBounds)
        {
            targetPosition.x = Mathf.Clamp(targetPosition.x, minBounds.x, maxBounds.x);
            targetPosition.y = Mathf.Clamp(targetPosition.y, minBounds.y, maxBounds.y);
        }
        
        // Smooth follow using SmoothDamp
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref currentVelocity, smoothSpeed * Time.deltaTime);
    }
    
    void OnDrawGizmosSelected()
    {
        if (useBounds)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(new Vector3((minBounds.x + maxBounds.x) * 0.5f, (minBounds.y + maxBounds.y) * 0.5f, 0f),
                              new Vector3(maxBounds.x - minBounds.x, maxBounds.y - minBounds.y, 1f));
        }
    }
}