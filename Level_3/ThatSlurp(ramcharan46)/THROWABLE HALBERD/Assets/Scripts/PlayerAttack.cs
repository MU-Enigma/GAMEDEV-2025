using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform shootPoint; 
    public float cooldownTime = 0.5f; 
    public AudioClip fireSound;

    private Collider2D playerCol;
    private float lastShotTime = -Mathf.Infinity;
    private AudioSource audioSource;

    public GameObject fireballPrefab;
    public float spawnRadius = 1.5f;
    public int fireballCount = 10;

    public GameObject helicalFireballPrefab;
    public float startRadius = 1.5f;
    void Start(){
        playerCol = GetComponent<Collider2D>();

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        audioSource.playOnAwake = false;
        audioSource.loop = false;
    }

    void Update(){
        if ((Input.GetMouseButtonDown(1) || (Gamepad.current != null && Gamepad.current.rightTrigger.ReadValue() > 0.5f)) && Time.time >= lastShotTime + cooldownTime)
        {
            Vector2 worldMouse = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Shoot(worldMouse);
            lastShotTime = Time.time;
        }

        if (Input.GetKeyDown(KeyCode.F)) // Press F to launch fireballs
        {
            SpawnFireballs();
        }
        if (Input.GetKeyDown(KeyCode.G)) // Press G to spawn orbiting fireballs
        {
            SpawnHelicalFireballs();
        }
    }

    void Shoot(Vector2 target)
    {
        Vector2 dir = (target - (Vector2)shootPoint.position).normalized;
        Vector2 spawnPos = (Vector2)shootPoint.position + dir * 0.5f;

        GameObject proj = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);

        Collider2D projCol = proj.GetComponent<Collider2D>();
        if (projCol != null && playerCol != null)
        {
            Physics2D.IgnoreCollision(projCol, playerCol);
        }

        if (fireSound != null && audioSource != null)
        {
            audioSource.clip = fireSound;
            audioSource.Play();
        }

        proj.GetComponent<Projectile>().Init(target);
    }

    void SpawnFireballs()
    {
        for (int i = 0; i < fireballCount; i++)
        {
            float angle = i * Mathf.PI * 2 / fireballCount;
            Vector3 spawnPos = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * spawnRadius;
            Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
        }
    }
    
    void SpawnHelicalFireballs()
    {
        for (int i = 0; i < fireballCount; i++)
        {
            float angle = i * Mathf.PI * 2 / fireballCount;

            // Spawn around the player in a perfect circle
            Vector3 spawnPos = transform.position + new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * startRadius;

            GameObject fb = Instantiate(helicalFireballPrefab, spawnPos, Quaternion.identity);
            HelicalOrbitFireball orb = fb.GetComponent<HelicalOrbitFireball>();

            if (orb != null)
            {
                orb.startRadius = startRadius;
                orb.Initialize(angle * Mathf.Rad2Deg);
            }
        }
    }
}
