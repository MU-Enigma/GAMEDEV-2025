using UnityEngine; 
public class SwordController : MonoBehaviour 
{ 
  private Animator animator; 
   
  void Start() 
  { 
      animator = GetComponent<Animator>(); 
  } 
   
  void Update() 
  { 
      if (Input.GetMouseButtonDown(0)) 
      { 
          animator.SetTrigger("swing");
      } 
  } 
}