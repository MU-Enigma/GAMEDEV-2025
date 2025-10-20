/*

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CrosshairController : MonoBehaviour
{
    public float rotateSpeed = 50f;
    public float lockShrinkScale = 0.6f;
    public float lockDuration = 0.2f;

    private RectTransform rect;
    private Vector3 originalScale;
    private bool locking;

    private Canvas canvas;
    

    void Start(){
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;

        canvas = GetComponentInParent<Canvas>();
        if (canvas == null){
            Debug.LogError("Crosshair must be inside a Canvas!");
        }else{
            canvas.sortingOrder = 999;
        }
    }

    void Update(){
        if (canvas == null) return;
        

        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform,Input.mousePosition,null,out pos);
        rect.anchoredPosition = pos;

        rect.Rotate(0, 0, rotateSpeed * Time.deltaTime);


        if ((Input.GetMouseButtonDown(1)||(Gamepad.current!=null&&Gamepad.current.rightTrigger.wasReleasedThisFrame)) && !locking)
            StartCoroutine(LockEffect());
        if ((Input.GetMouseButtonDown(0)||(Gamepad.current!=null&&Gamepad.current.leftTrigger.wasReleasedThisFrame)) && !locking)
            StartCoroutine(LockEffect());
    }

    System.Collections.IEnumerator LockEffect(){
        locking = true;
        float t = 0f;

        while (t < lockDuration){
            t += Time.deltaTime;
            float p = t / lockDuration;

            rect.localScale = Vector3.Lerp(originalScale, originalScale * lockShrinkScale, p);
            rect.Rotate(0, 0, 500 * Time.deltaTime);
            yield return null;
        }

        rect.localScale = originalScale;
        locking = false;
    }
}
*/

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class CrosshairController : MonoBehaviour
{
    public float rotateSpeed = 50f;
    public float lockShrinkScale = 0.6f;
    public float lockDuration = 0.2f;
    public float joystickSensitivity = 200f;

    private RectTransform rect;
    private Vector3 originalScale;
    private bool locking;
    private Canvas canvas;
    private Vector2 crosshairPosition;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        originalScale = rect.localScale;
        canvas = GetComponentInParent<Canvas>();
        
        if (canvas == null)
        {
            Debug.LogError("Crosshair must be inside a Canvas!");
        }
        else
        {
            canvas.sortingOrder = 999;
        }
        
        crosshairPosition = rect.anchoredPosition;
    }

    void Update()
    {
        if (canvas == null) return;

        if (Gamepad.current != null)
        {
            Vector2 rightStick = Gamepad.current.rightStick.ReadValue();
            crosshairPosition += rightStick * joystickSensitivity * Time.deltaTime;
            
            RectTransform canvasRect = canvas.transform as RectTransform;
            Vector2 canvasSize = canvasRect.sizeDelta;
            crosshairPosition.x = Mathf.Clamp(crosshairPosition.x, -canvasSize.x / 2, canvasSize.x / 2);
            crosshairPosition.y = Mathf.Clamp(crosshairPosition.y, -canvasSize.y / 2, canvasSize.y / 2);
            rect.anchoredPosition = crosshairPosition;
        }
        else
        {
            Vector2 pos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, Input.mousePosition, canvas.worldCamera, out pos);
            rect.anchoredPosition = pos;
            crosshairPosition = pos;
        }

        rect.Rotate(0, 0, rotateSpeed * Time.deltaTime);

        if ((Input.GetMouseButtonDown(1) || (Gamepad.current != null && Gamepad.current.rightTrigger.wasReleasedThisFrame)) && !locking)
        {
            StartCoroutine(LockEffect());
        }
        if ((Input.GetMouseButtonDown(0) || (Gamepad.current != null && Gamepad.current.leftTrigger.wasReleasedThisFrame)) && !locking)
        {
            StartCoroutine(LockEffect());
        }
    }

    System.Collections.IEnumerator LockEffect()
    {
        locking = true;
        float t = 0f;

        while (t < lockDuration)
        {
            t += Time.deltaTime;
            float p = t / lockDuration;

            rect.localScale = Vector3.Lerp(originalScale, originalScale * lockShrinkScale, p);
            rect.Rotate(0, 0, 500 * Time.deltaTime);
            yield return null;
        }

        rect.localScale = originalScale;
        locking = false;
    }
}