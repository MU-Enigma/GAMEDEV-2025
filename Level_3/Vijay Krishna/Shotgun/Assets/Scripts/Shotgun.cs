using UnityEngine;

/// <summary>
/// Rotates a GameObject to point at the mouse cursor in a 2D plane.
///
/// THIS SCRIPT HAS BEEN MODIFIED.
/// It no longer cares about the Player. It assumes a *separate* script
/// (like your Player Controller) is responsible for:
///   1. Setting this gun's 'transform.localPosition' (to move it left/right).
///   2. Setting this gun's 'transform.localScale.x' (to flip it horizontally).
///
/// This script is now *only* responsible for:
///   1. Calculating the rotation to the mouse.
///   2. Adjusting that rotation based on its *own* 'localScale.x'.
///   3. Flipping its *own* 'localScale.y' to stay upright.
///
/// It makes one key assumption:
/// 1. The GUN sprite faces RIGHT by default.
/// </summary>
public class GunRotation : MonoBehaviour
{
    // To store the original local Y scale
    private float originalYScale;
    private Camera mainCam; // --- NEW: Cache the camera

    void Start()
    {
        // Store the original Y scale when the game starts
        // We only care about Y, since X will be controlled by the player script.
        originalYScale = transform.localScale.y;
        
        // --- NEW: Cache the main camera for performance ---
        mainCam = Camera.main;
    }

    void Update()
    {
        if (mainCam == null) return; // Safety check

        // --- 1. Get Mouse Position ---
        Vector3 mouseWorldPosition = mainCam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorldPosition.z = 0f; // Ensure it's on the 2D plane (z=0)

        // --- 2. Calculate Direction (from gun to mouse) ---
        Vector3 direction = mouseWorldPosition - transform.position;

        // --- 3. Calculate Base Angle ---
        // This is the angle *before* any player-flip adjustments
        float baseAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // --- 4. Adjust Angle based on Gun's *Own* Flip ---
        // Start with the base angle
        float finalAngle = baseAngle;
        
        // We check if the gun itself is flipped horizontally.
        // We assume a player script is setting this.
        if (transform.localScale.x < 0)
        {
            // The gun is flipped (facing left).
            // We must add 180 degrees to the angle to make it point correctly.
            finalAngle += 180f;
        }

        // --- 5. Apply Rotation ---
        // Create a rotation quaternion based on our *final* angle
        transform.rotation = Quaternion.Euler(0f, 0f, finalAngle);

        // --- 6. Handle Sprite Flipping (Vertical) ---
        // *** THIS IS THE FIX ***
        // This logic now correctly uses the 'baseAngle' to determine
        // if the gun is pointing "up" or "down".
        
        if (Mathf.Abs(baseAngle) > 90)
        {
            // Gun is rotated > 90 or < -90 degrees, flip Y to keep it upright
            // We use the *current* localScale.x and .z
            transform.localScale = new Vector3(transform.localScale.x, -originalYScale, transform.localScale.z);
        }
        else
        {
            // Gun is in the "upright" rotation range, use original scale Y
            // We use the *current* localScale.x and .z
            transform.localScale = new Vector3(transform.localScale.x, originalYScale, transform.localScale.z);
        }
    }
}

