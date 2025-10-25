using UnityEngine;

public class Grenade2D : MonoBehaviour
{
    [Header("Timer")]
    public float delay = 3f;
    private float countdown;
    private bool hasExploded = false;

    [Header("Explosion Settings")]
    public float explosionRadius = 5f;
    public float explosionForce = 50f;
    public float damage = 50f;

    [Header("Effects")]
    public GameObject explosionEffectPrefab; 

    [Header("Physics")]
    public LayerMask explosionLayers; 

    void Start()
    {
        countdown = delay;
    }

    void Update()
    {
        countdown -= Time.deltaTime;

        if (countdown <= 0f && !hasExploded)
        {
            Explode();
            hasExploded = true;
        }
    }

    void Explode()
    {
       
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, transform.rotation);
        }

       
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, explosionLayers);

        
        foreach (Collider2D nearbyObject in colliders)
        {
           
            Rigidbody2D rb = nearbyObject.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
          
                Vector2 direction = (nearbyObject.transform.position - transform.position).normalized;
                rb.AddForce(direction * explosionForce, ForceMode2D.Impulse);
            }

           
            EnemyHealth2D enemy = nearbyObject.GetComponent<EnemyHealth2D>();
            if (enemy != null)
            {
                enemy.TakeDamage(damage);
            }
        }

     
        Destroy(gameObject);
    }

   
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
