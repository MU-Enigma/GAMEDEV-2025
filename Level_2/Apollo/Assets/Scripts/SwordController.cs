using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SwordController : MonoBehaviour
{
    private Animator animator;
    private bool isSwinging = false;
    private float swingTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        //Check for mouse click to swing sword
        if (Input.GetMouseButtonDown(0) && !isSwinging)
        {
            //Starts swinging animation
            isSwinging = true;
            animator.SetBool("isSwinging", true);
            swingTimer = 1f; // Sword swing duration
        }

        if (isSwinging)
        {
            swingTimer -= Time.deltaTime;

            if (swingTimer <= 0f)
            {
                //Stops swinging animation
                isSwinging = false;
                animator.SetBool("isSwinging", false);
            }
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Enemy") && isSwinging)
        {
            Debug.Log("Enemy hit: " + other.name);
        }
    }
}
