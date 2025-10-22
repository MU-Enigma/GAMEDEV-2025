using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("References")]
    public Transform firePoint; // empty child transform at muzzle
    public Bullet bulletPrefab; // link to Prefab (for pool)
    public BulletPool pool;     // link to BulletPool (or find at runtime)
    public AudioSource fireAudio;

    [Header("Gun stats")]
    public float fireRate = 6f; // rounds per second
    public int magazineSize = 12;
    public float reloadTime = 1.5f;
    public float bulletSpeedMultiplier = 1f;
    public float recoilAngle = 5f;
    public float recoilRecoverSpeed = 8f;

    int currentAmmo;
    float lastShotTime = -999f;
    bool reloading = false;
    float recoil = 0f;

    private SpriteRenderer spriteRenderer;


    void Start()
    {
        currentAmmo = magazineSize;
        if (pool == null && BulletPool.Instance != null) pool = BulletPool.Instance;
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        AimAtCursor();

        // Input: left mouse button or Fire1
        if (!reloading && (Input.GetButton("Fire1") || Input.GetMouseButton(0)))
        {
            TryFire();
        }

        // reload with R
        if (Input.GetKeyDown(KeyCode.R) && !reloading && currentAmmo < magazineSize)
        {
            StartCoroutine(ReloadCoroutine());
        }

        // Recoil recovery: lerp recoil towards 0
        if (recoil != 0f)
        {
            recoil = Mathf.MoveTowards(recoil, 0f, recoilRecoverSpeed * Time.deltaTime);
            transform.localEulerAngles = new Vector3(0, 0, transform.localEulerAngles.z + recoil);
        }
    }

    void AimAtCursor()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mouseWorld - transform.position;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 1. Rotate the gun correctly
        transform.rotation = Quaternion.Euler(0, 0, angle);

        // 2. Flip the gun sprite visually if pointing left
        // This keeps the gun aiming and bullets correct, just flips appearance
        if (spriteRenderer != null)
        {
            if (angle > 90 || angle < -90)
            {
                spriteRenderer.flipY = true;
            }
            else
            {
                spriteRenderer.flipY = false;
            }
        }
    }


    void TryFire()
    {
        if (Time.time - lastShotTime < 1f / fireRate) return;
        if (currentAmmo <= 0)
        {
            // Could play empty sound here
            return;
        }

        Fire();
    }

    void Fire()
    {
        lastShotTime = Time.time;
        currentAmmo--;

        // Get bullet from pool
        if (pool == null)
        {
            Debug.LogWarning("BulletPool missing on gun.");
            return;
        }

        var b = pool.GetBullet();
        b.transform.position = firePoint.position;
        b.transform.rotation = firePoint.rotation;
        // direction is right of firePoint
        Vector2 direction = firePoint.right;
        b.Initialize(direction, bulletSpeedMultiplier);

        // Play sound if provided
        if (fireAudio != null) fireAudio.Play();

        // Recoil: add a small random jitter
        float r = Random.Range(-recoilAngle, recoilAngle);
        recoil += r;
        transform.Rotate(0, 0, r); // quick visual kick

        // If you want muzzle flash, spawn here
    }

    System.Collections.IEnumerator ReloadCoroutine()
    {
        reloading = true;
        // play reload animation/sound here
        yield return new WaitForSeconds(reloadTime);
        currentAmmo = magazineSize;
        reloading = false;
    }

    // Optional getters for UI
    public int GetAmmo() => currentAmmo;
    public bool IsReloading() => reloading;
}
