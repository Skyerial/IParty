// TurfPlayerMover.cs
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(PlayerInput))]
/**
 * @brief Handles player movement, jumping, rotation, and turf-based penalties in Turf mode.
 */
public class TurfPlayerMover : MonoBehaviour
{
    [Header("Movement Settings")]
    /**
     * @brief Base movement speed (units/sec).
     */
    public float moveSpeed = 5f;
    /**
     * @brief Impulse force applied when jumping.
     */
    public float jumpForce = 7f;
    /**
     * @brief Rotation speed (degrees/sec) for smoothing player facing direction.
     */
    public float rotationSpeed = 360f;

    [Header("Ground Check")]
    /**
     * @brief Layers considered as ground for ground checks.
     */
    public LayerMask groundLayerMask;
    /**
     * @brief Distance below the capsule to check for ground contact.
     */
    public float groundCheckOffset = 0.1f;

    [Header("Turf Penalty")]
    /**
     * @brief Distance to check below the player for turf detection.
     */
    public float turfCheckDistance = 1f;
    /**
     * @brief Speed multiplier when not on own turf.
     */
    public float turfSpeedPenalty = 0.5f;
    /**
     * @brief Jump force multiplier when not on own turf.
     */
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

    /**
     * @brief Unity event called when the script instance is loaded; caches components and sets up input actions.
     */
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

    /**
     * @brief Unity event called when the object becomes active; enables input actions.
     */
    void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
    }

    /**
     * @brief Unity event called when the object becomes inactive; disables input actions.
     */
    void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
    }

    /**
     * @brief Unity event called on the first frame; freezes rotations, retrieves player color, and sets paint mask.
     */
    void Start()
    {
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        var pi = GetComponent<PlayerInput>();
        playerColor = TurfGameManager.Instance.GetPlayerColor(pi);
        paintMask   = LayerMask.GetMask("Paint");
    }

    /**
     * @brief Unity event called once per frame; updates animator parameters for running and grounded state.
     */
    void Update()
    {
        animator.SetBool("IsRunning", moveInput.sqrMagnitude > 0.01f);
        animator.SetBool("IsGrounded", IsGrounded());
    }

    /**
     * @brief Unity event called at fixed intervals; applies movement, rotation, jump, and turf penalties.
     */
    void FixedUpdate()
    {
        Vector3 dir = new Vector3(moveInput.x, 0, moveInput.y);
        if (dir.sqrMagnitude > 0.01f)
        {
            Quaternion target = Quaternion.LookRotation(dir.normalized);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation, target, rotationSpeed * Time.fixedDeltaTime);
        }

        bool onOwn = TurfUtils.IsOnOwnTurf(
            transform, playerColor, paintMask, turfCheckDistance);
        float speedMul = onOwn ? 1f : turfSpeedPenalty;
        float jumpMul  = onOwn ? 1f : turfJumpPenalty;
        Vector3 horizontal = (dir.sqrMagnitude > 1f ? dir.normalized : dir)
                             * moveSpeed * speedMul;

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

    /**
     * @brief Checks if the player is grounded by casting a sphere at the base of the capsule collider.
     * @return True if the player is grounded, false otherwise.
     */
    private bool IsGrounded()
    {
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
