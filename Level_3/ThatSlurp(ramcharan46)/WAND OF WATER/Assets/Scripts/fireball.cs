using UnityEngine;

public class Fireball : MonoBehaviour
{
    public float speed = 5f;
    public float rotateSpeed = 200f;
    public float lifeTime = 5f;

    private Transform target;

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (target == null)
        {
            FindNearestEnemy();
            if (target == null) return;
        }

        Vector2 direction = (Vector2)target.position - (Vector2)transform.position;
        direction.Normalize();

        transform.position += (Vector3)(direction * speed * Time.deltaTime);

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);
    }

    void FindNearestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        float minDist = Mathf.Infinity;
        GameObject nearest = null;

        foreach (GameObject e in enemies)
        {
            float dist = Vector2.Distance(transform.position, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = e;
            }
        }

        if (nearest != null)
            target = nearest.transform;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            Destroy(gameObject);
        }
    }
}
