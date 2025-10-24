using UnityEngine;

public class Collision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        //Debug.Log("Entered Object " + other.name);
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered");
        }
    }
}
