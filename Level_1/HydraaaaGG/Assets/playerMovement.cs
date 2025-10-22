using UnityEngine;

public class playermovement : MonoBehaviour
{
    public Rigidbody2D player;
    public float speed = 3f;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        player.linearVelocity = new Vector2(moveX, moveY).normalized * speed;

        if (moveX != 0)
        {
            spriteRenderer.flipX = moveX < 0;
        }
    }
}
