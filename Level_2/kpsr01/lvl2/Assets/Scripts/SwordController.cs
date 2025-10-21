using UnityEngine;
public class SwordController : MonoBehaviour
{
private Animator animator;
private bool isSwinging = false;
private float swingTimer = 0f;
void Start()
{
 animator = GetComponent<Animator>();
}
void Update()
{
 // Check for mouse click
 if (Input.GetMouseButtonDown(0) && !isSwinging)
 {
 // Start swinging
 isSwinging = true;
 animator.SetBool("IsSwinging", true);
 swingTimer = 1f; // 1 second swing duration
 }

 // Count down the swing timer
 if (isSwinging)
 {
 swingTimer -= Time.deltaTime;

 if (swingTimer <= 0f)
 {
 // Stop swinging
 isSwinging = false;
 animator.SetBool("IsSwinging", false);
 }
 }
}
}
