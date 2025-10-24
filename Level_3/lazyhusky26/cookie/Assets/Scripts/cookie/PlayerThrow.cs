// PlayerThrow.cs
using UnityEngine;

public class PlayerThrow : MonoBehaviour
{
    [Header("Projectile")]
    public GameObject snowballPrefab;
    public Transform throwOrigin; // where snowballs spawn
    public float throwSpeed = 12f;
    public float throwCooldown = 0.25f;

    float lastThrowTime = -999f;

    void Update()
    {
        // Example: left mouse button or Fire1
        if ((Input.GetButtonDown("Fire1") || Input.GetMouseButtonDown(0)) && Time.time >= lastThrowTime + throwCooldown)
        {
            Vector2 targetPos = GetAimPosition();
            ThrowAt(targetPos);
            lastThrowTime = Time.time;
        }
    }

    Vector2 GetAimPosition()
    {
        // Aim to mouse position in world space
        Vector3 mp = Input.mousePosition;
        mp.z = 0f;
        return Camera.main.ScreenToWorldPoint(mp);
    }

    void ThrowAt(Vector2 targetWorldPos)
    {
        if (snowballPrefab == null || throwOrigin == null) return;

        Vector2 spawnPos = throwOrigin.position;
        Vector2 dir = (targetWorldPos - spawnPos).normalized;

        GameObject go = Instantiate(snowballPrefab, spawnPos, Quaternion.identity);
        Rigidbody2D rb = go.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = dir * throwSpeed;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            rb.rotation = angle;
        }
    }
}
