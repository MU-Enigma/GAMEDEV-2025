using UnityEngine;

public class RinneganPull : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip rinneganActivateSound;
    public AudioClip swapSound; // Still used for the pull sound
    private AudioSource audioSource;

    [Header("Cursor Textures")]
    public Texture2D rinneganCursor;
    public Texture2D normalCursor;
    public Vector2 cursorHotspot = Vector2.zero;

    [Header("Pull Settings")]
    public float pullSpeed = 10f;        // Speed at which the enemy moves toward the player
    public float stopDistance = 1.5f;    // Distance in front of the player to stop

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
            TryPullEnemy();
        }
    }

    void ActivateRinnegan()
    {
        rinneganActive = true;
        Cursor.SetCursor(rinneganCursor, cursorHotspot, CursorMode.Auto);
        if (rinneganActivateSound && audioSource)
            audioSource.PlayOneShot(rinneganActivateSound);
    }

    void TryPullEnemy()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            if (swapSound && audioSource)
                audioSource.PlayOneShot(swapSound);

            // Start pulling the enemy toward the player
            StartCoroutine(PullEnemy(hit.collider.transform));
        }

        DeactivateRinnegan();
    }

    System.Collections.IEnumerator PullEnemy(Transform enemy)
    {
        Vector2 directionToPlayer = (enemy.position - transform.position).normalized;
        Vector2 targetPosition = (Vector2)transform.position + directionToPlayer * stopDistance;

        while (Vector2.Distance(enemy.position, targetPosition) > 0.05f)
        {
            enemy.position = Vector2.MoveTowards(enemy.position, targetPosition, pullSpeed * Time.deltaTime);
            yield return null;
        }
    }

    void DeactivateRinnegan()
    {
        rinneganActive = false;
        Cursor.SetCursor(normalCursor, cursorHotspot, CursorMode.Auto);
    }
}
