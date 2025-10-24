using UnityEngine;

public class TriggerDetector : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Object entered: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player detected!");
        }
    }
}
