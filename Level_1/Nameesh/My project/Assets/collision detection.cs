using UnityEngine;

public class collisiondetection : MonoBehaviour
{
    public GameObject blood;
    public Transform bloodPosition;
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("bullet"))
        {
            Debug.Log("Enemy entered");
            Instantiate(blood, bloodPosition.position, bloodPosition.rotation);
        }
    }
}
