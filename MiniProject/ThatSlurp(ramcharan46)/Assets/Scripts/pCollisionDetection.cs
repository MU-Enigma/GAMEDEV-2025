using UnityEngine;

public class pCollisionDetection : MonoBehaviour
{
    public GameObject particle;
    public Transform dmgpoint;

    private void OnParticleCollision(GameObject other)
    {
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Enemy Enter");
            Instantiate(particle, dmgpoint.position, dmgpoint.rotation);
            Destroy(other.gameObject);
        }
    }
}