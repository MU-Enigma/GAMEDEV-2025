using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public GameObject ghostPrefab;
    public Transform spawnPoint;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SpawnGhost();
        }
    }

    void SpawnGhost()
    {
        if (ghostPrefab != null && spawnPoint != null)
        {
            Instantiate(ghostPrefab, spawnPoint.position, Quaternion.identity);
        }
        else
        {
            Debug.LogError("Ghost prefab or spawn point missing!");
        }
    }
}
