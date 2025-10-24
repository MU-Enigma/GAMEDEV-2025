using UnityEngine;

public class Health : MonoBehaviour
{
    public int maxHealth = 3;
    int current;

    void Start() { current = maxHealth; }

    public void TakeDamage(int d)
    {
        current -= d;
        if (current <= 0) Die();
    }

    void Die()
    {
        // placeholder: destroy or play death fx
        Destroy(gameObject);
    }
}
