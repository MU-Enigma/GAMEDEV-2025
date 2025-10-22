using UnityEngine;

public class Kunai : MonoBehaviour
{
    public float Speed = 15f;
    public float DesTime = 2f;
    private Vector2 moveDirection;

    public void SetDirection(Vector2 dir)
    {
        moveDirection = dir.normalized;

        // Rotate kunai to face the movement direction
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        angle += 180f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void Start()
    {
        Destroy(gameObject, DesTime);
    }

    void Update()
    {
        // Move in world space along moveDirection
        transform.Translate(moveDirection * Speed * Time.deltaTime, Space.World);
    }
}
