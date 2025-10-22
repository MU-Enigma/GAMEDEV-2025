using UnityEngine;

public class swordHit : MonoBehaviour
{
    public Rigidbody2D weapon;
    public float rotationAngle = 30f;
    public float swingSpeed = 5f;
    public float returnSpeed = 5f;
    private Quaternion initialRotation;
    private Quaternion targetRotation;
    bool isSwinging;
    void Start()
    {
        initialRotation = transform.localRotation;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0) && !isSwinging)
        {
            targetRotation = Quaternion.Euler(0, 0, rotationAngle);
            isSwinging = true;
        }

        if (isSwinging)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * swingSpeed);

            if(Quaternion.Angle(transform.localRotation, targetRotation) < 1f)
            {
                isSwinging = false;
            }
        } else
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, initialRotation, Time.deltaTime * returnSpeed);
        }
        
    }
}
