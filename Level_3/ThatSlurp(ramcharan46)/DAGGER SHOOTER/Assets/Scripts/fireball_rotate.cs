using UnityEngine;

public class HelicalOrbitFireball : MonoBehaviour
{
    public float startRadius = 1.5f;   // Starting distance from player
    public float expandSpeed = 0.5f;   // How fast the circle expands
    public float orbitSpeed = 90f;     // How fast it rotates around player
    public float lifeTime = 6f;        // Lifetime before destroying
    public bool faceOutward = true;    // Make fireballs face outward

    private Transform player;
    private float angleDeg;
    private float currentRadius;

    // This will be called by PlayerAttack to give each fireball its initial angle
    public void Initialize(float initialAngle)
    {
        angleDeg = initialAngle;
    }

    void Start()
    {
        // Find player automatically
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj == null)
        {
            Debug.LogError("HelicalOrbitFireball: No GameObject tagged 'Player' found!");
            Destroy(gameObject);
            return;
        }

        player = playerObj.transform;
        currentRadius = startRadius;
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (player == null) return;

        // Slowly rotate and expand
        angleDeg += orbitSpeed * Time.deltaTime;
        if (angleDeg >= 360f) angleDeg -= 360f;

        currentRadius += expandSpeed * Time.deltaTime;

        // Convert polar coords to position
        float rad = angleDeg * Mathf.Deg2Rad;
        Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad)) * currentRadius;
        transform.position = player.position + offset;

        // Make the fireball face outward
        if (faceOutward)
        {
            Vector2 dir = (transform.position - player.position).normalized;
            float rotationZ = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, rotationZ - 90);
        }
    }
}
