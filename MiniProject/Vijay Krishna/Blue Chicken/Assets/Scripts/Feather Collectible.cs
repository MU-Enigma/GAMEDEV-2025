using UnityEngine;

public class FeatherCollectible : MonoBehaviour
{
    [Header("Bobbing Settings")]
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {
        float yOffset = Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = startPosition + new Vector3(0, yOffset, 0);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            player.AddFeathers(1);
            Destroy(gameObject);
        }
    }
}
