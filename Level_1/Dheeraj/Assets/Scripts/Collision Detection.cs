using UnityEngine;

public class CollisionDetection : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Entered object" + other.name);
            if (other.CompareTag("Player"))
    {
        Debug.Log("Player Entered");
    }
    }
}
