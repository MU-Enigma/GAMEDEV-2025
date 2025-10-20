using UnityEngine;

public class swordScript : MonoBehaviour
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
            animator.SetBool("LeftClick", true);
        }
        else
        {
            animator.SetBool("LeftClick", false);
        }
    }
}