using UnityEngine;

public class SwordHitDetection : MonoBehaviour
{
    private SordCont sordCont;
    
    [Header("Hit Effects")]
    public GameObject hitEffectPrefab;

    void Start()
    {
        sordCont = GetComponent<SordCont>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Only register hits when sword is swinging
        if (sordCont != null && sordCont.IsSwinging())
        {
            if (other.CompareTag("Enemy"))
            {
                Debug.Log("HIT ENEMY: " + other.name);
                HitEnemy(other.gameObject);
            }
        }
    }

    void HitEnemy(GameObject enemy)
    {
     
        // Spawn hit effect
        if (hitEffectPrefab != null)
        {
            Instantiate(hitEffectPrefab, enemy.transform.position, Quaternion.identity);
        }
        
        // Screen shake
        if (CameraShake.Instance != null)
        {
            CameraShake.Instance.Shake(0.2f, 0.1f);
        }
        
        // Destroy the enemy
        Destroy(enemy);
        Debug.Log("Enemy destroyed!");
    }
    
}