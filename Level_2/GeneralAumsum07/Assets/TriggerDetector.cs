 using UnityEngine;
 public class TriggerDetector : MonoBehaviour { 
    void OnTriggerEnter2D(Collider2D other) { 
        Debug.Log("Something entered the trigger!"); } 
        }