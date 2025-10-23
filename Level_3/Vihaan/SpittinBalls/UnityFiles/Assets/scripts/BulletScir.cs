using UnityEngine;

public class Pellet : MonoBehaviour
{
    public float Speed = 10f;
    public float DesTime = 2f;

    private Vector2 targetDirection;

    void Start()
    {
        // Get the mouse position in world space
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0;  // Reset z for 2D space
        
        // Calculate the direction from the bullet to the mouse position
        targetDirection = (mousePos - transform.position).normalized;

        Destroy(gameObject, DesTime);
    }

    void Update()
    {
        // Move the bullet toward the mouse position
        transform.Translate(targetDirection * Speed * Time.deltaTime, Space.World);
    }
}
