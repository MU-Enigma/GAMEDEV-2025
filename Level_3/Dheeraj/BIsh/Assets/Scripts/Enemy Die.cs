using UnityEngine;

public class EnemyDie : MonoBehaviour
{

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Axe"))
        {
            Destroy(gameObject);
        }
    }
}
