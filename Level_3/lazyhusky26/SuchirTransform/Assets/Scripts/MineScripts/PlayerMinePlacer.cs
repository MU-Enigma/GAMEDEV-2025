// PlayerMinePlacer.cs
using System.Collections.Generic;
using UnityEngine;

public class PlayerMinePlacer : MonoBehaviour
{
    public GameObject minePrefab;
    public float placeOffset = 0.8f;          // distance in front of player
    public int maxMines = 3;
    public float placeCooldown = 0.25f;
    public LayerMask blockingLayers;          // prevents placing inside walls
    public bool useLocalScaleForFacing = true;

    float cooldownTimer = 0f;
    List<GameObject> placed = new List<GameObject>();

    void Update()
    {
        cooldownTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.M) && cooldownTimer <= 0f)
        {
            TryPlaceMine();
            cooldownTimer = placeCooldown;
        }

        // cleanup null references
        for (int i = placed.Count - 1; i >= 0; i--)
            if (placed[i] == null) placed.RemoveAt(i);
    }

    void TryPlaceMine()
    {
        if (minePrefab == null) { Debug.LogWarning("Mine prefab not assigned"); return; }
        if (placed.Count >= maxMines) return;

        int facing = GetFacingDirection();
        Vector2 spawnPos = (Vector2)transform.position + new Vector2(placeOffset * facing, 0f);

        // small overlap check so we don't place inside walls
        float checkRadius = 0.2f;
        if (Physics2D.OverlapCircle(spawnPos, checkRadius, blockingLayers)) return;

        var go = Instantiate(minePrefab, spawnPos, Quaternion.identity);

        // optional: if you want the owner to be immune for the armDelay window you can implement that in Mine
        placed.Add(go);
    }

    int GetFacingDirection()
    {
        if (useLocalScaleForFacing)
            return transform.localScale.x >= 0 ? 1 : -1;
        return 1; // change if you have another facing system
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        int f = useLocalScaleForFacing ? (transform.localScale.x >= 0 ? 1 : -1) : 1;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere((Vector2)transform.position + new Vector2(placeOffset * f, 0f), 0.15f);
    }
#endif
}
