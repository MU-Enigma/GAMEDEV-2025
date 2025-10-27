using UnityEngine;

public class SordCont : MonoBehaviour
{
    private Animator animator;
    private bool isSwinging = false;
    private float swingTimer = 0f;
    public float swingDuration = 1f;

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

    void StartSwing()
    {
        isSwinging = true;
        animator.SetBool("IsSwinging", true);
        swingTimer = swingDuration;
        Debug.Log("Sword swing started!");
    }

    void EndSwing()
    {
        isSwinging = false;
        animator.SetBool("IsSwinging", false);
        Debug.Log("Sword swing ended!");
    }

    public bool IsSwinging()
    {
        return isSwinging;
    }
}