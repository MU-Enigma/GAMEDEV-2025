using UnityEngine;
using System.Collections;

public class SingerGuy : MonoBehaviour
{
    public AudioSource songAudio;
    public float effectRadius = 5f;
    public float songDuration = 3f;

    void OnEnable()
    {
        if (songAudio == null)
            songAudio = GetComponent<AudioSource>();

        if (songAudio != null)
            songAudio.Play();

        StartCoroutine(SingRoutine());
    }

    IEnumerator SingRoutine()
    {
        Collider2D[] enemies = Physics2D.OverlapCircleAll(transform.position, effectRadius);
        foreach (var enemy in enemies)
        {
            EnemyReaction e = enemy.GetComponent<EnemyReaction>();
            if (e != null)
                e.OnHeardSong();
        }

        yield return new WaitForSeconds(songDuration);

        gameObject.SetActive(false);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, effectRadius);
    }
}
