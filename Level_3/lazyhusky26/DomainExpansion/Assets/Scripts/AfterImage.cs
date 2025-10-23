using UnityEngine;

public class AfterImage : MonoBehaviour
{
    public float fadeSpeed = 5f;
    private SpriteRenderer sr;
    private Color color;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        if (sr == null)
        {
            Debug.LogWarning("AfterImage requires a SpriteRenderer.");
            Destroy(gameObject);
            return;
        }

        color = sr.color;
    }

    void Update()
    {
        color.a = Mathf.Max(0f, color.a - fadeSpeed * Time.deltaTime);
        sr.color = color;

        if (color.a <= 0f)
        {
            Destroy(gameObject);
        }
    }
}
