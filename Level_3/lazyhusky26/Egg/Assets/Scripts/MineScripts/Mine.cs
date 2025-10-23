// Mine.cs
using System.Collections;
using UnityEngine;

public class Mine : MonoBehaviour
{
    [Header("Trigger / Kill")]
    public string[] killTags = new string[] { "Player", "Enemy" };
    public float armDelay = 0f;             // set >0 to prevent immediate self-trigger after placing
    public GameObject explosionVFX;         // optional: assign a prefab that cleans itself up
    public AudioClip explosionSFX;          // optional

    bool armed = false;
    AudioSource audioSource;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Start()
    {
        if (armDelay <= 0f) armed = true;
        else StartCoroutine(ArmAfterDelay(armDelay));
    }

    IEnumerator ArmAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        armed = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!armed) return;

        // check tags
        foreach (var t in killTags)
        {
            if (other.CompareTag(t))
            {
                KillTarget(other);
                Explode();
                return;
            }
        }
    }

    void KillTarget(Collider2D col)
    {
        // try to destroy the root object that holds the tag (common in hierarchies)
        GameObject target = col.gameObject;
        // prefer top-most parent with the same tag
        Transform root = target.transform;
        while (root.parent != null)
        {
            if (root.parent.CompareTag(root.tag)) root = root.parent;
            else break;
        }

        // final safety: if the collider itself has the tag use that, else try root
        GameObject toDestroy = otherOrRootWithTag(target, root, col.tag);
        if (toDestroy != null)
        {
            Destroy(toDestroy);
        }
    }

    // helper to choose the correct object to destroy
    GameObject otherOrRootWithTag(GameObject otherObj, Transform rootTransform, string tag)
    {
        if (otherObj.CompareTag(tag)) return otherObj;
        if (rootTransform != null && rootTransform.CompareTag(tag)) return rootTransform.gameObject;
        // fallback to the collider's gameobject
        return otherObj;
    }

    void Explode()
    {
        if (explosionVFX != null)
            Instantiate(explosionVFX, transform.position, Quaternion.identity);

        if (explosionSFX != null && audioSource != null)
            audioSource.PlayOneShot(explosionSFX);

        // destroy mine immediately (if you want sound to finish, detach audio into a separate object)
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(1, 0.5f, 0, 0.6f);
        var c = GetComponent<Collider2D>();
        if (c != null) Gizmos.DrawWireSphere(transform.position, 0.5f);
    }
#endif
}
