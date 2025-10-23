using UnityEngine;

public class PlayerMovementSimple : MonoBehaviour
{
    public float moveSpeed = 5f;          // Movement speed
    private Rigidbody2D rb;               // Reference to Rigidbody2D
    private Vector2 moveInput;            // Stores current input

    void Start()
    {
        // Get the Rigidbody2D component on the same GameObject
        rb = GetComponent<Rigidbody2D>();
    }
    void Update()
    {
        // Get WASD / Arrow key input
        moveInput.x = Input.GetAxisRaw("Horizontal");  // A/D or Left/Right
        moveInput.y = Input.GetAxisRaw("Vertical");    // W/S or Up/Down

        // Normalize so diagonal movement isn't faster
        moveInput = moveInput.normalized;
    }
    void FixedUpdate()
    {
        // Apply movement using physics for smoothness
        rb.linearVelocity = moveInput * moveSpeed;
    }
}