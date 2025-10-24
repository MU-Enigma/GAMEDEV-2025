using UnityEngine;
using System.Collections.Generic;

public class WandFireballs : MonoBehaviour
{
    [Header("Fireball Settings")]
    public GameObject fireballPrefab;     // Assign your fireball prefab
    public int fireballCount = 10;        // Number of fireballs in each ring
    public float startRadius = 1.5f;      // Starting circle radius
    public float fireballScale = 0.6f;    // Fireball size
    public float lifetime = 5f;           // How long each ring lasts
    public KeyCode castKey = KeyCode.F;   // Key to cast the spell

    private List<Ring> activeRings = new List<Ring>();

    private class FireballData
    {
        public GameObject obj;
        public float baseAngle;
    }

    private class Ring
    {
        public List<FireballData> fireballs = new List<FireballData>();
        public float elapsed = 0f;
    }

    void Update()
    {
        if (Input.GetKeyDown(castKey))
        {
            SpawnRing();
        }
    }

    void SpawnRing()
    {
        Ring newRing = new Ring();

        for (int i = 0; i < fireballCount; i++)
        {
            float angle = i * Mathf.PI * 2f / fireballCount;
            Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * startRadius;
            Vector3 spawnPos = transform.position + offset;

            GameObject fb = Instantiate(fireballPrefab, spawnPos, Quaternion.identity);
            fb.transform.localScale = Vector3.one * fireballScale;

            // Rotate fireball to face outward
            Vector2 dir = offset.normalized;
            float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            fb.transform.rotation = Quaternion.Euler(0, 0, rotZ - 90);

            newRing.fireballs.Add(new FireballData { obj = fb, baseAngle = angle });
        }

        activeRings.Add(newRing);
    }

}
