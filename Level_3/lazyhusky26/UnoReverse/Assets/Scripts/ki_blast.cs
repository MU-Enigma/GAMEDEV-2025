using UnityEngine;

public class ki_blast : MonoBehaviour
{
    public float Speed = 20f;
    public float DesTime = 2f;
    private Vector2 moveDirection;

    public void SetDirection(Vector2 dir)
    {
        moveDirection = dir.normalized;
    }

    void Start()
    {
        Destroy(gameObject, DesTime);
    }

    void Update()
    {
        transform.Translate(moveDirection * Speed * Time.deltaTime);
    }
}
