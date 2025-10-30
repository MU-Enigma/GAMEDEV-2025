using UnityEngine;
using UnityEngine.InputSystem;  

public class PlayerMovement : MonoBehaviour
{
    public float moveSpeed = 5f; 

    private Vector2 moveInput;

    void Update()
    {
        moveInput = new Vector2(Keyboard.current.aKey.isPressed ? -1 : Keyboard.current.dKey.isPressed ? 1 : 0,
                                Keyboard.current.wKey.isPressed ? 1 : Keyboard.current.sKey.isPressed ? -1 : 0);
        Vector2 movement = moveInput.normalized;
        transform.Translate(movement * moveSpeed * Time.deltaTime);
        Debug.Log("Player Position: " + transform.position);
    }
}
