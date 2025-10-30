// PlayerHook.cs
using UnityEngine;
using System.Collections;

public class PlayerHook : MonoBehaviour
{
    [Header("Hook Settings")]
    public GameObject hookPrefab;
    public Transform hookSpawnPoint;
    public float maxCharge = 3f;

    [Header("Distance & Speed")]
    public float hookSpeed = 25f;
    public float minHookDistance = 5f;
    public float maxHookDistance = 20f;
    
    [Header("Effects")]
    public float pullShakeDuration = 0.15f;
    public float pullShakeMagnitude = 0.2f;
    // 👇 NEW: Add a field for the blood particle prefab
    public GameObject bloodEffectPrefab; 

    private float chargeTime;
    private bool isCharging;
    private GameObject chargingHook;
    
    // ... Update() and LaunchHook() methods are unchanged ...
    #region Unchanged Methods
    void Update()
    {
        // Start charging
        if (Input.GetMouseButtonDown(0))
        {
            isCharging = true;
            chargeTime = 0f;
            chargingHook = Instantiate(hookPrefab, hookSpawnPoint.position, Quaternion.identity);

            Hook previewHookScript = chargingHook.GetComponent<Hook>();
            if (previewHookScript != null)
            {
                previewHookScript.enabled = false;
            }

            Rigidbody2D rb = chargingHook.GetComponent<Rigidbody2D>();
            if (rb != null) rb.isKinematic = true;

            if (chargingHook.GetComponent<LineRenderer>())
            {
                chargingHook.GetComponent<LineRenderer>().enabled = false;
            }
        }

        // While holding (charging)
        if (Input.GetMouseButton(0) && isCharging)
        {
            chargeTime += Time.deltaTime;
            chargeTime = Mathf.Min(chargeTime, maxCharge);
            if (chargingHook)
            {
                chargingHook.transform.position = hookSpawnPoint.position;
                Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2 dir = (mousePos - (Vector2)hookSpawnPoint.position).normalized;
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                chargingHook.transform.rotation = Quaternion.Euler(0, 0, angle);
            }
        }

        // Release the hook
        if (Input.GetMouseButtonUp(0) && isCharging)
        {
            if (chargingHook) Destroy(chargingHook);
            LaunchHook();
            isCharging = false;
        }
    }

    void LaunchHook()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = (mousePos - (Vector2)hookSpawnPoint.position).normalized;
        float chargeRatio = chargeTime / maxCharge;
        float travelDistance = Mathf.Lerp(minHookDistance, maxHookDistance, chargeRatio);
        GameObject hook = Instantiate(hookPrefab, hookSpawnPoint.position, Quaternion.identity);
        
        if (hook.GetComponent<LineRenderer>())
        {
            hook.GetComponent<LineRenderer>().enabled = true;
        }

        Rigidbody2D rb = hook.GetComponent<Rigidbody2D>();
        rb.linearVelocity = direction * hookSpeed;
        
        hook.GetComponent<Hook>().Init(this, hookSpawnPoint, travelDistance);
    }
    #endregion
    
    public void PullEnemy(GameObject enemy)
    {
        // Trigger the screenshake
        if (ScreenShake.instance != null)
        {
            ScreenShake.instance.TriggerShake(pullShakeDuration, pullShakeMagnitude);
        }

        // 👇 NEW: Spawn blood particles at the enemy's position
        if (bloodEffectPrefab != null)
        {
            Instantiate(bloodEffectPrefab, enemy.transform.position, Quaternion.identity);
        }

        StartCoroutine(PullEnemyTowardsPlayer(enemy));
    }

    private IEnumerator PullEnemyTowardsPlayer(GameObject enemy)
    {
        float pullSpeed = 10f;
        float pullDistance = 2f;
        Vector3 targetPos = transform.position + (enemy.transform.position - transform.position).normalized * pullDistance;
        while (enemy != null && Vector3.Distance(enemy.transform.position, targetPos) > 0.1f)
        {
            enemy.transform.position = Vector3.MoveTowards(enemy.transform.position, targetPos, pullSpeed * Time.deltaTime);
            yield return null;
        }
    }
}