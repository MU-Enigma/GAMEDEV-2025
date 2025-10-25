using UnityEngine;

public class RinneganSwap : MonoBehaviour
{
    [Header("Audio")]
    public AudioClip rinneganActivateSound;
    public AudioClip swapSound;
    private AudioSource audioSource;

    [Header("Cursor Textures")]
    public Texture2D rinneganCursor;
    public Texture2D normalCursor;
    public Vector2 cursorHotspot = Vector2.zero;

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
            TrySwapWithEnemy();
        }
    }

    void ActivateRinnegan()
    {
        rinneganActive = true;
        Cursor.SetCursor(rinneganCursor, cursorHotspot, CursorMode.Auto);
        if (rinneganActivateSound && audioSource)
            audioSource.PlayOneShot(rinneganActivateSound);
    }

    void TrySwapWithEnemy()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Enemy"))
        {
            Vector3 enemyPos = hit.collider.transform.position;
            Vector3 playerPos = transform.position;

            transform.position = enemyPos;
            hit.collider.transform.position = playerPos;

            if (swapSound && audioSource)
                audioSource.PlayOneShot(swapSound);

            DeactivateRinnegan();
        }
        else
        {
            DeactivateRinnegan();
        }
    }

    void DeactivateRinnegan()
    {
        rinneganActive = false;
        Cursor.SetCursor(normalCursor, cursorHotspot, CursorMode.Auto);
    }
}
