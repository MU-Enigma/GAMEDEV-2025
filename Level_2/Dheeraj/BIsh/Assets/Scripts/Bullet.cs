using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 8f;
    public float life = 5f;
    private Vector2 moveInput;
    void Start()
    {
        Destroy(gameObject, life);
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();
    }

    void Update()
    {
        if (moveInput == Vector2.zero)
        {
            transform.Translate(Vector2.up * speed * Time.deltaTime);
        }
        else
        {
            transform.Translate(moveInput * speed * Time.deltaTime);
        }
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}