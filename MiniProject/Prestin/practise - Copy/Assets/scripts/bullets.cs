using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour

{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public float Speed = 2.0f;
    public float TimeDes = 0.5f;
    public Vector2 bulletinput;
    void Start()
    {
        bulletinput.x = Input.GetAxisRaw("Horizontal");
        bulletinput.y = Input.GetAxisRaw("Vertical");
        bulletinput.Normalize();
        Destroy(gameObject, TimeDes);

    }

    // Update is called once per frame
    void Update()
    {
        if (bulletinput == Vector2.zero)
        {
            transform.Translate(Vector2.up * Speed * Time.deltaTime);
        }
        transform.Translate(bulletinput *Speed* Time.deltaTime);
    }
}
