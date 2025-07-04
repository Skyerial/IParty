// PlayerMovement.cs
// Handles player movement (walking, sprinting), jumping, rotating to face movement direction,
// and a one‐time air dash/dive feature that stays tilted until landing.
// Now also includes a finite stamina system for sprinting, with a world‐space bar above the player.

using UnityEngine;
using UnityEngine.UI;    // Needed for the stamina UI Image
using UnityEngine.InputSystem;
using Unity.VisualScripting;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(PlayerInput))]

/**
 * @brief Handles tank movement, shooting, and ground checks.
 * This script manages the tank's movement speed, rotation, ground detection,
 * and shooting mechanics with a cooldown system.
 */
public class TankMovement : MonoBehaviour
{
    [Header("Movement Settings")]
    [Tooltip("Base movement speed (units/sec).")]
    public float moveSpeed = 5f;

    [Tooltip("Rotation speed (degrees/sec) for smoothing.")]
    public float rotationSpeed = 360f;

    [Header("Ground Check")]
    [Tooltip("Layers considered as ground.")]
    public LayerMask groundLayerMask;

    [Tooltip("How far below the capsule to check for ground.")]
    public float groundCheckOffset = 0.1f;

    [Header("Shooting Settings")]
    [Tooltip("Cooldown (seconds)")]
    public float cooldown = 2;


    [Header("Reload UI (World‐Space)")]
    [Tooltip("Assign a UI Image (with Image Type = Filled, Fill Method = Horizontal) that represents the bar.")]
    public Image reloadBarFillImage;

    [Header("Projectile")]
    public GameObject bulletPrefab;
    public GameObject bulletPoint;
    public GameObject smoke;
    private Rigidbody rb;
    private CapsuleCollider capsule;

    // INPUT via PlayerInput:
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private Vector2 moveInput = Vector2.zero;
    private bool jumpPressed = false;

    // Next time fired:
    private float nextFire;
    private float currentFire;

    /**
     * @brief Initializes components, sets up input actions, and configures Rigidbody constraints.
     */
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        currentFire = Time.time - cooldown;
        nextFire = Time.time;

        // Freeze rotation on X/Z so we only rotate around Y manually
        rb.constraints = RigidbodyConstraints.FreezeRotationX
                       | RigidbodyConstraints.FreezeRotationZ
                       | RigidbodyConstraints.FreezeRotationY;

        // Set up PlayerInput and find actions
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        jumpAction = playerInput.actions.FindAction("Jump");

        // Subscribe to input action callbacks
        moveAction.performed += ctx => moveInput = ctx.ReadValue<Vector2>();
        moveAction.canceled += ctx => moveInput = Vector2.zero;

        jumpAction.performed += ctx => jumpPressed = true;
    }

    /**
     * @brief Enables input actions when the object is enabled.
     * This allows the callbacks to fire when the object is active.
     */
    private void OnEnable()
    {
        // Enable each action so callbacks fire
        moveAction.Enable();
        jumpAction.Enable();
    }

    /**
     * @brief Disables input actions when the object is disabled.
     * This prevents callbacks from firing when the object is inactive.
     */
    private void OnDisable()
    {
        // Disable actions to stop callbacks when object is inactive
        moveAction.Disable();
        jumpAction.Disable();
    }

    /**
     * @brief Updates the stamina bar UI every frame.
     * The fill amount is calculated based on the time since the last fire.
     */
    private void Update()
    {
        // Update the stamina bar UI every frame
        if (reloadBarFillImage != null)
        {
            reloadBarFillImage.fillAmount = (Time.time - currentFire) / cooldown;
        }
    }

    /**
     * @brief FixedUpdate is called at a fixed interval and is used for physics calculations.
     * This method handles movement, rotation, and shooting logic.
     */
    private void FixedUpdate()
    {
        // Build world‐space input direction
        Vector3 inputDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        // Calculate current move speed (account for accelerating sprint)
        Vector3 worldMove = (inputDirection.sqrMagnitude > 1f)
            ? inputDirection.normalized * moveSpeed
            : inputDirection * moveSpeed;

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

        if (isGrounded)
        {
            // On ground: apply horizontal movement and allow jumping
            Vector3 moveDirection = new Vector3(worldMove.x, rb.linearVelocity.y, worldMove.z);

            rb.linearVelocity = moveDirection;

            if (jumpPressed && Time.time > nextFire)
            {
                Shoot();
                nextFire = Time.time + cooldown;
                currentFire = Time.time;
            }
        }

        // Reset jumpPressed for next frame
        jumpPressed = false;
    }

    /**
     * @brief Checks if the tank is grounded by casting a sphere at the bottom of the capsule.
     * This method uses Physics.CheckSphere to determine if there is ground beneath the tank.
     * @return True if grounded, false otherwise.
     */
    private bool IsGrounded()
    {
        Vector3 worldCenter = transform.TransformPoint(capsule.center);
        float bottomOffset = (capsule.height * 0.5f) - capsule.radius;
        Vector3 bottomOrigin = worldCenter + Vector3.down * bottomOffset;
        float sphereRadius = capsule.radius + groundCheckOffset;

        return Physics.CheckSphere(
            bottomOrigin,
            sphereRadius,
            groundLayerMask,
            QueryTriggerInteraction.Ignore
        );
    }

    /**
     * @brief Instantiates a bullet at the bullet point and applies force to it.
     * This method creates a bullet GameObject, applies a forward force to it,
     * and destroys it after a specified time.
     */
    void Shoot()
    {
        GameObject bullet = Instantiate(bulletPrefab, bulletPoint.transform.position, transform.rotation);

        // Smoke effect
        // GameObject trail = Instantiate(smoke, bullet.transform);
        // trail.transform.Rotate(-90f, 0f, 0f);
        // trail.transform.localPosition = Vector3.zero;

        bullet.GetComponent<Rigidbody>().AddForce(transform.forward * 500);
        Destroy(bullet, 10);
    }
}
