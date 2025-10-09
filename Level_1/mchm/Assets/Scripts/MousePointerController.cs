// MousePointerController.cs
using UnityEngine;
/// <summary>
/// also a 1:1 copy of the MousePointerController i was given
/// </summary>

public class MousePointerController : MonoBehaviour
{
    [Header("Pointer Settings")]
    public float pointerSpeed = 10f;
    public float controllerDeadzone = 0.2f;
    public float controllerSwitchDelay = 0.5f;
    
    [Header("Visual Settings")]
    public Color pointerColor = Color.white;
    public float pointerSize = 0.1f;
    
    private Camera mainCamera;
    private GameObject pointerVisual;
    private Vector3 pointerWorldPosition;
    private bool usingController = false;
    private float lastControllerInputTime = 0f;
    private Vector3 lastMousePosition;
    
    public static MousePointerController Instance { get; private set; }
    public Vector3 PointerWorldPosition => pointerWorldPosition;
    public bool UsingController => usingController;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        FindMainCamera();
        CreatePointerVisual();
        UpdateMousePosition();
        lastMousePosition = Input.mousePosition;
    }

    void Update()
    {
        // Ensure we always have a camera reference
        if (mainCamera == null)
        {
            FindMainCamera();
            if (mainCamera == null) return;
        }

        HandlePointerInput();
        UpdatePointerPosition();
        UpdatePointerVisual();
    }

    private void FindMainCamera()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            mainCamera = FindFirstObjectByType<Camera>();
        }
    }

    private void CreatePointerVisual()
    {
        // Create a simple quad for the pointer
        pointerVisual = GameObject.CreatePrimitive(PrimitiveType.Quad);
        pointerVisual.name = "PointerVisual";
        pointerVisual.transform.SetParent(transform);
        pointerVisual.transform.localPosition = Vector3.zero;
        pointerVisual.transform.localScale = Vector3.one * pointerSize;
        
        // Set up the material and color
        Renderer renderer = pointerVisual.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Sprites/Default"));
            material.color = pointerColor;
            renderer.material = material;
        }
        
        // Remove the collider since we don't need it
        Collider collider = pointerVisual.GetComponent<Collider>();
        if (collider != null) Destroy(collider);
    }

    private void HandlePointerInput()
    {
        // Check for controller input
        Vector2 rightStickInput = new Vector2(
            Input.GetAxis("RightStickHorizontal"),
            Input.GetAxis("RightStickVertical")
        );

        bool hasControllerInput = rightStickInput.magnitude > controllerDeadzone;
        bool hasMouseMovement = HasMouseMoved();

        // Update controller input time
        if (hasControllerInput)
        {
            lastControllerInputTime = Time.time;
        }

        // Switch to controller if we recently had controller input
        if (hasControllerInput || Time.time - lastControllerInputTime < controllerSwitchDelay)
        {
            usingController = true;
        }
        // Switch to mouse if mouse is moving and no recent controller input
        else if (hasMouseMovement)
        {
            usingController = false;
        }

        // Handle input based on current mode
        if (usingController && hasControllerInput)
        {
            // Move pointer with right stick
            Vector3 movement = new Vector3(rightStickInput.x, rightStickInput.y, 0) * pointerSpeed * Time.deltaTime;
            pointerWorldPosition += movement;
            ClampPointerToScreen();
        }
        else
        {
            // Use mouse position
            UpdateMousePosition();
        }
    }

    private bool HasMouseMoved()
    {
        Vector3 currentMousePos = Input.mousePosition;
        bool moved = (currentMousePos - lastMousePosition).sqrMagnitude > 0.1f;
        lastMousePosition = currentMousePos;
        return moved;
    }

    private void UpdateMousePosition()
    {
        if (mainCamera != null)
        {
            Vector3 mousePos = Input.mousePosition;
            // Use the camera's distance from the world plane (usually Z=0)
            float distanceToWorld = Mathf.Abs(mainCamera.transform.position.z);
            mousePos.z = distanceToWorld;
            pointerWorldPosition = mainCamera.ScreenToWorldPoint(mousePos);
            pointerWorldPosition.z = 0;
        }
    }

    private void ClampPointerToScreen()
    {
        if (mainCamera == null) return;

        Vector3 viewportPos = mainCamera.WorldToViewportPoint(pointerWorldPosition);
        
        // Add padding to keep pointer fully visible
        float padding = 0.02f;
        viewportPos.x = Mathf.Clamp(viewportPos.x, padding, 1f - padding);
        viewportPos.y = Mathf.Clamp(viewportPos.y, padding, 1f - padding);
        
        pointerWorldPosition = mainCamera.ViewportToWorldPoint(viewportPos);
        pointerWorldPosition.z = 0;
    }

    private void UpdatePointerPosition()
    {
        if (transform != null)
        {
            transform.position = pointerWorldPosition;
        }
    }

    private void UpdatePointerVisual()
    {
        if (pointerVisual != null)
        {
            // Show pointer visual only when using controller
            pointerVisual.SetActive(usingController);
            
            if (usingController)
            {
                // Rotate the pointer visual for animation
                pointerVisual.transform.Rotate(0, 0, 90f * Time.deltaTime);
            }
            else
            {
                // Reset rotation when using mouse
                pointerVisual.transform.rotation = Quaternion.identity;
            }
        }
    }

    // Public methods for external control
    public void SetPointerColor(Color newColor)
    {
        pointerColor = newColor;
        if (pointerVisual != null)
        {
            Renderer renderer = pointerVisual.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = newColor;
            }
        }
    }

    public void SetPointerSize(float newSize)
    {
        pointerSize = newSize;
        if (pointerVisual != null)
        {
            pointerVisual.transform.localScale = Vector3.one * newSize;
        }
    }

    public void ForceControllerMode(bool useController)
    {
        usingController = useController;
        if (useController)
        {
            lastControllerInputTime = Time.time;
        }
    }

    // Handle scene changes
    void OnLevelWasLoaded(int level)
    {
        FindMainCamera();
    }
}