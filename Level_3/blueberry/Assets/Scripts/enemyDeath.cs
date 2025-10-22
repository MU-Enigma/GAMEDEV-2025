using UnityEngine;

public class EnemyDeath : MonoBehaviour
{
    public GameObject enemy;
    public GameObject bloodEffectPrefab;
    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Weapon"))
        {
            Instantiate(bloodEffectPrefab, enemy.transform.position, Quaternion.identity);
            Destroy(enemy);
        }
    }
}
