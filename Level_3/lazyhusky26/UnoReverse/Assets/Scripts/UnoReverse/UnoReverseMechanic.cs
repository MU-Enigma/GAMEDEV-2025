using System.Collections;
using UnityEngine;

public class UnoReverseMechanic : MonoBehaviour
{
    public GameObject pelletPrefab;
    public Transform enemy;
    public Transform player;
    public GameObject bloodVFXPrefab;
    public GameObject unoReverseCardVisual;  // Visual shown when reverse activates

    private GameObject currentPellet;
    private bool pelletIncoming = false;
    private bool reverseActivated = false;

    void Start()
    {
        unoReverseCardVisual.SetActive(false);
        StartCoroutine(SpawnPelletsContinuously());
    }

    void Update()
    {
        if (pelletIncoming && !reverseActivated && Input.GetKeyDown(KeyCode.C))
        {
            ActivateUnoReverse();
        }
    }

    IEnumerator SpawnPelletsContinuously()
    {
        while (true)
        {
            if (!pelletIncoming)
            {
                FirePellet();
            }
            yield return new WaitForSeconds(2f);
        }
    }

    void FirePellet()
    {
        Vector2 spawnPos = (Vector2)enemy.position + ((Vector2)(player.position - enemy.position).normalized * 0.5f);
        currentPellet = Instantiate(pelletPrefab, spawnPos, Quaternion.identity);

        PelletMovement pelletMovement = currentPellet.GetComponent<PelletMovement>();
        pelletMovement.player = player;
        pelletMovement.enemy = enemy;

        pelletIncoming = true;
        reverseActivated = false;

        Collider2D pelletCollider = currentPellet.GetComponent<Collider2D>();
        Collider2D enemyCollider = enemy.GetComponent<Collider2D>();
        Physics2D.IgnoreCollision(pelletCollider, enemyCollider, true);
        StartCoroutine(ReenableCollision(pelletCollider, enemyCollider));
    }

    void ActivateUnoReverse()
    {
        reverseActivated = true;
        unoReverseCardVisual.SetActive(true);

        PelletMovement pelletMovement = currentPellet.GetComponent<PelletMovement>();
        pelletMovement.Reverse();

        Debug.Log("Uno Reverse Activated! Pellet reflected back to enemy!");
    }


    IEnumerator ReenableCollision(Collider2D pelletCol, Collider2D enemyCol)
    {
        yield return new WaitForSeconds(0.3f);
        Physics2D.IgnoreCollision(pelletCol, enemyCol, false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentPellet == null) return;  // Safety check

        if (other.gameObject == currentPellet)
        {
            if (!reverseActivated)
            {
                // Pellet hit player, player takes damage or dies
                Debug.Log("Player hit! No Uno Reverse!");
                Destroy(currentPellet);
                pelletIncoming = false;
                unoReverseCardVisual.SetActive(false);
            }
            else
            {
                // Pellet hit player but reversed — should not happen
            }
        }
        else if (other.gameObject == enemy.gameObject && reverseActivated)
        {
            // Pellet hit enemy after reverse, enemy dies with blood VFX
            Debug.Log("Enemy killed by Uno Reverse pellet!");
            Instantiate(bloodVFXPrefab, enemy.position, Quaternion.identity);
            Destroy(enemy.gameObject);
            Destroy(currentPellet);
            unoReverseCardVisual.SetActive(false);
            pelletIncoming = false;
        }
    }
}
