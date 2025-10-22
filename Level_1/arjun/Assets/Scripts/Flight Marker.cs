using UnityEngine;

public class FlightMarker : MonoBehaviour
{
    [Header("Marker Animation")]
    [Tooltip("How fast the marker bobs up and down.")]
    public float bobSpeed = 2f;
    [Tooltip("Height of the bobbing animation.")]
    public float bobHeight = 0.2f;
    [Tooltip("Should the marker rotate while active?")]
    public bool rotateMarker = true;
    [Tooltip("Rotation speed of the marker.")]
    public float rotationSpeed = 90f;
    
    private Vector3 startPosition;
    
    void Start()
    {
        startPosition = transform.position;
    }
    
    void Update()
    {
        // Bobbing animation
        float bobOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPosition + Vector3.up * bobOffset;
        
        // Rotation animation
        if (rotateMarker)
        {
            transform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
    }
}
