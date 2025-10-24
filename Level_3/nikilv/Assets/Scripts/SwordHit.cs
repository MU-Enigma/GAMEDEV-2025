using UnityEngine;

public class SwordHit : MonoBehaviour
{
    private Animator animator;
    void Start()
    {
        animator = GetComponent<Animator>();   
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            animator.SetBool("LeftClicked", true);
        }
        else
        {
            animator.SetBool("LeftClicked", false);
        }
    }
}