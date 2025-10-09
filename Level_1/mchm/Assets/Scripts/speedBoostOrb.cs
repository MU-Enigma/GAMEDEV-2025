using UnityEngine;

/// <summary>
/// The script for the little orbs that give you a speed boost.
/// It just pulses, waits for the player to touch it, then dies. A noble life.
/// </summary>
[RequireComponent(typeof(CircleCollider2D))]
public class SpeedBoostOrb : MonoBehaviour
{
    [Header("Orb Settings")]
    public float pulseSpeed = 2f;
    public float pulseMagnitude = 0.1f;
    
    private Vector3 _originalScale;
    private OrbSpawner _spawnerRef; // a reference to the thing that spawned it
    
    void Start()
    {
        _originalScale = transform.localScale;
        // just making sure the collider is a trigger so you can pass through it.
        GetComponent<CircleCollider2D>().isTrigger = true;
    }
    
    void Update()
    {
        // a classic sin wave pulse effect. cheap and effective.
        // it just bobs the scale up and down around its original size.
        float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseMagnitude;
        transform.localScale = _originalScale + Vector3.one * pulse;
    }
    
    // this is called by the spawner so the orb knows its master.
    public void SetSpawner(OrbSpawner spawner) => _spawnerRef = spawner;

    void OnTriggerEnter2D(Collider2D other)
    {
        // did the player touch me?
        if (other.CompareTag("Player"))
        {
            // grab the player's controller script
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // GO FAST
                player.ApplySpeedBoost();
                
                // tell my father that i have fulfilled my purpose and am ascending to a higher plane of existence
                if(_spawnerRef != null) _spawnerRef.OnOrbCollected();

                // ok bye
                Destroy(gameObject);
            }
        }
    }
}
