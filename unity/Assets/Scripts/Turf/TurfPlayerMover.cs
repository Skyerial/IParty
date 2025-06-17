using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(PlayerInput))]
public class TurfPlayerMover : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float rotationSpeed = 360f;

    [Header("Ground Check")]
    public LayerMask groundLayerMask;
    public float groundCheckOffset = 0.1f;

    [Header("Turf Penalty")]
    public float turfCheckDistance = 1f;
    public float turfSpeedPenalty = 0.5f;
    public float turfJumpPenalty  = 0.5f;

    private Rigidbody rb;
    private CapsuleCollider cap;
    private Animator animator;

    private InputAction moveAction;
    private InputAction jumpAction;

    private Vector2 moveInput;
    private bool    jumpPressed;

    private Color     playerColor;
    private LayerMask paintMask;

    void Awake()
    {
        rb       = GetComponent<Rigidbody>();
        cap      = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();

        // get InputActions once
        var pi = GetComponent<PlayerInput>();
        moveAction = pi.actions["Move"];
        jumpAction = pi.actions["Jump"];
        moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled  += _   => moveInput = Vector2.zero;
        jumpAction.performed += _   => jumpPressed = true;
    }

    void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
    }

    void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
    }

    void Start()
    {
        // freeze rotations, keep physics-driven movement
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        // look up this player’s color and paint-layer mask
        var dev = GetComponent<PlayerInput>().devices[0];
        playerColor = PlayerManager.findColor(dev).color;
        paintMask   = LayerMask.GetMask("Paint"); // adjust to your paint layer name
    }

    void Update()
    {
        animator.SetBool("IsRunning", moveInput.sqrMagnitude > 0.01f);
        animator.SetBool("IsGrounded", IsGrounded());
    }

    void FixedUpdate()
    {
        // handle facing direction
        Vector3 dir = new Vector3(moveInput.x, 0, moveInput.y);
        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, target, rotationSpeed * Time.fixedDeltaTime);
        }

        // compute turf penalties
        bool onOwn = TurfUtils.IsOnOwnTurf(
            transform, playerColor, paintMask, turfCheckDistance);
        float speedMul = onOwn ? 1f : turfSpeedPenalty;
        float jumpMul  = onOwn ? 1f : turfJumpPenalty;
        Vector3 horizontal = (dir.sqrMagnitude > 1f ? dir.normalized : dir)
                             * moveSpeed * speedMul;

        // **only apply horizontal & jump when grounded** (original behavior)
        if (IsGrounded())
        {
            rb.linearVelocity = new Vector3(
                horizontal.x,
                rb.linearVelocity.y,
                horizontal.z
            );

            if (jumpPressed)
            {
                animator.SetTrigger("Jump");
                rb.AddForce(Vector3.up * (jumpForce * jumpMul), ForceMode.Impulse);
            }
        }

        jumpPressed = false;
    }

    private bool IsGrounded()
    {
        // compute sphere‐check at bottom of capsule
        Vector3 worldCenter = transform.TransformPoint(cap.center);
        float bottomOffset = (cap.height * 0.5f) - cap.radius;
        Vector3 origin = worldCenter + Vector3.down * bottomOffset;
        float radius = cap.radius + groundCheckOffset;

        return Physics.CheckSphere(
            origin, 
            radius, 
            groundLayerMask, 
            QueryTriggerInteraction.Ignore
        );
    }
}
