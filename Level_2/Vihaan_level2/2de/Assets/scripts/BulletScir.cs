using UnityEngine;

public class Pellet : MonoBehaviour
{
    public float Speed = 0;
    public float DesTime = 0;
    private Vector2 bulletInput; 
    void Start() {
        bulletInput.x = Input.GetAxisRaw("Horizontal");
        bulletInput.y = Input.GetAxisRaw("Vertical");
        bulletInput.Normalize();
        Destroy(gameObject, DesTime);
    }
        void Update() {
        if(bulletInput == Vector2.zero) {
            transform.Translate(Vector2.up * Speed * Time.deltaTime);
        }
        transform.Translate(bulletInput * Speed * Time.deltaTime);
   }
}

