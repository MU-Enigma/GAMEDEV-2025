using UnityEngine;

public class Sword : MonoBehaviour
{
    [Header("Swing Settings")]
    [SerializeField] private Animator animator;
    [SerializeField] private float swingCooldown = 0.5f;
    
    [Header("Audio (Optional)")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip swingSound;
    
    [Header("Screen Shake (Optional)")]
    [SerializeField] private bool enableScreenShake = false;
    [SerializeField] private float shakeAmount = 0.1f;
    
    // Public variable for collision detection
    public bool isSwinging = false;
    
    public bool canSwing = true;
    private float cooldownTimer = 0f;

    void Update()
    {
        // Handle cooldown
        if (!canSwing)
        {
            cooldownTimer -= Time.deltaTime;
            if (cooldownTimer <= 0f)
            {
                canSwing = true;
            }
        }

        // Check for B button press
        if (Input.GetKeyDown(KeyCode.Mouse0 ) && canSwing)
        {
            Swing();
        }
    }

    void Swing()
    {
        // Start cooldown
        canSwing = false;
        cooldownTimer = swingCooldown;

        // Set swinging state to true
        isSwinging = true;

        // Play animation
        animator.SetTrigger("swing");
        animator.Play("swing", -1, 0f);

        // Play sound effect
        if (audioSource != null && swingSound != null)
        {
            audioSource.PlayOneShot(swingSound);
        }

        // Optional: Screen shake for impact feel
        if (enableScreenShake)
        {
            StartCoroutine(ScreenShake());
        }

        // Reset swinging state after animation duration
        Invoke("ResetSwing", 0.01f); // Adjust timing to match your animation
    }

    void ResetSwing()
    {
        isSwinging = false;
    }

    // Optional screen shake for juicy feel
    System.Collections.IEnumerator ScreenShake()
    {
        Vector3 originalPos = Camera.main.transform.position;
        float elapsed = 0f;
        float duration = 0.15f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * shakeAmount;
            float y = Random.Range(-1f, 1f) * shakeAmount;

            Camera.main.transform.position = new Vector3(originalPos.x + x, originalPos.y + y, originalPos.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        Camera.main.transform.position = originalPos;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (isSwinging && collision.gameObject.CompareTag("Enemy"))
        {
            Destroy(collision.gameObject);
        }
    }

    
}