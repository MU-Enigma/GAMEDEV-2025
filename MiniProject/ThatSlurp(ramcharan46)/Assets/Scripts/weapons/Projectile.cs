using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float speed = 15f;
    public float maxDistance = 10f;
    public float lifetime = 1f;
    public int damage = 1;

    private Vector3 startPos;
    private Vector2 direction;
    private Animator anim;
    private bool hasBurst = false;

    public void Init(Vector2 target){
        direction = (target - (Vector2)transform.position).normalized;
        startPos = transform.position;
    }

    void Start(){
        anim = GetComponent<Animator>();
        Invoke(nameof(PlayBurst), lifetime);
    }

    void Update(){
        if (hasBurst) return;

        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        if (Vector3.Distance(startPos, transform.position) >= maxDistance){
            PlayBurst();
        }

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
    }

    void OnTriggerEnter2D(Collider2D other){
        if (hasBurst) return;

        if (other.CompareTag("Player")) return;

        PlayBurst();
    }

    void PlayBurst(){
        if (hasBurst) return;
        hasBurst = true;

        speed = 0f;

        if (anim != null)
        {
            anim.SetTrigger("Burst");
        }

        Destroy(gameObject, 0.3f);
    }
}
