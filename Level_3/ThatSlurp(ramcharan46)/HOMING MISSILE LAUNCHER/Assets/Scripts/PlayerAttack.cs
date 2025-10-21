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

}
