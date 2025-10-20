// ElectronControl.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ElectronData
{
    public float radius = 1.5f;
    public Vector3 orbitAxis = Vector3.up;
    public float orbitSpeed = 90f;
    public float depthScale = 0.1f;
    [HideInInspector] public float currentAngle = 0f;
}

public class ElectronControl : MonoBehaviour
{
    [Header("Electron Settings")]
    public GameObject electronPrefab;
    public List<ElectronData> electronsData = new List<ElectronData>();
    public float respawnTime = 2f;

    [Header("Laser Settings")]
    public GameObject laserPrefab;
    public float laserDuration = 0.5f;
    public float laserShakeIntensity = 0.2f;
    public float laserShakeDuration = 0.3f;
    public float laserMaxLength = 20f;

    [Header("Input Settings")]
    public KeyCode laserKey = KeyCode.E;
    public string controllerLaserButton = "Fire1"; // PS4 X or Xbox A

    private GameObject[] electronObjects;
    private TrailRenderer[] electronTrails;
    private Vector2 targetDirection;
    private Camera mainCamera;
    private bool isFiringLaser = false;
    private bool[] electronActive;
    private float[] respawnTimers;

    void Start()
    {
        mainCamera = Camera.main;
        InitializeElectrons();
    }

    void InitializeElectrons()
    {
        int electronCount = electronsData.Count;
        electronObjects = new GameObject[electronCount];
        electronTrails = new TrailRenderer[electronCount];
        electronActive = new bool[electronCount];
        respawnTimers = new float[electronCount];
        
        for (int i = 0; i < electronCount; i++)
        {
            electronsData[i].currentAngle = i * (360f / electronCount);
            electronActive[i] = true;
        }
        
        for (int i = 0; i < electronCount; i++)
        {
            electronObjects[i] = Instantiate(electronPrefab, transform.position, Quaternion.identity);
            electronObjects[i].transform.parent = transform;
            electronObjects[i].name = "Electron_" + i;
            
            TrailRenderer trail = electronObjects[i].AddComponent<TrailRenderer>();
            trail.time = 0.5f;
            trail.startWidth = 0.15f;
            trail.endWidth = 0.05f;
            trail.material = new Material(Shader.Find("Sprites/Default"));
            trail.startColor = Color.white;
            trail.endColor = new Color(1f, 1f, 1f, 0f);
            electronTrails[i] = trail;
        }
    }

    void Update()
    {
        if (isFiringLaser) return;

        // Get target position from mouse pointer
        Vector3 targetPos;
        if (MousePointerController.Instance.UsingController)
        {
            targetPos = MousePointerController.Instance.PointerWorldPosition;
        }
        else
        {
            targetPos = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        }
        targetPos.z = 0;
        targetDirection = (targetPos - transform.position).normalized;

        UpdateRespawnTimers();

        bool laserInput = false;
        if (!MousePointerController.Instance.UsingController)
        {
            laserInput = Input.GetKeyDown(laserKey);
        }
        else
        {
            laserInput = Input.GetButtonDown(controllerLaserButton);
        }

        if (laserInput)
        {
            StartCoroutine(FireLaser());
        }

        OrbitElectrons();
    }

    void UpdateRespawnTimers()
    {
        for (int i = 0; i < electronsData.Count; i++)
        {
            if (!electronActive[i])
            {
                respawnTimers[i] -= Time.deltaTime;
                if (respawnTimers[i] <= 0)
                {
                    electronActive[i] = true;
                    electronObjects[i].SetActive(true);
                }
            }
        }
    }

    void OrbitElectrons()
    {
        for (int i = 0; i < electronsData.Count; i++)
        {
            if (electronObjects[i] == null || !electronActive[i]) continue;

            ElectronData data = electronsData[i];
            data.currentAngle += data.orbitSpeed * Time.deltaTime;
            if (data.currentAngle > 360f) data.currentAngle -= 360f;
            
            float angleRad = data.currentAngle * Mathf.Deg2Rad;
            Quaternion rotation = Quaternion.AngleAxis(data.currentAngle, data.orbitAxis.normalized);
            Vector3 orbitPos = rotation * (Vector3.forward * data.radius);
            orbitPos.z *= data.depthScale;
            electronObjects[i].transform.localPosition = orbitPos;
        }
    }

    IEnumerator FireLaser()
    {
        isFiringLaser = true;
        
        int electronIndex = -1;
        for (int i = 0; i < electronsData.Count; i++)
        {
            if (electronActive[i])
            {
                electronIndex = i;
                break;
            }
        }

        if (electronIndex == -1) 
        {
            isFiringLaser = false;
            yield break;
        }

        electronActive[electronIndex] = false;
        electronObjects[electronIndex].SetActive(false);
        respawnTimers[electronIndex] = respawnTime;

        GameObject laser = Instantiate(laserPrefab, transform.position, Quaternion.identity);
        float angle = Mathf.Atan2(targetDirection.y, targetDirection.x) * Mathf.Rad2Deg;
        laser.transform.rotation = Quaternion.Euler(0, 0, angle);
        
        float laserLength = CalculateLaserLength();
        laser.transform.localScale = new Vector3(laserLength, 0.1f, 1f);
        laser.transform.position = transform.position + (Vector3)targetDirection * laserLength * 0.5f;

        if (mainCamera != null)
        {
            CameraShake cameraShake = mainCamera.GetComponent<CameraShake>();
            if (cameraShake != null)
            {
                cameraShake.TriggerShake(laserShakeIntensity, laserShakeDuration);
            }
        }

        yield return new WaitForSeconds(laserDuration);
        Destroy(laser);
        isFiringLaser = false;
    }

    float CalculateLaserLength()
    {
        Vector3 screenEdge = mainCamera.ViewportToWorldPoint(new Vector3(1, 1, mainCamera.nearClipPlane));
        float maxDistance = Vector3.Distance(transform.position, screenEdge) * 1.5f;
        return Mathf.Min(maxDistance, laserMaxLength);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        
        foreach (ElectronData data in electronsData)
        {
            Vector3 center = transform.position;
            Vector3 up = data.orbitAxis.normalized;
            Vector3 forward = Vector3.Slerp(up, -up, 0.5f);
            Vector3 right = Vector3.Cross(up, forward).normalized;
            forward *= data.depthScale;
            
            Vector3 prevPoint = center + right * data.radius;
            for (int i = 0; i <= 36; i++)
            {
                float angle = i * Mathf.PI * 2 / 36;
                Vector3 nextPoint = center + (right * Mathf.Cos(angle) + forward * Mathf.Sin(angle)) * data.radius;
                Gizmos.DrawLine(prevPoint, nextPoint);
                prevPoint = nextPoint;
            }
        }
    }

    public void AddElectron(float radius = 1.5f, Vector3 orbitAxis = default, float orbitSpeed = 90f, float depthScale = 0.1f)
    {
        if (orbitAxis == default) orbitAxis = Vector3.up;
        
        ElectronData newData = new ElectronData();
        newData.radius = radius;
        newData.orbitAxis = orbitAxis;
        newData.orbitSpeed = orbitSpeed;
        newData.depthScale = depthScale;
        newData.currentAngle = electronsData.Count * (360f / (electronsData.Count + 1));
        
        electronsData.Add(newData);
        
        if (electronObjects != null)
        {
            foreach (GameObject electron in electronObjects)
            {
                if (electron != null) Destroy(electron);
            }
        }
        
        InitializeElectrons();
    }
}