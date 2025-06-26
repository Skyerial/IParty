// SpleefPlayerMover.cs
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(PlayerInput))]
/**
 * @brief Handles player movement, sprinting, jumping, dashing, and stamina management in Spleef.
 */
public class SpleefPlayerMover : MonoBehaviour
{
    [Header("Movement Settings")]
    /**
     * @brief Base movement speed (units/sec).
     */
    [Tooltip("Base movement speed (units/sec).")]
    public float moveSpeed = 5f;

    /**
     * @brief Multiplier applied to moveSpeed when sprinting.
     */
    [Tooltip("Multiplier applied to moveSpeed when sprinting.")]
    public float sprintMultiplier = 1.5f;

    /**
     * @brief Time in seconds for sprint to reach full speed.
     */
    [Tooltip("Time in seconds for sprint to reach full speed.")]
    public float sprintRampUpTime = 2f;

    /**
     * @brief Impulse force applied when jumping.
     */
    [Tooltip("Jump impulse force.")]
    public float jumpForce = 7f;

    /**
     * @brief Rotation speed (degrees/sec) for smoothing player facing direction.
     */
    [Tooltip("Rotation speed (degrees/sec) for smoothing.")]
    public float rotationSpeed = 360f;

    [Header("Ground Check")]
    /**
     * @brief Layers considered as ground for ground checks.
     */
    [Tooltip("Layers considered as ground.")]
    public LayerMask groundLayerMask;

    /**
     * @brief Distance below the capsule to check for ground contact.
     */
    [Tooltip("How far below the capsule to check for ground.")]
    public float groundCheckOffset = 0.1f;

    [Header("Dash (Dive) Settings")]
    /**
     * @brief Speed of the aerial dash (units/sec).
     */
    [Tooltip("Speed of the dash (units/sec).")]
    public float dashSpeed = 15f;

    /**
     * @brief Duration (seconds) of the dash.
     */
    [Tooltip("Duration of the dash in seconds.")]
    public float dashDuration = 0.2f;

    /**
     * @brief Fraction of dashSpeed applied upward when dashing in air.
     */
    [Tooltip("When dashing in the air, what fraction of dashSpeed is applied upward.")]
    [Range(0f, 1f)]
    public float upwardDashFactor = 0.3f;

    [Header("Stamina Settings")]
    /**
     * @brief Maximum stamina value.
     */
    [Tooltip("Maximum stamina value.")]
    public float maxStamina = 100f;

    /**
     * @brief Stamina points drained per second while sprinting.
     */
    [Tooltip("How many stamina points drain per second while sprinting.")]
    public float staminaDrainRate = 25f;

    /**
     * @brief Stamina points regenerated per second when not sprinting.
     */
    [Tooltip("How many stamina points regenerate per second when not sprinting.")]
    public float staminaRegenRate = 15f;

    /**
     * @brief Stamina cost per dash/dive.
     */
    [Tooltip("How many stamina points are used per dash/dive.")]
    public float dashStaminaCost = 20f;

    /**
     * @brief Delay (seconds) after sprinting or dashing before stamina regeneration begins.
     */
    [Tooltip("Time (sec) to wait after sprinting or dashing before stamina regeneration begins.")]
    public float staminaRegenDelay = 1f;

    [Header("Stamina UI (World‐Space)")]
    /**
     * @brief UI Image (FilledHorizontal) representing the stamina bar fill.
     */
    [Tooltip("Assign a UI Image (with Image Type = Filled, Fill Method = Horizontal) that represents the bar.")]
    public Image staminaBarFillImage;

    private Animator animator;
    private Rigidbody rb;
    private CapsuleCollider capsule;

    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction sprintAction;

    private Vector2 moveInput     = Vector2.zero;
    private bool    jumpPressed   = false;
    private bool    sprintPressed = false;

    private bool    isDashing     = false;
    private float   dashTimeLeft  = 0f;
    private Vector3 dashDirection = Vector3.zero;
    private bool    hasDashed     = false;

    private float currentStamina;
    private float regenDelayTimer = 0f;
    private bool  isSprintingAllowed => currentStamina > 0f;
    private bool  canDash          => currentStamina >= dashStaminaCost;

    private float currentSprintMultiplier = 1f;
    private float sprintAccelerationRate;

    /**
     * @brief Unity event called when the script instance is loaded; initializes components, constraints, input actions, and stamina.
     */
    private void Awake()
    {
        rb      = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();

        rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY
                       | RigidbodyConstraints.FreezeRotationZ;

        currentStamina = maxStamina;
        regenDelayTimer = 0f;

        sprintAccelerationRate = (sprintMultiplier - 1f) / sprintRampUpTime;

        playerInput = GetComponent<PlayerInput>();
        moveAction  = playerInput.actions.FindAction("Move"); 
        jumpAction  = playerInput.actions.FindAction("Jump");
        sprintAction= playerInput.actions.FindAction("Sprint");

        moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled  += ctx => moveInput = Vector2.zero;

        jumpAction.performed += ctx => jumpPressed = true;

        sprintAction.performed += ctx => sprintPressed = true;
        sprintAction.canceled  += ctx => sprintPressed = false;
    }

    /**
     * @brief Unity event called when object becomes active; enables input actions so callbacks fire.
     */
    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
    }

    /**
     * @brief Unity event called when object becomes inactive; disables input actions to stop callbacks.
     */
    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
    }

    /**
     * @brief Unity event called once per frame; handles stamina regeneration, updates animator and UI, and sets grounded state.
     */
    private void Update()
    {
        HandleStamina();

        if (staminaBarFillImage != null)
            staminaBarFillImage.fillAmount = currentStamina / maxStamina;

        bool isMoving = moveInput.sqrMagnitude > 0.001f;
        animator.SetBool("IsRunning", isMoving);

        bool isGrounded = IsGrounded();
        animator.SetBool("IsGrounded", isGrounded);
    }

    /**
     * @brief Unity event called at fixed intervals; processes movement, sprint, jump, and dash logic including stamina drain.
     */
    private void FixedUpdate()
    {
        if (isDashing)
        {
            HandleDashing();
            jumpPressed = false;
            return;
        }

        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);
        bool wantToSprint = sprintPressed && inputDirection.sqrMagnitude > 0.01f && isSprintingAllowed;

        if (wantToSprint)
        {
            currentSprintMultiplier += sprintAccelerationRate * Time.fixedDeltaTime;
            currentSprintMultiplier = Mathf.Min(currentSprintMultiplier, sprintMultiplier);
        }
        else
        {
            currentSprintMultiplier = 1f;
        }

        float currentSpeed = moveSpeed * currentSprintMultiplier;
        Vector3 worldMove = (inputDirection.sqrMagnitude > 1f)
            ? inputDirection.normalized * currentSpeed
            : inputDirection * currentSpeed;

        if (inputDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDirection.normalized);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.fixedDeltaTime
            );
        }

        bool isGrounded = IsGrounded();

        if (isGrounded)
            hasDashed = false;

        if (isGrounded)
        {
            rb.linearVelocity = new Vector3(worldMove.x, rb.linearVelocity.y, worldMove.z);

            if (jumpPressed)
            {
                animator.SetTrigger("Jump");
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
        else
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, rb.linearVelocity.z);

            if (jumpPressed)
            {
                animator.SetTrigger("Dash");
                TryStartDash();
            }
        }

        jumpPressed = false;

        if (wantToSprint)
        {
            float drainThisFrame = staminaDrainRate * Time.fixedDeltaTime;
            currentStamina = Mathf.Max(0f, currentStamina - drainThisFrame);
            regenDelayTimer = staminaRegenDelay;
        }
    }

    /**
     * @brief Processes stamina regeneration and delay timer each frame.
     */
    private void HandleStamina()
    {
        if (regenDelayTimer > 0f)
        {
            regenDelayTimer -= Time.deltaTime;
            if (regenDelayTimer < 0f)
                regenDelayTimer = 0f;
        }

        bool movingHorizontally = moveInput.sqrMagnitude > 0.01f;
        bool actuallySprinting = sprintPressed && movingHorizontally && isSprintingAllowed;

        if (!actuallySprinting && regenDelayTimer <= 0f)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina)
                currentStamina = maxStamina;
        }
    }

    /**
     * @brief Attempts to start an aerial dash if conditions are met (airborne, able to dash, not already dashing).
     */
    private void TryStartDash()
    {
        if (IsGrounded() || hasDashed || isDashing)
        {
            return;
        }

        if (!canDash)
        {
            return;
        }

        isDashing    = true;
        dashTimeLeft = dashDuration;
        hasDashed    = true;

        currentStamina = Mathf.Max(0f, currentStamina - dashStaminaCost);
        regenDelayTimer = staminaRegenDelay;

        Vector3 forward = transform.forward.normalized;
        dashDirection = (forward + Vector3.up * upwardDashFactor).normalized;

        rb.linearVelocity = dashDirection * dashSpeed;
    }

    /**
     * @brief Handles ongoing dash movement and ends dash when grounded.
     */
    private void HandleDashing()
    {
        if (dashTimeLeft > 0f)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            dashTimeLeft -= Time.fixedDeltaTime;
        }

        if (IsGrounded())
            isDashing = false;
    }

    /**
     * @brief Checks if the player is grounded by casting a sphere at the base of the capsule collider.
     * @return True if the player is grounded, false otherwise.
     */
    private bool IsGrounded()
    {
        Vector3 worldCenter  = transform.TransformPoint(capsule.center);
        float   bottomOffset = (capsule.height * 0.5f) - capsule.radius;
        Vector3 bottomOrigin = worldCenter + Vector3.down * bottomOffset;
        float   sphereRadius = capsule.radius + groundCheckOffset;

        return Physics.CheckSphere(
            bottomOrigin,
            sphereRadius,
            groundLayerMask,
            QueryTriggerInteraction.Ignore
        );
    }
}
