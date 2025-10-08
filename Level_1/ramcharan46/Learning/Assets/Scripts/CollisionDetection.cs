using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    private Animator animator;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Collided with: " + other.name);

        } 
    }
}
