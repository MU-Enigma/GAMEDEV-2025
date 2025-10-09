using UnityEngine;
/// <summary>
/// another script stolen 1:1 ;)
/// </summary>
public class ShadowEcho : MonoBehaviour
{
    public float lifetime = 0.6f;
    public float shrinkAmount = 0.3f;
    public float fadeCurvePower = 1.2f;

    private SpriteRenderer sr;
    private float age;
    private Color startColor;
    private Vector3 startScale;

    void Awake(){
        sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        startColor = sr.color;
        startScale = transform.localScale;
    }

    void Update(){
        age += Time.deltaTime;
        float t = Mathf.Clamp01(age / lifetime);

        float eased = 1f - Mathf.Pow(t, fadeCurvePower);
        Color c = startColor;
        c.a = startColor.a * eased;
        sr.color = c;

        transform.localScale = Vector3.Lerp(startScale, startScale * (1f - shrinkAmount), t);

        if (age >= lifetime)
            Destroy(gameObject);
    }
}
