using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 1f;

    public GameObject afterImagePrefab;          
    public float afterImageInterval = 0.04f;
    private float afterImageTimer = 0f;
    private SpriteRenderer sr;                

    private Vector2 moveInput;
    private InputSystem_Actions inputActions; 
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    private void Awake()
    {
        Debug.Log("Awake called");
        inputActions = new InputSystem_Actions();

        
        var moveAction = inputActions.Player.Move;
        moveAction.performed += ctx =>
        {
            moveInput = ctx.ReadValue<Vector2>();
            Debug.Log($"Move input: {moveInput} from {ctx.control.device}");
        };
        moveAction.canceled += ctx => moveInput = Vector2.zero;

        var dashAction = inputActions.Player.Sprint; 
        dashAction.performed += ctx =>
        {
            Debug.Log($"Dash button pressed from {ctx.control.device}");
            if (!isDashing && dashCooldownTimer <= 0f && moveInput != Vector2.zero)
            {
                StartCoroutine(Dash());
            }
        };

        sr = GetComponent<SpriteRenderer>();
    }

    private void OnEnable()
    {
        if (inputActions != null)
            inputActions.Player.Enable();
        else
            Debug.LogError("inputActions is null in OnEnable!");
    }

    private void OnDisable()
    {
        inputActions.Player.Disable();
    }

    void Update()
    {
        
        var gamepad = Gamepad.current;
        if (gamepad != null)
        {
            Vector2 stickInput = gamepad.leftStick.ReadValue();
            if (stickInput != Vector2.zero)
            {
                moveInput = stickInput;
                Debug.Log($"Left stick input used for movement: {moveInput}");
            }
           
        }

        if (isDashing)
        {
            dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - Time.deltaTime);
            return; 
        }

        dashCooldownTimer = Mathf.Max(0f, dashCooldownTimer - Time.deltaTime);

        Vector3 move = new Vector3(moveInput.x, moveInput.y, 0f).normalized;
        transform.position += move * speed * Time.deltaTime;

        
        if (moveInput.x > 0.01f)
            sr.flipX = false; 
        else if (moveInput.x < -0.01f)
            sr.flipX = true;  
    }

    IEnumerator Dash()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;
        float elapsed = 0f;

        Vector3 dashDirection = new Vector3(moveInput.x, moveInput.y, 0f).normalized;
        afterImageTimer = 0f;

        while (elapsed < dashDuration)
        {
            transform.position += dashDirection * dashSpeed * Time.deltaTime;
            elapsed += Time.deltaTime;

            afterImageTimer -= Time.deltaTime;
            if (afterImageTimer <= 0f)
            {
                CreateAfterImage();
                afterImageTimer = afterImageInterval;
            }
            yield return null;
        }

        isDashing = false;
    }

    void CreateAfterImage()
    {
        var afterImage = Instantiate(afterImagePrefab, transform.position, transform.rotation);
        var script = afterImage.GetComponent<AfterImage>();
        script.Init(sr.sprite, transform.localScale, transform.rotation);

        // Ensure afterimages respect flipping
        script.GetComponent<SpriteRenderer>().flipX = sr.flipX;
    }
}
