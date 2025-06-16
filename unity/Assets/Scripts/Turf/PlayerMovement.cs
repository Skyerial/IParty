// PlayerMovement.cs
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider), typeof(PlayerInput))]
public class PlayerMovementTurf : MonoBehaviour
{
    [Header("Movement Settings")]
    public float moveSpeed = 5f;
    public float jumpForce = 7f;
    public float rotationSpeed = 360f;

    [Header("Ground Check")]
    public LayerMask groundLayerMask;
    public float groundCheckOffset = 0.1f;

    [Header("Turf Penalty Settings")]
    public float turfCheckDistance = 1f;
    public float turfSpeedPenalty = 0.5f;
    public float turfJumpPenalty = 0.5f;

    private Animator animator;
    private Rigidbody rb;
    private CapsuleCollider capsule;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;

    private Vector2 moveInput;
    private bool jumpPressed;
    private Color playerColor;

    private void Start()
    {
        rb       = GetComponent<Rigidbody>();
        capsule  = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();
        rb.constraints = RigidbodyConstraints.FreezeRotation;

        playerColor = TurfUtilities.GetPlayerColor(transform);

        playerInput = GetComponent<PlayerInput>();
        moveAction  = playerInput.actions.FindAction("Move");
        jumpAction  = playerInput.actions.FindAction("Jump");

        moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled  += _   => moveInput = Vector2.zero;
        jumpAction.performed += _   => jumpPressed = true;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
    }

    private void Update()
    {   
        animator.SetBool("IsRunning", moveInput.sqrMagnitude > 0.001f);
        animator.SetBool("IsGrounded", IsGrounded());
    }

    private void FixedUpdate()
    {
        var onOwnTurf = TurfUtilities.IsOnOwnTurf(transform, playerColor, groundLayerMask, turfCheckDistance);
        var speedMul  = onOwnTurf ? 1f : turfSpeedPenalty;
        var jumpMul   = onOwnTurf ? 1f : turfJumpPenalty;

        var inputDir = new Vector3(moveInput.x, 0f, moveInput.y);
        var finalSpeed = moveSpeed * speedMul;
        var moveVec = (inputDir.sqrMagnitude > 1f ? inputDir.normalized : inputDir) * finalSpeed;

        if (inputDir.sqrMagnitude > 0.001f)
        {
            var targetRot = Quaternion.LookRotation(inputDir.normalized);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime);
        }

        if (IsGrounded())
        {
            rb.linearVelocity = new Vector3(moveVec.x, rb.linearVelocity.y, moveVec.z);
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
        var worldCenter  = transform.TransformPoint(capsule.center);
        var bottomOffset = (capsule.height * 0.5f) - capsule.radius;
        var bottomOrigin = worldCenter + Vector3.down * bottomOffset;
        var radius       = capsule.radius + groundCheckOffset;
        return Physics.CheckSphere(bottomOrigin, radius, groundLayerMask, QueryTriggerInteraction.Ignore);
    }
}
