using UnityEngine;
public class Boomerang : MonoBehaviour
{
    [Header("References")]
    public Transform targetEnemy;
    public GameObject sword;
    
    [Header("Settings")]
    public float speed = 15f;
    
    [Header("Screen Shake")]
    [SerializeField] private float shakeIntensity = 0.15f;
    
    private Transform parent;
    private Vector3 startPosition;
    private Quaternion startRotation;
    private bool returning = false;
    private bool isThrown = false;
    private Rigidbody2D rb;
    private CameraShake cameraShake;
    
    private void Start()
    {
        parent = sword.transform.parent;
        startPosition = sword.transform.localPosition;
        startRotation = sword.transform.localRotation;
        rb = sword.GetComponent<Rigidbody2D>();
        
        cameraShake = Camera.main.GetComponent<CameraShake>();
        if (cameraShake == null)
        {
            cameraShake = Camera.main.gameObject.AddComponent<CameraShake>();
        }
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1) && !isThrown)
        {
            ThrowSword();
        }
        
        if (isThrown)
        {
            if (!returning && targetEnemy != null)
            {
                Vector2 directionToEnemy = (targetEnemy.position - sword.transform.position).normalized;
                rb.linearVelocity = directionToEnemy * speed;
            }
            else if (returning)
            {
                Vector2 directionToParent = (parent.position - sword.transform.position).normalized;
                rb.linearVelocity = directionToParent * speed;
                
                if (Vector2.Distance(sword.transform.position, parent.position) < 0.5f)
                {
                    ReturnToParent();
                }
            }
        }
    }
    
    private void ThrowSword()
    {
        if (targetEnemy == null) return;
        
        isThrown = true;      
        cameraShake.StartShake(shakeIntensity);      
        sword.transform.SetParent(null);
        
        Vector2 direction = (targetEnemy.position - sword.transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<EnemyDeath>() != null && !returning)
        {
            if (cameraShake != null)
            {
                cameraShake.AddImpulse(shakeIntensity * 2f);
            }
            returning = true;
            targetEnemy = null;
        }
    }
    
    private void ReturnToParent()
    {
        cameraShake.StopShake();
        sword.transform.SetParent(parent);
        sword.transform.localPosition = startPosition;
        sword.transform.localRotation = startRotation;
        rb.linearVelocity = Vector2.zero;
        returning = false;
        isThrown = false;
    }
}

// ===== CAMERA SHAKE =====
public class CameraShake : MonoBehaviour
{
    private Vector3 originalPos;
    private bool isShaking = false;
    private float currentIntensity = 0f;
    
    private void Start()
    {
        originalPos = transform.localPosition;
    }
    private void Update()
    {
        if (isShaking)
        {
            float offsetX = Random.Range(-1f, 1f) * currentIntensity;
            float offsetY = Random.Range(-1f, 1f) * currentIntensity;
            transform.localPosition = originalPos + new Vector3(offsetX, offsetY, 0);
            
            currentIntensity = Mathf.Lerp(currentIntensity, 0, Time.deltaTime * 3f);
        }
    }
    public void StartShake(float intensity)
    {
        isShaking = true;
        currentIntensity = intensity;
    }
    
    public void AddImpulse(float intensity)
    {
        currentIntensity += intensity;
    }
    
    public void StopShake()
    {
        isShaking = false;
        transform.localPosition = originalPos;
        currentIntensity = 0f;
    }
}