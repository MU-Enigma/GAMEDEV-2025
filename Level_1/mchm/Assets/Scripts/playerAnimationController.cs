using UnityEngine;
using System.Collections;

/// <summary>
/// This script acts as a centralized hub for controlling all player animations.
/// Other scripts, like playerController and playerAbilities, will call methods on this
/// component instead of directly manipulating the Animator.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;

    // --- Animator Parameter Hashes ---
    // using integer hashes instead of raw strings is way faster. we do this for everything.
    // it's a one-time cost at the start instead of a string comparison every single time we call a method.
    private readonly int isChargingHash = Animator.StringToHash("isCharging");
    private readonly int chargePercentHash = Animator.StringToHash("chargePercent");
    private readonly int isFullyChargedHash = Animator.StringToHash("isFullyCharged");
    private readonly int isBlueHash = Animator.StringToHash("isBlue");
    private readonly int jumpTriggerHash = Animator.StringToHash("jumpTrigger");

    void Awake()
    {
        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogError("PlayerAnimationController: Animator component not found on this GameObject! We're boned.", this);
    }

    // --- Methods for Abilities ---

    public void SetChargingState(bool isCharging) => animator?.SetBool(isChargingHash, isCharging);

    public void SetChargePercentage(float chargePercentage) => animator?.SetFloat(chargePercentHash, chargePercentage);

    public void SetFullyChargedState(bool isFullyCharged) => animator?.SetBool(isFullyChargedHash, isFullyCharged);


    // --- Methods for Visual State ---

    /// <summary>
    /// Sets the animator's 'isBlue' boolean for speed boost visual feedback.
    /// </summary>
    /// <param name="blueState">True if the player should be blue.</param>
    public void SetIsBlue(bool blueState) => animator?.SetBool(isBlueHash, blueState);
}

