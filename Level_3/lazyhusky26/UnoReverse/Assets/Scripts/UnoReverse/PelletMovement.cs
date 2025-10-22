using UnityEngine;

public class PelletMovement : MonoBehaviour
{
    public float speed = 10f;
    public Transform player;
    public Transform enemy;

    private bool reversed = false;
    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        if (player == null || enemy == null)
            return;

        Vector2 target = reversed ? (Vector2)enemy.position : (Vector2)player.position;
        Vector2 direction = (target - rb.position).normalized;
        rb.linearVelocity = direction * speed;
    }

    public void Reverse()
    {
        reversed = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!reversed)
        {
            // If pellet hits player (or the object it was fired at)
            if (other.CompareTag("Player"))
            {
                Debug.Log("Player hit! No Uno Reverse!");
                Destroy(gameObject);
                // You could add player damage logic here or call a method on player script
            }
            else if (other.CompareTag("Wall"))
            {
                Destroy(gameObject);
            }
        }
        else
        {
            // Pellet reversed — hits enemy or wall
            if (other.CompareTag("Enemy"))
            {
                Debug.Log("Enemy killed by Uno Reverse pellet!");
                // Spawn blood effect at enemy position
                if (other.transform != null)
                {
                    Instantiate(GameObject.FindObjectOfType<UnoReverseMechanic>().bloodVFXPrefab, other.transform.position, Quaternion.identity);
                }
                Destroy(other.gameObject); // Destroy enemy
                Destroy(gameObject); // Destroy pellet
            }
            else if (other.CompareTag("Wall"))
            {
                Destroy(gameObject);
            }
        }
    }
}
