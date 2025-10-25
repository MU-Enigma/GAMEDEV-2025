using UnityEngine;

public class SpinObject : MonoBehaviour
{
 
    [SerializeField] private float spinSpeed = 720f;

 
    void Update()
    {
        
        transform.Rotate(0, 0, spinSpeed * Time.deltaTime);
    }
}