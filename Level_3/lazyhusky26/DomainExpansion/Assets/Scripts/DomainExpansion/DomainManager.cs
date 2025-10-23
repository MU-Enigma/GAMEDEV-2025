using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class DomainManager : MonoBehaviour
{
    public static DomainManager Instance;

    private Vector3 playerOriginalPosition;
    private AsyncOperation loadOperation;
    public GameObject explosionVFX;
    public AudioClip domainActivateSFX;
    public AudioClip enemyDeathSFX;

    private AudioSource audioSource;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartDomainExpansion(GameObject player, List<GameObject> enemies)
    {
        if (domainActivateSFX != null)
        {
            audioSource.PlayOneShot(domainActivateSFX);  // Play immediately on trigger
        }
        StartCoroutine(DomainSequence(player, enemies));
    }

    private IEnumerator DomainSequence(GameObject player, List<GameObject> enemies)
    {
        if (player == null)
        {
            Debug.LogError("Player is null! Cannot start domain expansion.");
            yield break;
        }

        // Prevent player from being destroyed during scene transitions
        DontDestroyOnLoad(player);

        // Save original player position and scene
        playerOriginalPosition = player.transform.position;
        Scene originalScene = player.scene;

        // Load the domain scene additively
        loadOperation = SceneManager.LoadSceneAsync("DomainDimensionScene", LoadSceneMode.Additive);
        while (!loadOperation.isDone)
            yield return null;

        Scene domainScene = SceneManager.GetSceneByName("DomainDimensionScene");

        // Move player to domain scene & reset position
        SceneManager.MoveGameObjectToScene(player, domainScene);
        player.transform.position = Vector3.zero;

        // Move enemies to domain scene and spread them relative to player
        for (int i = 0; i < enemies.Count; i++)
        {
            GameObject enemy = enemies[i];
            if (enemy != null)
            {
                SceneManager.MoveGameObjectToScene(enemy, domainScene);

                // Keep enemies same relative distance to player as original scene
                Vector3 offset = enemies[i].transform.position - playerOriginalPosition;
                enemy.transform.position = player.transform.position + offset;
            }
        }

        yield return null; // wait one frame for scene moves

        yield return new WaitForSeconds(0.5f); // short delay before effects

        // Trigger enemy flashing and explosions with VFX and death SFX
        foreach (var enemy in enemies)
        {
            if (enemy != null)
            {
                EnemyDomainEffect ede = enemy.GetComponent<EnemyDomainEffect>();
                if (ede != null)
                {
                    ede.StartFlashingAndExplode(2f, explosionVFX, enemyDeathSFX, audioSource);
                }
            }
        }

        // Wait until all enemies are destroyed (null)
        bool enemiesAlive = true;
        while (enemiesAlive)
        {
            enemiesAlive = false;
            foreach (var enemy in enemies)
            {
                if (enemy != null)
                {
                    enemiesAlive = true;
                    break;
                }
            }
            yield return null;
        }

        // Wait a little longer to let final sounds/VFX finish
        yield return new WaitForSeconds(0.5f);

        // Unload the domain scene only (keep original scene intact)
        yield return SceneManager.UnloadSceneAsync(domainScene);

        // Move player back to original scene and restore position
        SceneManager.MoveGameObjectToScene(player, originalScene);
        player.transform.position = playerOriginalPosition;

        Debug.Log("Domain expansion complete. Player returned.");
    }
}
