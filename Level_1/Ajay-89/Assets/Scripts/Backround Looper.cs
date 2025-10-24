using UnityEngine;

public class BackgroundLooper : MonoBehaviour
{
    [Header("Background Settings")]
    [Tooltip("The background sprite prefab to tile.")]
    public GameObject backgroundTilePrefab;
    [Tooltip("The player transform to center the grid around.")]
    public Transform player;
    
    [Header("Grid Settings")]
    [Tooltip("Size of each background tile (should match your sprite size).")]
    public Vector2 tileSize = new Vector2(10f, 10f);
    [Tooltip("How many tiles in each direction from center (4 = 9x9 grid total).")]
    public int gridRadius = 4;
    
    [Header("Anti-Jitter Settings")]
    [Tooltip("Buffer distance before repositioning to prevent frequent updates.")]
    public float repositionBuffer = 0.1f;
    [Tooltip("Snap positions to prevent floating point precision issues.")]
    public bool snapToPixels = true;
    
    // Private variables
    private GameObject[,] backgroundTiles;
    private Vector2 lastPlayerChunk;
    private int gridSize;
    private bool isRepositioning = false;
    
    void Start()
    {
        if (player == null)
        {
            Debug.LogError("BackgroundLooper: Player reference not set!");
            return;
        }
        
        if (backgroundTilePrefab == null)
        {
            Debug.LogError("BackgroundLooper: Background tile prefab not set!");
            return;
        }
        
        // Calculate total grid size (gridRadius of 4 = 9x9 grid)
        gridSize = (gridRadius * 2) + 1;
        
        InitializeGrid();
        
        // Set initial player chunk position
        lastPlayerChunk = GetPlayerChunkPosition();
        PositionGrid();
        
        Debug.Log($"BackgroundLooper: Created {gridSize}x{gridSize} = {gridSize * gridSize} background tiles");
    }
    
    // Use LateUpdate to ensure this happens after player movement and camera updates
    void LateUpdate()
    {
        if (player == null || isRepositioning) return;
        
        Vector2 currentPlayerChunk = GetPlayerChunkPosition();
        
        // Check if player has moved far enough to warrant repositioning
        if (ShouldRepositionGrid(currentPlayerChunk))
        {
            StartCoroutine(RepositionGridSmooth());
            lastPlayerChunk = currentPlayerChunk;
        }
    }
    
    /// <summary>
    /// Check if the grid should be repositioned with buffer to prevent jittering
    /// </summary>
    bool ShouldRepositionGrid(Vector2 currentChunk)
    {
        Vector2 chunkDelta = currentChunk - lastPlayerChunk;
        
        // Only reposition if moved a full chunk plus buffer
        return Mathf.Abs(chunkDelta.x) >= 1f || Mathf.Abs(chunkDelta.y) >= 1f;
    }
    
    /// <summary>
    /// Initialize the 9x9 grid of background tiles
    /// </summary>
    void InitializeGrid()
    {
        backgroundTiles = new GameObject[gridSize, gridSize];
        
        // Create exactly gridSize x gridSize tiles
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                GameObject tile = Instantiate(backgroundTilePrefab);
                tile.name = $"BackgroundTile_{x}_{y}";
                tile.transform.SetParent(transform); // Organize under this GameObject
                
                backgroundTiles[x, y] = tile;
            }
        }
    }
    
    /// <summary>
    /// Get the player's current chunk position with precise calculation
    /// </summary>
    Vector2 GetPlayerChunkPosition()
    {
        Vector3 playerPos = player.position;
        
        // Use more precise chunk calculation to avoid floating point issues
        int chunkX = Mathf.RoundToInt((playerPos.x - tileSize.x * 0.5f) / tileSize.x);
        int chunkY = Mathf.RoundToInt((playerPos.y - tileSize.y * 0.5f) / tileSize.y);
        
        return new Vector2(chunkX, chunkY);
    }
    
    /// <summary>
    /// Position the entire grid around the player's current chunk
    /// </summary>
    void PositionGrid()
    {
        Vector2 playerChunk = GetPlayerChunkPosition();
        
        // Position each tile in the grid
        for (int x = 0; x < gridSize; x++)
        {
            for (int y = 0; y < gridSize; y++)
            {
                // Calculate the world chunk position for this grid tile
                int worldChunkX = (int)playerChunk.x + (x - gridRadius);
                int worldChunkY = (int)playerChunk.y + (y - gridRadius);
                
                // Convert chunk position to world position with precise positioning
                Vector3 worldPosition = new Vector3(
                    worldChunkX * tileSize.x,
                    worldChunkY * tileSize.y,
                    0f
                );
                
                // Snap to pixels if enabled to prevent sub-pixel positioning
                if (snapToPixels)
                {
                    worldPosition.x = Mathf.Round(worldPosition.x);
                    worldPosition.y = Mathf.Round(worldPosition.y);
                }
                
                // Position the tile
                backgroundTiles[x, y].transform.position = worldPosition;
            }
        }
    }

    /// <summary>
    /// Smooth repositioning to prevent visual snapping
    /// </summary>
    System.Collections.IEnumerator RepositionGridSmooth()
    {
        isRepositioning = true;

        // Wait one frame to ensure all movement is complete
        yield return null;

        PositionGrid();

        // Wait another frame before allowing next repositioning
        yield return null;
        isRepositioning = false;
    }
}