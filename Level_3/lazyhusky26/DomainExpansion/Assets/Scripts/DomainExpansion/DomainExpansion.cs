using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DomainExpansion : MonoBehaviour
{
    [Header("Domain Settings")]
    public float domainRadius = 5f;
    public float circleExpandDuration = 1f;
    public LayerMask enemyLayer;
    public Transform domainCircle; // assign your expanding circle sprite
    public Color circleColor = new Color(0.8f, 0.8f, 0.8f, 0.3f);

    [Header("Audio")]
    public AudioClip domainActivationSFX;
    private AudioSource audioSource;

    private bool domainActive = false;

    void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && !domainActive)
        {
            StartCoroutine(ActivateDomain());
        }
    }

    private IEnumerator ActivateDomain()
    {
        domainActive = true;

        // 🔊 Play activation SFX instantly (on button press)
        if (domainActivationSFX != null)
            audioSource.PlayOneShot(domainActivationSFX);

        // 🎨 Animate circle coming out
        yield return StartCoroutine(AnimateCircle());

        // 🔍 Find enemies in radius
        Collider2D[] enemiesColliders = Physics2D.OverlapCircleAll(transform.position, domainRadius, enemyLayer);
        List<GameObject> enemies = new List<GameObject>();
        foreach (var col in enemiesColliders)
            enemies.Add(col.gameObject);

        // 🌌 Call DomainManager to handle teleportation & explosions
        DomainManager.Instance.StartDomainExpansion(gameObject, enemies);

        // hide circle after activation
        if (domainCircle != null)
            domainCircle.localScale = Vector3.zero;

        domainActive = false;
    }

    private IEnumerator AnimateCircle()
    {
        if (domainCircle == null)
        {
            Debug.LogWarning("No domainCircle assigned!");
            yield break;
        }

        SpriteRenderer sr = domainCircle.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = circleColor;

        float elapsed = 0f;
        Vector3 startScale = Vector3.zero;
        Vector3 endScale = new Vector3(domainRadius * 2f, domainRadius * 2f, 1f);

        while (elapsed < circleExpandDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / circleExpandDuration);
            domainCircle.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, domainRadius);
    }
}
