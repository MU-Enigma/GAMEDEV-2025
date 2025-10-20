// PlayerVisualEffects.cs
using UnityEngine;
using System.Collections;

public class PlayerVisualEffects : MonoBehaviour
{
    [Header("Squash & Stretch")]
    public float moveSquashAmount = 0.7f;
    public float moveStretchAmount = 1.1f;
    public float dashSquash = 1.2f;
    public float dashStretch = 0.8f;
    public float chargeSquashX = 0.8f;
    public float chargeStretchY = 1.2f;
    public float scaleLerpSpeed = 10f;
    public float bounceFrequency = 8f;
    public float bounceAmplitude = 0.1f;

    [Header("Idle Breathing Animation")]
    public float idleBreathAmplitude = 0.05f;
    public float idleBreathFrequency = 0.8f;
    public float mouseMoveThreshold = 0.3f;

    [Header("Afterimage Settings")]
    public GameObject afterImagePrefab;
    public float afterImageSpacing = 0.05f;

    [Header("Dash Afterimage Burst")]
    public int dashAfterimageCount = 22;
    public float dashAfterimageInterval = 0.06f;
    public float dashAfterimageDuration = 0.5f;

    [Header("Visual Feedback")]
    public ParticleSystem chargeParticles;
    public Gradient chargeColorGradient;

    [Header("Dash Transition Sprites")]
    public Sprite normalSprite;
    public Sprite chargeSprite;
    public Sprite dashSprite;

    private PlayerMovement movement;
    private SpriteRenderer sr;
    private Vector3 originalScale;
    private float bounceTimer = 0f;
    private float idleBreathTimer = 0f;
    private bool isMouseMoving = false;
    private float mouseMoveTimer = 0f;
    private float afterImageTimer;
    private bool isUsingTransitionSprites = false;
    private Coroutine dashAfterimageCoroutine;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        sr = GetComponent<SpriteRenderer>();
        originalScale = transform.localScale;
        
        if (normalSprite == null)
        {
            normalSprite = sr.sprite;
        }
    }

    void Update()
    {
        UpdateMouseMovementDetection();
        
        if (movement.GetMoveInput().magnitude > 0.1f)
        {
            bounceTimer += Time.deltaTime * bounceFrequency;
        }

        HandleIdleBreathing();
        HandleAfterImageTimer();
    }

    void FixedUpdate()
    {
        HandleSquashAndStretch();
    }

    private void UpdateMouseMovementDetection()
    {
        bool mouseMovingNow = Mathf.Abs(Input.GetAxis("Mouse X")) > 0.1f || Mathf.Abs(Input.GetAxis("Mouse Y")) > 0.1f;
        
        if (mouseMovingNow)
        {
            isMouseMoving = true;
            mouseMoveTimer = 0f;
        }
        else
        {
            mouseMoveTimer += Time.deltaTime;
            if (mouseMoveTimer > mouseMoveThreshold)
            {
                isMouseMoving = false;
            }
        }
    }

    private void HandleSquashAndStretch()
    {
        if (!movement.IsDashing() && !movement.IsChargingDash())
        {
            if (movement.GetMoveInput().magnitude > 0.1f)
            {
                float bounceOffset = Mathf.Sin(bounceTimer) * bounceAmplitude;
                float horizontalSquash = moveSquashAmount - Mathf.Abs(movement.GetMoveInput().x) * 0.2f;
                
                Vector3 targetScale = new Vector3(
                    originalScale.x * horizontalSquash, 
                    originalScale.y * (moveStretchAmount + bounceOffset), 
                    originalScale.z
                );
                
                transform.localScale = Vector3.Lerp(transform.localScale, targetScale, scaleLerpSpeed * Time.deltaTime);
                
                if (isUsingTransitionSprites)
                {
                    sr.sprite = normalSprite;
                    isUsingTransitionSprites = false;
                }
            }
            else
            {
                transform.localScale = Vector3.Lerp(transform.localScale, originalScale, scaleLerpSpeed * Time.deltaTime);
                bounceTimer = 0f;
                
                if (isUsingTransitionSprites)
                {
                    sr.sprite = normalSprite;
                    isUsingTransitionSprites = false;
                }
            }
        }
        else if (movement.IsChargingDash())
        {
            HandleChargeVisuals();
        }
        else if (movement.IsDashing())
        {
            transform.localScale = new Vector3(originalScale.x * dashSquash, originalScale.y * dashStretch, originalScale.z);
        }
    }

    private void HandleChargeVisuals()
    {
        float chargePercent = movement.GetDashChargePercent();
        sr.color = chargeColorGradient.Evaluate(chargePercent);
        
        float targetXScale = Mathf.Lerp(originalScale.x, originalScale.x * chargeSquashX, chargePercent);
        float targetYScale = Mathf.Lerp(originalScale.y, originalScale.y * chargeStretchY, chargePercent);
        
        transform.localScale = new Vector3(targetXScale, targetYScale, originalScale.z);
        
        if (chargeSprite != null && !isUsingTransitionSprites)
        {
            sr.sprite = chargeSprite;
            isUsingTransitionSprites = true;
        }
        
        if (chargeParticles != null && !chargeParticles.isPlaying)
        {
            chargeParticles.Play();
        }
        
        if (chargeParticles != null && chargePercent >= 1f)
        {
            var main = chargeParticles.main;
            main.startColor = Color.red;
        }
    }

    private void HandleIdleBreathing()
    {
        if (movement.GetMoveInput().magnitude <= 0.1f && 
            !movement.IsDashing() && 
            !movement.IsChargingDash() && 
            !isMouseMoving)
        {
            idleBreathTimer += Time.deltaTime;
            float breathEffect = Mathf.Sin(idleBreathTimer * idleBreathFrequency) * idleBreathAmplitude;
            Vector3 breathScale = originalScale * (1 + breathEffect);
            transform.localScale = breathScale;
        }
        else
        {
            idleBreathTimer = 0f;
        }
    }

    private void HandleAfterImageTimer()
    {
        if (afterImageTimer > 0f)
        {
            afterImageTimer -= Time.deltaTime;
        }
    }

    public void CreateStaticAfterImage(Vector3 position, float duration, float alpha)
    {
        if (afterImagePrefab != null)
        {
            GameObject afterImage = Instantiate(afterImagePrefab, position, transform.rotation);
            RemovePhysicsComponents(afterImage);
            
            SpriteRenderer afterImageSR = afterImage.GetComponent<SpriteRenderer>();
            
            if (afterImageSR != null)
            {
                afterImageSR.sprite = sr.sprite;
                afterImageSR.flipX = sr.flipX;
                
                Color afterImageColor = afterImageSR.color;
                afterImageColor.a = alpha;
                afterImageSR.color = afterImageColor;
            }

            Destroy(afterImage, duration);
        }
    }

    private void RemovePhysicsComponents(GameObject obj)
    {
        Rigidbody2D rb2d = obj.GetComponent<Rigidbody2D>();
        if (rb2d != null) Destroy(rb2d);
        
        Collider2D collider2d = obj.GetComponent<Collider2D>();
        if (collider2d != null) Destroy(collider2d);
        
        MonoBehaviour[] scripts = obj.GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in scripts)
        {
            if (script != null && script.GetType() != typeof(SpriteRenderer))
            {
                Destroy(script);
            }
        }
    }

    public void StartDashAfterimages()
    {
        if (dashAfterimageCoroutine != null)
            StopCoroutine(dashAfterimageCoroutine);
        dashAfterimageCoroutine = StartCoroutine(CreateDashAfterimageBurst());
    }

    private IEnumerator CreateDashAfterimageBurst()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < dashAfterimageDuration && movement.IsDashing())
        {
            CreateStaticAfterImage(transform.position, dashAfterimageDuration, 0.3f);
            elapsedTime += dashAfterimageInterval;
            yield return new WaitForSeconds(dashAfterimageInterval);
        }
    }
}