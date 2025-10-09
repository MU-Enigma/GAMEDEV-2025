using UnityEngine;

public class CD : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered pond!");
            // Example: slow down player or change animation
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited pond!");
            // Example: restore speed or change animation back
        }
    }
}
