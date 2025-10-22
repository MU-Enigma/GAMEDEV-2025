using System.Collections;
using UnityEngine;

public class DeathNoteController : MonoBehaviour
{
    [Header("Input")]
    public KeyCode equipKey = KeyCode.Tab;       // toggle equip
    public LayerMask clickableLayers;            // set to layer(s) that contain player/enemies
    public string playerTag = "Player";
    public string enemyTag = "Enemy";

    [Header("Ritual")]
    public AudioClip heartbeatClip;              // short heartbeat sound (one beat)
    public AudioClip deathClip;                  // final death sound

    [Header("Visual")]
    public Color flashColor = Color.red;
    public float flashFrequency = 8f;            // how fast the sprite pulses during ritual

    [Header("Cursor / UI")]
    public Texture2D deathCursor;                // optional cursor when equipped
    public Vector2 cursorHotspot = new Vector2(16,16);

    bool equipped = false;
    CursorMode cursorMode = CursorMode.Auto;

    public GameObject deathNoteVisual; // assign in inspector


    void Update()
    {
        // toggle equip
        if (Input.GetKeyDown(equipKey))
        {
            equipped = !equipped;
            CursorSet(equipped);
            ToggleDeathNoteVisual(equipped);
        }

        // if equipped, left mouse click tries to target something
        if (equipped && Input.GetMouseButtonDown(0))
        {
            TryClickTarget();
        }
    }

    void CursorSet(bool on)
    {
        if (deathCursor != null && on)
            Cursor.SetCursor(deathCursor, cursorHotspot, cursorMode);
        else
            Cursor.SetCursor(null, Vector2.zero, cursorMode);

        if (deathNoteVisual != null)
            deathNoteVisual.SetActive(on);
    }


    void TryClickTarget()
    {
        Vector2 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hit = Physics2D.OverlapPoint(wp, clickableLayers);
        if (hit == null) 
        {
            Debug.Log("No collider hit");
            return;
        }

        GameObject target = hit.gameObject;
        Debug.Log($"Clicked object: {target.name} with tag {target.tag}");

        // climb up parents until find tagged object (optional)
        Transform t = target.transform;
        while (t.parent != null && t.CompareTag("") )
        {
            t = t.parent;
        }
        // use the collider object or the first parent that has the expected tag
        if (hit.CompareTag(playerTag) || hit.CompareTag(enemyTag))
            target = hit.gameObject;
        else
        {
            // search parents for correct tag
            Transform p = hit.transform;
            while (p != null)
            {
                if (p.CompareTag(playerTag) || p.CompareTag(enemyTag))
                {
                    target = p.gameObject;
                    break;
                }
                p = p.parent;
            }
        }

        if (target.CompareTag(playerTag))
        {
            // instant kill player
            PerformInstantKill(target);
        }
        else if (target.CompareTag(enemyTag))
        {
            // start ritual on enemy (disable stacking by checking a component)
            if (target.GetComponent<DeathNoteRitual>() == null)
            {
                var ritual = target.AddComponent<DeathNoteRitual>();
                ritual.Init(target, heartbeatClip, deathClip, flashColor, flashFrequency);
            }
        }
    }


    void PerformInstantKill(GameObject victim)
    {
        // optional: play death clip at victim position
        if (deathClip != null)
        {
            SoundUtils.PlayClipAtPoint(deathClip, victim.transform.position);
        }

        // destroy immediately
        Destroy(victim);
    }

    void ToggleDeathNoteVisual(bool on)
    {
        if (deathNoteVisual != null)
            deathNoteVisual.SetActive(on);
    }
}
