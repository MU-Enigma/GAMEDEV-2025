using UnityEngine;

public class StarSpin : MonoBehaviour
{
   
    public float spinSpeed = 2000f;

    void Start()
    {
       
        Rigidbody rb = GetComponent<Rigidbody>();

      

        if (rb != null)
        {
          
            rb.AddTorque(Vector3.forward * spinSpeed);

          
        }
    }
}