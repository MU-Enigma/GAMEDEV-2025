// Hook.cs
using UnityEngine;

public class Hook : MonoBehaviour
{
    // --- Script Variables --- //
    private PlayerHook playerHook;
    private Rigidbody2D rb;
    private LineRenderer line;
    private Transform hookSpawnPoint;
    private Vector3 startPosition;
    private float maxTravelDistance;
    private bool hasHit = false;

    [Header("Settings")]
    public float destroyDelayAfterHit = 1.0f;
    // This public field MUST be assigned in the Inspector
    public Transform ropeAnchor; 

    [Header("Effects")]
    public GameObject impactEffect;

    void Start()
    {
        startPosition = transform.position;
    }

    public void Init(PlayerHook hook, Transform spawnPoint, float travelDistance)
    {
        playerHook = hook;
        rb = GetComponent<Rigidbody2D>();
        hookSpawnPoint = spawnPoint;
        line = GetComponent<LineRenderer>();
        maxTravelDistance = travelDistance;
    }

    void Update()
    {
        if (hasHit) return;

        // Rotate the hook to face its velocity
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            float angle = Mathf.Atan2(rb.linearVelocity.y, rb.linearVelocity.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }

        // Update the line renderer
        if (line != null && hookSpawnPoint != null)
        {
            // --- THIS IS THE CRITICAL LOGIC --- //
            // Define the connection point. Use the anchor if assigned, otherwise default to the pivot.
            Vector3 connectionPoint = (ropeAnchor != null) ? ropeAnchor.position : transform.position;

            // Set the start and end points of the line.
            line.SetPosition(0, hookSpawnPoint.position);
            line.SetPosition(1, connectionPoint);

            // Calculate distance to the anchor for correct texture tiling.
            float distance = Vector3.Distance(hookSpawnPoint.position, connectionPoint);
            line.material.mainTextureScale = new Vector2(1, distance); // Fix for vertical sprite
        }

        // Check for max travel distance
        if (Vector3.Distance(transform.position, startPosition) >= maxTravelDistance)
        {
            Destroy(gameObject);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        hasHit = true;
        rb.linearVelocity = Vector2.zero;

        if (impactEffect != null)
        {
            Vector2 impactPoint = col.contacts[0].point;
            Instantiate(impactEffect, impactPoint, Quaternion.identity);
        }

        if (col.gameObject.CompareTag("Enemy"))
        {
            playerHook.PullEnemy(col.gameObject);
            Destroy(gameObject);
        }
        else
        {
            Destroy(gameObject, destroyDelayAfterHit);
        }
    }
}