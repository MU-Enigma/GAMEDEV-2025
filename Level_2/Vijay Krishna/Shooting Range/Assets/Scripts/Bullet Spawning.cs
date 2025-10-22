using UnityEngine;

public class BulletSpawning : MonoBehaviour
{
    public GameObject bulletPrefab; // Reference to the bullet prefab
    public Transform firePoint; // The point from which the bullet will be fired

    // This is a simplified way to track direction, based on movement
    private Vector2 lastDirection = Vector2.up; // Default direction

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        // Get player input
    float moveX = Input.GetAxisRaw("Horizontal");
    float moveY = Input.GetAxisRaw("Vertical");

    // Update the last known direction if there's movement
    if (moveX != 0 || moveY != 0)
    {
        lastDirection = new Vector2(moveX, moveY).normalized;
    }

    // Fire the projectile on button press
    if (Input.GetMouseButtonDown(0)) // Check if the left mouse button is pressed
    {
        GameObject newProjectile = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        newProjectile.GetComponent<BulletFiring>().SetDirection(lastDirection);
    }
    }
}