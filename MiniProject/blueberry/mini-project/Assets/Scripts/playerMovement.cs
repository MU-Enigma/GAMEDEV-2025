using UnityEngine;

public class playermovement : MonoBehaviour
{
    public Rigidbody2D player;
    public float speed = 3f;
    private bool facingRight = true;

    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        player.linearVelocity = new Vector2(moveX, moveY).normalized * speed;

        if (moveX > 0 && !facingRight)
            Flip();
        else if (moveX < 0 && facingRight)
            Flip();
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }
}
