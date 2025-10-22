// BlinkAbility.cs
using UnityEngine;
using System.Collections;

public class BlinkAbility : MonoBehaviour
{
    [Header("Blink Settings")]
    public float maxBlinkDistance = 8f;
    public float blinkCooldown = 2f;
    public GameObject blinkEffectPrefab;
    public float blinkAfterimageDuration = 0.3f;
    
    [Header("Input Settings")]
    public KeyCode blinkKey = KeyCode.LeftShift;
    public string blinkButton = "Fire2";
    public string controllerBlinkButton = "L1"; // PS4 L1 or Xbox LB
    
    [Header("Visual Settings")]
    public GameObject blinkAfterimagePrefab;
    public int blinkAfterimageCount = 5;
    public float blinkAfterimageInterval = 0.05f;

    private PlayerMovement movement;
    private SpriteRenderer spriteRenderer;
    private Camera mainCamera;
    private float blinkCooldownTimer;
    private bool canBlink = true;

    void Start()
    {
        movement = GetComponent<PlayerMovement>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        mainCamera = Camera.main;
        
        if (blinkAfterimagePrefab == null)
        {
            blinkAfterimagePrefab = CreateDefaultAfterimagePrefab();
        }
    }

    void Update()
    {
        UpdateCooldown();
        HandleBlinkInput();
    }

    private void UpdateCooldown()
    {
        if (blinkCooldownTimer > 0f)
        {
            blinkCooldownTimer -= Time.deltaTime;
            if (blinkCooldownTimer <= 0f)
            {
                canBlink = true;
                blinkCooldownTimer = 0f;
            }
        }
    }

    private void HandleBlinkInput()
    {
        bool blinkInput = false;
        
        // Keyboard/Mouse input
        if (!MousePointerController.Instance.UsingController)
        {
            blinkInput = Input.GetKeyDown(blinkKey) || Input.GetButtonDown(blinkButton);
        }
        // Controller input
        else
        {
            blinkInput = Input.GetButtonDown(controllerBlinkButton);
        }

        if (canBlink && blinkInput)
        {
            PerformBlink();
        }
    }

    private void PerformBlink()
    {
        Vector3 blinkTarget = CalculateBlinkPosition();
        
        CreateBlinkEffects(transform.position);
        StartCoroutine(CreateBlinkAfterimages(transform.position, blinkTarget));
        transform.position = blinkTarget;
        CreateBlinkEffects(transform.position);
        
        canBlink = false;
        blinkCooldownTimer = blinkCooldown;
    }

    private Vector3 CalculateBlinkPosition()
    {
        Vector3 targetPosition;
        
        if (MousePointerController.Instance.UsingController)
        {
            // Use pointer position for controller
            targetPosition = MousePointerController.Instance.PointerWorldPosition;
        }
        else
        {
            // Use mouse position for keyboard/mouse
            Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = transform.position.z;
            targetPosition = mouseWorldPos;
        }
        
        Vector3 direction = targetPosition - transform.position;
        float distance = direction.magnitude;
        
        if (distance > maxBlinkDistance)
        {
            direction = direction.normalized * maxBlinkDistance;
        }
        
        return transform.position + direction;
    }

    private void CreateBlinkEffects(Vector3 position)
    {
        if (blinkEffectPrefab != null)
        {
            Instantiate(blinkEffectPrefab, position, Quaternion.identity);
        }
    }

    private IEnumerator CreateBlinkAfterimages(Vector3 startPos, Vector3 endPos)
    {
        for (int i = 0; i < blinkAfterimageCount; i++)
        {
            float t = (float)i / (blinkAfterimageCount - 1);
            Vector3 afterimagePos = Vector3.Lerp(startPos, endPos, t);
            CreateSingleAfterimage(afterimagePos);
            yield return new WaitForSeconds(blinkAfterimageInterval);
        }
    }

    private void CreateSingleAfterimage(Vector3 position)
    {
        if (blinkAfterimagePrefab != null)
        {
            GameObject afterimage = Instantiate(blinkAfterimagePrefab, position, transform.rotation);
            SetupAfterimage(afterimage);
        }
    }

    private void SetupAfterimage(GameObject afterimage)
    {
        RemovePhysicsComponents(afterimage);
        
        SpriteRenderer afterimageSR = afterimage.GetComponent<SpriteRenderer>();
        if (afterimageSR != null && spriteRenderer != null)
        {
            afterimageSR.sprite = spriteRenderer.sprite;
            afterimageSR.flipX = spriteRenderer.flipX;
            
            Color color = afterimageSR.color;
            color.a = 0.5f;
            afterimageSR.color = color;
        }
        
        AfterimageFade fade = afterimage.GetComponent<AfterimageFade>();
        if (fade == null)
        {
            fade = afterimage.AddComponent<AfterimageFade>();
        }
        fade.fadeDuration = blinkAfterimageDuration;
        
        Destroy(afterimage, blinkAfterimageDuration);
    }

    private void RemovePhysicsComponents(GameObject obj)
    {
        Rigidbody2D rb2d = obj.GetComponent<Rigidbody2D>();
        if (rb2d != null) Destroy(rb2d);
        
        Collider2D collider = obj.GetComponent<Collider2D>();
        if (collider != null) Destroy(collider);
        
        MonoBehaviour[] components = obj.GetComponents<MonoBehaviour>();
        foreach (var component in components)
        {
            if (component != null && component != this && !(component is AfterimageFade))
            {
                Destroy(component);
            }
        }
    }

    private GameObject CreateDefaultAfterimagePrefab()
    {
        GameObject prefab = new GameObject("BlinkAfterimage");
        SpriteRenderer sr = prefab.AddComponent<SpriteRenderer>();
        prefab.AddComponent<AfterimageFade>();
        sr.sortingOrder = -1;
        return prefab;
    }

    void OnGUI()
    {
        if (!canBlink)
        {
            GUI.Label(new Rect(10, 40, 200, 20), $"Blink Cooldown: {blinkCooldownTimer:F1}s");
        }
    }

    public bool CanBlink() => canBlink;
    public float GetCooldownPercent() => blinkCooldownTimer / blinkCooldown;
    public float GetCooldownRemaining() => blinkCooldownTimer;
}

public class AfterimageFade : MonoBehaviour
{
    public float fadeDuration = 0.3f;
    private SpriteRenderer spriteRenderer;
    private float fadeTimer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        fadeTimer = fadeDuration;
        
        if (spriteRenderer == null)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        fadeTimer -= Time.deltaTime;
        
        if (fadeTimer <= 0f)
        {
            Destroy(gameObject);
            return;
        }
        
        float alpha = fadeTimer / fadeDuration;
        Color color = spriteRenderer.color;
        color.a = alpha * 0.5f;
        spriteRenderer.color = color;
    }
}