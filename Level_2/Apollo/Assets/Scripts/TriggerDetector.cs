using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger : MonoBehaviour
{
    //Trigger Detection script
    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("Trigger Detected with " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("Player has entered the trigger area.");
        }   
    }
}
