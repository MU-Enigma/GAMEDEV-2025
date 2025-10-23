using UnityEngine;

public class Sword : MonoBehaviour
{
    private Rigidbody2D rb;
    private Animator animator;
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space)){
            if (animator!= null){
                animator.SetTrigger("attack");
            }
        }
    }
}
