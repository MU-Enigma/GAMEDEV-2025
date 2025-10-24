using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Sprite normalSprite;
    public Sprite transformedSprite;
    public GameObject transformVFX;

    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = normalSprite;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("PowerUp"))
        {
            // Change sprite
            spriteRenderer.sprite = transformedSprite;

            // Play VFX
            if (transformVFX != null)
            {
                Instantiate(transformVFX, transform.position, Quaternion.identity);
            }

            // Remove the item
            Destroy(collision.gameObject);
        }
    }
}
