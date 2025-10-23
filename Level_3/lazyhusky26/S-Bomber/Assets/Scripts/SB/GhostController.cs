using UnityEngine;

public class GhostController : MonoBehaviour
{
    public float speed = 5f;
    public GameObject explosionVFX;
    public AudioClip runSoundClip;    // Looping running sound
    public AudioClip explosionSound;  // Explosion sound

    private Transform enemy;
    private AudioSource audioSource;
    private bool isMoving = false;

    void Start()
    {
        enemy = GameObject.FindGameObjectWithTag("Enemy")?.transform;

        if (enemy == null)
        {
            Debug.LogError("Enemy not found! Make sure enemy GameObject has the tag 'Enemy'.");
            Destroy(gameObject);
            return;
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            Debug.LogError("No AudioSource component found on ghost!");
            Destroy(gameObject);
            return;
        }

        if (runSoundClip != null)
        {
            audioSource.clip = runSoundClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        isMoving = true;
    }

    void Update()
    {
        if (isMoving && enemy != null)
        {
            Vector2 currentPos = transform.position;
            Vector2 targetPos = enemy.position;
            float step = speed * Time.deltaTime;
            transform.position = Vector2.MoveTowards(currentPos, targetPos, step);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            if (audioSource != null)
            {
                audioSource.Stop(); // Stop running sound
            }

            if (explosionVFX != null)
            {
                Instantiate(explosionVFX, transform.position, Quaternion.identity);
            }

            if (explosionSound != null)
            {
                // Create a temporary game object to play explosion sound so it won't get cut off
                GameObject tempGO = new GameObject("TempExplosionSound");
                tempGO.transform.position = transform.position;
                AudioSource tempAudioSource = tempGO.AddComponent<AudioSource>();
                tempAudioSource.clip = explosionSound;
                tempAudioSource.Play();

                // Destroy this temp object after the clip finishes playing
                Destroy(tempGO, explosionSound.length);
            }

            Destroy(collision.gameObject);
            Destroy(gameObject);
        }
    }
}
