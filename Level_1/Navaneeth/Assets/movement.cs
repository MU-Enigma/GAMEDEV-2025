using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D player;
    public float speed = 5f;
    public float jumpForce = 7f;
    private SpriteRenderer spriteRenderer;
    private bool isGrounded = true; // To check if the player is on the ground

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // Horizontal movement only
        float moveX = Input.GetAxisRaw("Horizontal");
        player.linearVelocity = new Vector2(moveX * speed, player.linearVelocity.y);

        // Flip sprite when moving left/right
        if (moveX != 0)
        {
            spriteRenderer.flipX = moveX < 0;
        }

        // Jump when pressing Space and grounded
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            player.linearVelocity = new Vector2(player.linearVelocity.x, jumpForce);
            isGrounded = false;
        }
    }

    // Detect when player touches the ground again
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.contacts.Length > 0)
        {
            // You can add a tag check here if needed (e.g., collision.gameObject.CompareTag("Ground"))
            isGrounded = true;
        }
    }
}
