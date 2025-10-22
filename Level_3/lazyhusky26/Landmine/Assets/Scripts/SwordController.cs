using UnityEngine;

public class SwordController : MonoBehaviour
{
    private Animator animator;
    private bool isSwinging = false;
    private float swingTimer = 0f;

    [SerializeField]
    private float swingDuration = 1f; // Adjustable in Inspector

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isSwinging)
        {
            StartSwing();
        }

        if (isSwinging)
        {
            swingTimer -= Time.deltaTime;

            if (swingTimer <= 0f)
            {
                EndSwing();
            }
        }
    }

    private void StartSwing()
    {
        isSwinging = true;
        animator.SetBool("IsSwinging", true);
        swingTimer = swingDuration;
    }

    private void EndSwing()
    {
        isSwinging = false;
        animator.SetBool("IsSwinging", false);
    }
}
