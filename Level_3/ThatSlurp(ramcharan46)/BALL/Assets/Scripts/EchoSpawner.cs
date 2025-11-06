using UnityEngine;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class EchoSpawner : MonoBehaviour
{
    [Header("Echo Settings")]
    public GameObject echoPrefab;
    public float echoLifetime = 1.5f;
    public KeyCode spawnKey = KeyCode.Q;
    public KeyCode swapKey = KeyCode.E;

    [Header("Audio")]
    public AudioClip warpClip;
    private AudioSource audioSource;

    private List<GameObject> activeEchoes = new List<GameObject>();

    void Start(){
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null){
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    void Update(){
        if (Input.GetKeyDown(spawnKey)||(Gamepad.current!=null&&Gamepad.current.leftShoulder.wasReleasedThisFrame)){
            SpawnEcho();
        }

        if (Input.GetKeyDown(swapKey)||(Gamepad.current!=null&&Gamepad.current.rightShoulder.wasReleasedThisFrame)){
            SwapWithLatestEcho();
        }
    }

    void SpawnEcho(){
        if (echoPrefab == null) return;

        GameObject echo = Instantiate(echoPrefab,transform.position,transform.rotation);

        echo.transform.localScale = transform.localScale;

        echo.transform.SetParent(null);

        SpriteRenderer sr = echo.GetComponent<SpriteRenderer>();
        if (sr != null) StartCoroutine(FadeAndDestroy(echo, sr, echoLifetime));

        activeEchoes.Add(echo);
    }

    void SwapWithLatestEcho(){
        if (activeEchoes.Count == 0) return;

        GameObject latestEcho = activeEchoes[activeEchoes.Count - 1];
        if (latestEcho == null) return;

        transform.position = latestEcho.transform.position;

        if (warpClip != null && audioSource != null){
            audioSource.PlayOneShot(warpClip);
        }

        activeEchoes.Remove(latestEcho);
        Destroy(latestEcho);
    }

    System.Collections.IEnumerator FadeAndDestroy(GameObject obj, SpriteRenderer sr, float lifetime)
    {
        float t = 0f;
        Color baseColor = sr.color;

        while (t < lifetime){
            if (obj == null || sr == null) yield break;

            float alpha = Mathf.Lerp(1f, 0f, t / lifetime);
            sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
            t += Time.deltaTime;
            yield return null;
        }

        if (obj != null){
            activeEchoes.Remove(obj);
            Destroy(obj);
        }
    }
}
