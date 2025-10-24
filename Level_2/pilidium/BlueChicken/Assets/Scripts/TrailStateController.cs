using UnityEngine;

public class TrailStateController : StateMachineBehaviour
{
    [Tooltip("If true, the trail disappears instantly. If false, it fades out.")]
    public bool clearTrailInstantly = false;

    private TrailRenderer trail;

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (trail == null)
        {
            trail = animator.GetComponentInChildren<TrailRenderer>();
        }

        if (trail != null)
        {
            trail.emitting = false;

            if (clearTrailInstantly)
            {
                trail.Clear();
            }
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (trail != null)
        {
            trail.emitting = true;
        }
    }
}