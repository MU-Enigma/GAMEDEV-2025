using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        // Get the Animator component (on Player or Weapon)
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Left mouse button (0) pressed
        if (Input.GetMouseButtonDown(0))
        {
            animator.SetTrigger("Attack");
        }
    }
}