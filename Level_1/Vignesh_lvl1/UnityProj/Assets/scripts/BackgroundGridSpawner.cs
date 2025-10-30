using UnityEngine;

public class BackgroundGridSpawner : MonoBehaviour
{
    public GameObject tilePrefab; // Assign your BackgroundTile prefab here
    public int gridWidth = 9;
    public int gridHeight = 9;
    public float cellSize = 1f; // Distance between tiles

    void Start()
    {
        for (int y = 0; y < gridHeight; y++)
        {
            for (int x = 0; x < gridWidth; x++)
            {
                Vector3 position = new Vector3(x * cellSize, y * cellSize, 0);
                Instantiate(tilePrefab, position, Quaternion.identity, transform);
            }
        }
    }
}
