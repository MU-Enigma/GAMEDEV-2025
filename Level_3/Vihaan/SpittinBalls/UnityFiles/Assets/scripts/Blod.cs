using UnityEngine;

public class Blod : MonoBehaviour
{

    public float LifeTime = 1;
    void Update()
    {
        Destroy(gameObject, LifeTime);
    }
}
