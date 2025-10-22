using UnityEngine;

public class BackgroundTileGridSpawner : MonoBehaviour
{
    public GameObject tile1Prefab; // Assign prefab with just sprite
    public GameObject tile2Prefab; // Assign prefab with sprite + colliders

    public int gridWidth = 9;
    public int gridHeight = 9;
    public float tileSpacing = 1.1f; // Adjust to your sprite size

    void Start()
    {
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                // 75% chance for Tile 1, 25% for Tile 2
                float rand = Random.value;
                GameObject prefabToUse = rand < 0.75f ? tile1Prefab : tile2Prefab;

                // Instantiate at correct grid position
                Vector3 pos = new Vector3(x * tileSpacing, y * tileSpacing, 0);
                Instantiate(prefabToUse, pos, Quaternion.identity, transform);
            }
        }
    }
}
