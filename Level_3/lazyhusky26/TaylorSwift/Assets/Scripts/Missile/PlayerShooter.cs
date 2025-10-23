using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    public GameObject missilePrefab;
    public Transform firePoint;
    public float fireCooldown = 0.5f;

    private float lastFireTime;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > lastFireTime + fireCooldown)
        {
            FireMissile();
            lastFireTime = Time.time;
        }
    }

    void FireMissile()
    {
        Instantiate(missilePrefab, firePoint.position, firePoint.rotation);
    }
}
