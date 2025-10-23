using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float Speed = 2.0f;
    public float TimeDes = 3.0f;
    private Vector2 waterinput;
    void Start()
    {
        waterinput.x = Input.GetAxisRaw("Horizontal");
        waterinput.y = Input.GetAxisRaw("Vertical");
        waterinput.Normalize();
        Destroy(gameObject, TimeDes);
    }

    // Update is called once per frame
    void Update()
    {
        if (waterinput == Vector2.zero)
        {
            transform.Translate(Vector2.up * Speed * Time.deltaTime);
        }
        else
        {
            transform.Translate(waterinput * Speed * Time.deltaTime);
        }
    }
}
