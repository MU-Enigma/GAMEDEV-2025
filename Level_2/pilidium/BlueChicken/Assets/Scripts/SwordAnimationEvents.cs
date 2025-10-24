using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SwordAnimationEvents : MonoBehaviour
{
    private Collider2D swordCollider;

    void Start()
    {
        swordCollider = GetComponent<Collider2D>();
        
        swordCollider.enabled = false;
    }

    public void EnableCollider()
    {
        swordCollider.enabled = true;
    }

    public void DisableCollider()
    {
        swordCollider.enabled = false;
    }
}