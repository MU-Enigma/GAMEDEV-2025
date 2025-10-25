using UnityEngine;

public class RinneganPush : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip rinneganActivateSound;
    public AudioClip swapSound;
    private AudioSource audioSource;

    [Header("Cursor Textures")]
    public Texture2D rinneganCursor;
    public Texture2D normalCursor;
    public Vector2 cursorHotspot = Vector2.zero;

    [Header("Push Settings")]
    public float pushDistance = 5f;   // How far the enemy is flung
    public float pushSpeed = 15f;     // Speed of the push

    [Header("VFX")]
    public GameObject hitVFXPrefab;   // Visual effect to spawn when enemy hits a wall

    private bool rinneganActive = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        Cursor.SetCursor(normalCursor, cursorHotspot, CursorMode.Auto);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            ActivateRinnegan();
        }

        if (rinneganActive && Input.GetMouseButtonDown(0))
        {
            TryPushEnemy();
        }
    }

    void ActivateRinnegan()
    {
        rinneganActive = true;
        Cursor.SetCursor(rinneganCursor, cursorHotspot, CursorMode.Auto);
        if (rinneganActivateSound && audioSource)
            audioSource.PlayOneShot(rinneganActivateSound);
    }

    void TryPushEnemy()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            if (swapSound && audioSource)
                audioSource.PlayOneShot(swapSound);

            StartCoroutine(PushEnemy(hit.collider.transform));
        }

        DeactivateRinnegan();
    }

    System.Collections.IEnumerator PushEnemy(Transform enemy)
    {
        Vector2 direction = (enemy.position - transform.position).normalized;
        Vector2 startPosition = enemy.position;
        Vector2 targetPosition = startPosition + direction * pushDistance;

        Rigidbody2D rb = enemy.GetComponent<Rigidbody2D>();
        if (!rb) rb = enemy.gameObject.AddComponent<Rigidbody2D>();
        rb.isKinematic = true; // We'll move manually

        while (Vector2.Distance(enemy.position, targetPosition) > 0.05f)
        {
            Vector2 nextPos = Vector2.MoveTowards(enemy.position, targetPosition, pushSpeed * Time.deltaTime);

            // Check for wall collision using tag
            Collider2D[] colliders = Physics2D.OverlapCircleAll(nextPos, 0.1f);
            foreach (var col in colliders)
            {
                if (col.CompareTag("Wall"))
                {
                    // Spawn VFX at collision point
                    if (hitVFXPrefab)
                        Instantiate(hitVFXPrefab, nextPos, Quaternion.identity);

                    Destroy(enemy.gameObject);
                    yield break;
                }
            }

            enemy.position = nextPos;
            yield return null;
        }

        // If enemy didn't hit a wall, destroy after push distance
        Destroy(enemy.gameObject);
    }

    void DeactivateRinnegan()
    {
        rinneganActive = false;
        Cursor.SetCursor(normalCursor, cursorHotspot, CursorMode.Auto);
    }
}
