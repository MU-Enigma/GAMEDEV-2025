using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class GrapplingHook : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;          // Assign in Inspector or will auto-use Camera.main
    public Transform hookHead;         // Assign the HookHead Sprite object
    public Rigidbody2D playerRb;       // Drag your Player's Rb here (from PlayerController.Rb)

    [Header("Settings")]
    public float hookSpeed = 20f;
    public float pullSpeed = 10f;
    public float maxDistance = 10f;
    public LayerMask grappleLayerMask;

    private LineRenderer lr;
    private Vector2 targetPoint;
    private bool isFiring = false;
    private bool isPulling = false;
    private Vector2 fireDirection;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();

        // LineRenderer defaults
        lr.enabled = false;
        lr.positionCount = 0;
        lr.startWidth = 0.06f;
        lr.endWidth = 0.06f;

        if (lr.material == null)
        {
            // Auto-assign a simple material if none given
            lr.material = new Material(Shader.Find("Sprites/Default"));
        }

        if (mainCamera == null)
            mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isFiring && !isPulling)
        {
            FireHook();
        }

        if (Input.GetMouseButtonUp(0))
        {
            ResetHook();
        }

        if (isFiring)
        {
            MoveHookHead();
        }

        if (isPulling)
        {
            PullPlayer();
        }

        if (isFiring || isPulling)
        {
            UpdateRope();
        }
    }

    private void FireHook()
    {
        Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
        targetPoint = mouseWorld;
        fireDirection = (targetPoint - (Vector2)transform.position).normalized;

        hookHead.gameObject.SetActive(true);
        hookHead.position = transform.position;

        isFiring = true;
        lr.enabled = true;
        lr.positionCount = 2;
    }

    private void MoveHookHead()
    {
        hookHead.position = Vector2.MoveTowards(hookHead.position,
                                                hookHead.position + (Vector3)fireDirection,
                                                hookSpeed * Time.deltaTime);

        float dist = Vector2.Distance(transform.position, hookHead.position);
        if (dist > maxDistance)
        {
            ResetHook();
            return;
        }

        // Check hit
        RaycastHit2D hit = Physics2D.Raycast(transform.position, fireDirection, dist, grappleLayerMask);
        if (hit.collider != null)
        {
            hookHead.position = hit.point;
            isFiring = false;
            isPulling = true;
        }
    }

    private void PullPlayer()
    {
        Vector2 dir = ((Vector2)hookHead.position - playerRb.position).normalized;
        playerRb.linearVelocity = dir * pullSpeed;

        float dist = Vector2.Distance(playerRb.position, hookHead.position);
        if (dist < 1f)
        {
            ResetHook();
        }
    }

    private void UpdateRope()
    {
        lr.SetPosition(0, transform.position);
        lr.SetPosition(1, hookHead.position);
    }

    private void ResetHook()
    {
        isFiring = false;
        isPulling = false;
        hookHead.gameObject.SetActive(false);
        lr.enabled = false;
        lr.positionCount = 0;
    }
}
