using UnityEngine;
using UnityEngine.UI;    // Needed for the stamina UI Image
using UnityEngine.InputSystem;  // Added for PlayerInput and InputAction

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(PlayerInput))]
public class PlayerMovementSpleef : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base movement speed (units/sec).")]
    public float moveSpeed = 5f;

    [Tooltip("Multiplier applied to moveSpeed when sprinting.")]
    public float sprintMultiplier = 1.5f;

    [Tooltip("Time in seconds for sprint to reach full speed.")]
    public float sprintRampUpTime = 2f;

    [Tooltip("Jump impulse force.")]
    public float jumpForce = 7f;

    [Tooltip("Rotation speed (degrees/sec) for smoothing.")]
    public float rotationSpeed = 360f;

    [Header("Ground Check")]
    [Tooltip("Layers considered as ground.")]
    public LayerMask groundLayerMask;

    [Tooltip("How far below the capsule to check for ground.")]
    public float groundCheckOffset = 0.1f;

    [Header("Dash (Dive) Settings")]
    [Tooltip("Speed of the dash (units/sec).")]
    public float dashSpeed = 15f;

    [Tooltip("Duration of the dash in seconds.")]
    public float dashDuration = 0.2f;

    [Tooltip("When dashing in the air, what fraction of dashSpeed is applied upward.")]
    [Range(0f, 1f)]
    public float upwardDashFactor = 0.3f;

    [Header("Stamina Settings")]
    [Tooltip("Maximum stamina value.")]
    public float maxStamina = 100f;

    [Tooltip("How many stamina points drain per second while sprinting.")]
    public float staminaDrainRate = 25f;

    [Tooltip("How many stamina points regenerate per second when not sprinting.")]
    public float staminaRegenRate = 15f;

    [Tooltip("How many stamina points are used per dash/dive.")]
    public float dashStaminaCost = 20f;

    [Tooltip("Time (sec) to wait after sprinting or dashing before stamina regeneration begins.")]
    public float staminaRegenDelay = 1f;

    [Header("Stamina UI (World‐Space)")]
    [Tooltip("Assign a UI Image (with Image Type = Filled, Fill Method = Horizontal) that represents the bar.")]
    public Image staminaBarFillImage;

    private Animator animator;
    private Rigidbody rb;
    private CapsuleCollider capsule;

    // INPUT via PlayerInput:
    private PlayerInput    playerInput;
    private InputAction    moveAction;
    private InputAction    jumpAction;
    private InputAction    sprintAction;

    private Vector2 moveInput     = Vector2.zero;
    private bool    jumpPressed   = false;
    private bool    sprintPressed = false;

    // Dash state:
    private bool    isDashing     = false;
    private float   dashTimeLeft  = 0f;
    private Vector3 dashDirection = Vector3.zero;
    private bool    hasDashed     = false;

    // Stamina state:
    private float currentStamina;
    private float regenDelayTimer = 0f;
    private bool  isSprintingAllowed => currentStamina > 0f;
    private bool  canDash          => currentStamina >= dashStaminaCost;

    // Sprint acceleration state:
    private float currentSprintMultiplier = 1f;
    private float sprintAccelerationRate;

    private void Awake()
    {
        rb      = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        animator = GetComponent<Animator>();

        // Freeze rotation on X/Z so we only rotate around Y manually
        rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationY
                       | RigidbodyConstraints.FreezeRotationZ;

        // Initialize stamina
        currentStamina = maxStamina;
        regenDelayTimer = 0f;

        // Calculate how fast we interpolate from 1 -> sprintMultiplier over sprintRampUpTime
        sprintAccelerationRate = (sprintMultiplier - 1f) / sprintRampUpTime;

        // Set up PlayerInput and find actions
        playerInput = GetComponent<PlayerInput>();
        moveAction  = playerInput.actions.FindAction("Move"); 
        jumpAction  = playerInput.actions.FindAction("Jump");
        sprintAction= playerInput.actions.FindAction("Sprint");

        // Subscribe to input action callbacks
        moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled  += ctx => moveInput = Vector2.zero;

        jumpAction.performed += ctx => jumpPressed = true;

        sprintAction.performed += ctx => sprintPressed = true;
        sprintAction.canceled  += ctx => sprintPressed = false;
    }

    private void OnEnable()
    {
        // Enable each action so callbacks fire
        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
    }

    private void OnDisable()
    {
        // Disable actions to stop callbacks when object is inactive
        moveAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
    }

    private void Update()
    {
        // Handle stamina regeneration each frame
        HandleStamina();

        // Update the stamina bar UI every frame
        if (staminaBarFillImage != null)
        {
            staminaBarFillImage.fillAmount = currentStamina / maxStamina;
        }

        bool isMoving = moveInput.sqrMagnitude > 0.001f;
        animator.SetBool("IsRunning", isMoving);

        // hook up grounded state for looping dive animation
        bool isGrounded = IsGrounded();
        animator.SetBool("IsGrounded", isGrounded);
    }

    private void FixedUpdate()
    {
        // If we are currently in a dash, handle dash movement & tilt until grounded
        if (isDashing)
        {
            HandleDashing();
            jumpPressed = false;
            return;
        }

        // Otherwise, normal movement:

        // Build world‐space input direction
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        // Determine if we can sprint (enough stamina, moving, and holding sprint)
        bool wantToSprint = sprintPressed && inputDirection.sqrMagnitude > 0.01f && isSprintingAllowed;

        // Handle sprint acceleration
        if (wantToSprint)
        {
            currentSprintMultiplier += sprintAccelerationRate * Time.fixedDeltaTime;
            currentSprintMultiplier = Mathf.Min(currentSprintMultiplier, sprintMultiplier);
        }
        else
        {
            currentSprintMultiplier = 1f;
        }

        // Calculate current move speed (account for accelerating sprint)
        float currentSpeed = moveSpeed * currentSprintMultiplier;
        Vector3 worldMove = (inputDirection.sqrMagnitude > 1f)
            ? inputDirection.normalized * currentSpeed
            : inputDirection * currentSpeed;

        // Smoothly rotate toward movement direction (if any)
        if (inputDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(inputDirection.normalized);
            transform.rotation = Quaternion.RotateTowards(
                transform.rotation,
                targetRot,
                rotationSpeed * Time.fixedDeltaTime
            );
        }

        // Ground check
        bool isGrounded = IsGrounded();

        // Reset dash availability when grounded
        if (isGrounded)
        {
            hasDashed = false;
        }

        if (isGrounded)
        {
            // On ground: apply horizontal movement and allow jumping
            rb.linearVelocity = new Vector3(worldMove.x, rb.linearVelocity.y, worldMove.z);

            if (jumpPressed)
            {
                animator.SetTrigger("Jump");
                rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            }
        }
        else
        {
            // In air: preserve existing velocity
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, rb.linearVelocity.y, rb.linearVelocity.z);

            // If Jump was pressed while airborne, attempt dash instead of jump
            if (jumpPressed)
            {
                animator.SetTrigger("Dash");
                TryStartDash();
            }
        }

        // Reset jumpPressed for next frame
        jumpPressed = false;

        // Drain stamina if we are sprinting this FixedUpdate
        if (wantToSprint)
        {
            float drainThisFrame = staminaDrainRate * Time.fixedDeltaTime;
            currentStamina = Mathf.Max(0f, currentStamina - drainThisFrame);
            regenDelayTimer = staminaRegenDelay;
        }
    }

    private void HandleStamina()
    {
        // Decrease the regen delay timer if it's above zero
        if (regenDelayTimer > 0f)
        {
            regenDelayTimer -= Time.deltaTime;
            if (regenDelayTimer < 0f)
                regenDelayTimer = 0f;
        }

        // Determine if the player is actively sprinting right now
        bool movingHorizontally = moveInput.sqrMagnitude > 0.01f;
        bool actuallySprinting = sprintPressed && movingHorizontally && isSprintingAllowed;

        // If we are NOT sprinting AND regen delay has elapsed, regenerate stamina
        if (!actuallySprinting && regenDelayTimer <= 0f)
        {
            currentStamina += staminaRegenRate * Time.deltaTime;
            if (currentStamina > maxStamina)
                currentStamina = maxStamina;
        }

        // Once stamina is zero, sprinting is disabled until regen
        // (The isSprintingAllowed property will reflect that.)
    }

    private void TryStartDash()
    {
        // Only allow a dash if:
        // - player is in the air (not grounded)
        // - player hasn’t dashed yet since last landing
        // - not already mid-dash
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

        // Deduct stamina for dashing
        currentStamina = Mathf.Max(0f, currentStamina - dashStaminaCost);
        regenDelayTimer = staminaRegenDelay;

        // Determine dash direction: forward + small upward component
        Vector3 forward = transform.forward.normalized;
        dashDirection = (forward + Vector3.up * upwardDashFactor).normalized;

        // Immediately set velocity so dash “feels” instant:
        rb.linearVelocity = dashDirection * dashSpeed;
    }

    private void HandleDashing()
    {
        // If dashDuration not over, continue overriding velocity:
        if (dashTimeLeft > 0f)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            dashTimeLeft -= Time.fixedDeltaTime;
        }

        // stop dash when grounded
        if (IsGrounded())
        {
            isDashing = false;
        }
    }

    private bool IsGrounded()
    {
        Vector3 worldCenter   = transform.TransformPoint(capsule.center);
        float   bottomOffset  = (capsule.height * 0.5f) - capsule.radius;
        Vector3 bottomOrigin  = worldCenter + Vector3.down * bottomOffset;
        float   sphereRadius  = capsule.radius + groundCheckOffset;

        return Physics.CheckSphere(
            bottomOrigin,
            sphereRadius,
            groundLayerMask,
            QueryTriggerInteraction.Ignore
        );
    }
}
