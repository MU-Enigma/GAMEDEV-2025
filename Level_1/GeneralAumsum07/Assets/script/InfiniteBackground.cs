using UnityEngine;

public class InfiniteBackground : MonoBehaviour
{
    [Header("Player Reference")]
    public Transform player;  // Drag your player here

    [Header("Scroll Settings")]
    public float scrollMultiplier = 0.5f; // How fast the background moves relative to player

    private Renderer rend;
    private Vector3 lastPlayerPos;

    void Start()
    {
        rend = GetComponent<Renderer>();
        if (player == null)
        {
            Debug.LogError("Player not assigned in InfiniteBackground!");
            enabled = false;
            return;
        }
        lastPlayerPos = player.position;
    }

    void Update()
    {
        // Calculate player movement since last frame
        Vector3 delta = player.position - lastPlayerPos;

        // Apply offset to the material
        float offsetX = rend.material.mainTextureOffset.x + delta.z * scrollMultiplier;
        float offsetY = rend.material.mainTextureOffset.y + delta.z * scrollMultiplier; // z = vertical in top-down

        rend.material.mainTextureOffset = new Vector2(offsetX, offsetY);

        lastPlayerPos = player.position;
    }
}