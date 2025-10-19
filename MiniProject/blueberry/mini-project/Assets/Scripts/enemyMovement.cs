using UnityEngine;

public class EnemyFollow : MonoBehaviour
{
    [Header("Follow Settings")]
    public Transform player;
    public float speed = 5f;
    public bool flip = true;
    private bool facingRight = true;
    void Update()
    {
        if (player == null) return;
        Vector3 direction = (player.position - transform.position).normalized;
        transform.position += new Vector3(direction.x, 0f, 0f) * speed * Time.deltaTime;

        if (flip)
        {
            if (direction.x > 0 && !facingRight)
                Flip();
            else if (direction.x < 0 && facingRight)
                Flip();
        }
    }
    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= 1;
        transform.localScale = scale;
    }
}
