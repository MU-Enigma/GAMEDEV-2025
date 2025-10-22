using UnityEngine;

public class movementpoookieya : MonoBehaviour
{
    public float speed = 5;
    public float sprintspeed = 7;
    public Rigidbody2D wow;
    public Animator anim;
    public int facdir = 1;

    // Update is called once per frame
    void FixedUpdate()
    {
        // FIX 1: Changed "hor" to "Horizontal" and "ver" to "Vertical"
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        if(horizontal>0 && transform.localScale.x < 0 ||
        horizontal < 0 && transform.localScale.x > 0)
        {
            flip();
        }
        float currentspeed;
        if(Input.GetKey(KeyCode.LeftShift))
        {
            currentspeed=sprintspeed;
        }
        else
        {
            currentspeed=speed;
        }
        

        wow.linearVelocity = new Vector2(horizontal, vertical) * currentspeed;
        anim.SetFloat("horizontal", Mathf.Abs(horizontal * currentspeed));
        anim.SetFloat("vertical", Mathf.Abs(vertical * currentspeed));
        
    }
    void flip()
    {
        facdir *=-1;
        transform.localScale = new Vector3(transform.localScale.x * -1,transform.localScale.y,transform.localScale.z);
    }
}