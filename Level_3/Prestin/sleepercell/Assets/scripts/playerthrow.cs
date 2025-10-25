using UnityEngine;

public class PlayerThrowGrenade2D : MonoBehaviour
{
    [Header("Grenade Settings")]
    public float throwForce = 15f;
    public KeyCode throwKey = KeyCode.G;

    [Tooltip("Controls the height of the arc. 0 = straight shot, 0.5 = small arc, 1.0 = high arc.")]
    [Range(0f, 1.5f)]
    public float lobAngle = 0.5f;

    [Header("References")]
    public GameObject grenadePrefab; 
    public Transform grenadeSpawnPoint; 
    
    private Camera mainCamera; 

    void Start()
    {
        
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("PlayerThrowGrenade2D: No 'MainCamera' tagged camera found in scene!");
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(throwKey))
        {
            ThrowGrenade();
        }
    }

    void ThrowGrenade()
    {
        if (grenadePrefab == null || grenadeSpawnPoint == null || mainCamera == null)
        {
            Debug.LogError("PlayerThrowGrenade2D script is missing references or camera!");
            return;
        }

        
        GameObject grenade = Instantiate(grenadePrefab, grenadeSpawnPoint.position, grenadeSpawnPoint.rotation);
        
      
        Rigidbody2D rb = grenade.GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("Grenade prefab is missing a Rigidbody2D component!");
            Destroy(grenade);
            return;
        }

       
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Vector2 throwDirection = (mouseWorldPos - grenadeSpawnPoint.position).normalized;

       
        throwDirection.y += lobAngle;
        throwDirection = throwDirection.normalized; 

     
        rb.AddForce(throwDirection * throwForce, ForceMode2D.Impulse);
    }
}

