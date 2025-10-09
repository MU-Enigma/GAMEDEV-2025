using UnityEngine;

public class AfterImage : MonoBehaviour
{
    public float fadeTime = 0.3f;
    private float fadeTimer;
    private SpriteRenderer sr;
    private Color origColor;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        origColor = sr.color;
        fadeTimer = fadeTime;
    }

    public void Init(Sprite sprite, Vector3 scale, Quaternion rot)
    {
        sr.sprite = sprite;
        transform.localScale = scale;
        transform.rotation = rot;
        origColor = sr.color;
        fadeTimer = fadeTime;
    }

    void Update()
    {
        fadeTimer -= Time.deltaTime;
        if (fadeTimer > 0f)
        {
            float t = fadeTimer / fadeTime;
            sr.color = new Color(origColor.r, origColor.g, origColor.b, t);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
