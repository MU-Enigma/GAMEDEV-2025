using UnityEngine;
public class WeaponCollision : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered");
         }
        if (other.CompareTag("Weapons"))
        {
            Debug.Log("HIT!!");
         }
    }
}
