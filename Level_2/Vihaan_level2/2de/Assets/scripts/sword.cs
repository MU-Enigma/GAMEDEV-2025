using UnityEngine;

public class sword : MonoBehaviour
{
    private Animator animator;
    [SerializeField] private TrailRenderer swordTrail; // must be assigned in Inspector
    void Start()
    {
        animator = GetComponent<Animator>();
        swordTrail.enabled = false;
    }

    void Update()
    {
        bool isSwinging = animator.GetCurrentAnimatorStateInfo(0).IsName("swing");

        if (isSwinging)
        {
            swordTrail.enabled = true;
            gameObject.tag = "Bullet";
            gameObject.layer = LayerMask.NameToLayer("Default");
        }
        else
        {
            swordTrail.enabled = false;
            gameObject.tag = "Untagged";
            gameObject.layer = LayerMask.NameToLayer("player");
        }

        if (Input.GetMouseButton(0))
            animator.SetBool("LeftClick", true);
        else
            animator.SetBool("LeftClick", false);
    }
}
