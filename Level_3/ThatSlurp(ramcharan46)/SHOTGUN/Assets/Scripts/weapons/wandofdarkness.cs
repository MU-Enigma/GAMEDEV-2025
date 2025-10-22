using UnityEngine;
using System.Collections.Generic;

public class WandHelicalFireballs : MonoBehaviour
{
    [Header("Fireball Settings")]
    public GameObject fireballPrefab;     // Assign your fireball prefab
    public int fireballCount = 10;        // Number of fireballs in each ring
    public float startRadius = 1.5f;      // Starting circle radius
    public float expandSpeed = 0.4f;      // How fast each ring expands outward
    public float rotateSpeed = 90f;       // Rotation speed of the ring
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

        UpdateAllRings();
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

    void UpdateAllRings()
    {
        for (int r = activeRings.Count - 1; r >= 0; r--)
        {
            Ring ring = activeRings[r];
            ring.elapsed += Time.deltaTime;

            float currentRadius = startRadius + expandSpeed * ring.elapsed;
            float rotationOffset = rotateSpeed * ring.elapsed * Mathf.Deg2Rad;

            for (int i = 0; i < ring.fireballs.Count; i++)
            {
                var f = ring.fireballs[i];
                if (f.obj == null) continue;

                float angle = f.baseAngle + rotationOffset;
                Vector3 offset = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)) * currentRadius;
                f.obj.transform.position = transform.position + offset;

                // Keep facing outward
                Vector2 dir = offset.normalized;
                float rotZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                f.obj.transform.rotation = Quaternion.Euler(0, 0, rotZ - 90);
            }

            // Remove finished rings
            if (ring.elapsed > lifetime)
            {
                foreach (var f in ring.fireballs)
                {
                    if (f.obj != null) Destroy(f.obj);
                }
                activeRings.RemoveAt(r);
            }
        }
    }
}
