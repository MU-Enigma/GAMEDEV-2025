using UnityEngine;

public class WeaponHit : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("Tag of the objects this weapon can destroy (e.g., 'Enemy')")]
    public string targetTag = "Enemy";

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Check if the collider belongs to the intended target
        if (other.CompareTag(targetTag))
        {
            Debug.Log("Hit target: " + other.name);
            // Destroy the enemy GameObject
            Destroy(other.gameObject);
        }
    }
}