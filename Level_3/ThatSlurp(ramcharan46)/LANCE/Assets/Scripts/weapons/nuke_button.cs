using UnityEngine;
using System.Collections;

public class nb : MonoBehaviour
{
    [Header("Nuke Settings")]
    public GameObject nukePrefab;           // Your Nuke prefab
    public float spawnDelay = 3f;           // Time (seconds) after siren starts before spawning

    [Header("Audio Settings")]
    public AudioSource audioSource;         // Plays the siren sound
    public AudioClip nukeSirenClip;         // The nuke siren sound clip

    private bool nukeCalled = false;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B) && !nukeCalled)
        {
            nukeCalled = true;
            StartCoroutine(PlaySirenAndSpawn());
        }
    }

    private IEnumerator PlaySirenAndSpawn()
    {
        // --- Play Siren ---
        if (audioSource != null && nukeSirenClip != null)
        {
            audioSource.clip = nukeSirenClip;
            audioSource.Play();
        }

        // --- Wait before spawning ---
        yield return new WaitForSeconds(spawnDelay);

        // --- Spawn Nuke ---
        Instantiate(nukePrefab);

        // (optional) reset so you can call it again after some time
        yield return new WaitForSeconds(1f);
        nukeCalled = false;
    }
}
