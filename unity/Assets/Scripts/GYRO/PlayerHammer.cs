using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerHammer : MonoBehaviour
{
    [Header("Hammer Settings")]
    public Transform hammer;              // Reference to the actual hammer Transform (e.g., Hammer03 child object)
    public float slamSpeed = 500f;        // Rotation speed for slamming
    public float maxSlamAngle = 130f;     // How far down the hammer should rotate in degrees
    public LayerMask moleLayer;           // LayerMask to detect moles only (assign "Mole" layer in inspector)

    private Quaternion originalRotation;  // The hammer's default rotation to reset to after slam
    private bool isSlamming = false;      // Lock to prevent double slam
    private bool moleWasHit = false;      // Flag to track if a mole was hit during this slam

    private PlayerInput playerInput;      // Input System reference
    private InputAction slamAction;       // Attack action (spacebar or button)

    void Awake()
    {
        // Setup input system and hook into Attack action
        playerInput = GetComponent<PlayerInput>();
        slamAction = playerInput.actions.FindAction("Attack");

        if (slamAction != null)
            slamAction.performed += ctx => TrySlam();
        else
            Debug.LogError("❌ No 'Attack' action found in InputActions.");
    }

    void Start()
    {
        // Cache original hammer rotation
        originalRotation = hammer.localRotation;
    }

    void OnEnable() => slamAction?.Enable();
    void OnDisable() => slamAction?.Disable();

    private void TrySlam()
    {
        if (!isSlamming)
        {
            Debug.Log("🔨 Slam triggered!");
            StartCoroutine(Slam());
        }
    }

    private System.Collections.IEnumerator Slam()
    {
        isSlamming = true;
        moleWasHit = false;

        Quaternion targetRotation = originalRotation * Quaternion.Euler(maxSlamAngle, 0f, 0f);

        // --- Slam phase ---
        while (!moleWasHit && Quaternion.Angle(hammer.localRotation, targetRotation) > 0.5f)
        {
            hammer.localRotation = Quaternion.RotateTowards(
                hammer.localRotation,
                targetRotation,
                slamSpeed * Time.deltaTime
            );

            yield return null;
        }

        if (!moleWasHit)
        {
            Debug.LogWarning("❌ Slam finished but hit NOTHING. Make sure colliders and layers are correct.");
        }

        // --- Return phase ---
        yield return new WaitForSeconds(0.1f);

        float t = 0f;
        float returnDuration = 0.1f;
        Quaternion downRot = hammer.localRotation;

        while (t < returnDuration)
        {
            hammer.localRotation = Quaternion.Slerp(downRot, originalRotation, t / returnDuration);
            t += Time.deltaTime;
            yield return null;
        }

        hammer.localRotation = originalRotation;
        isSlamming = false;
    }

    // ✅ Trigger detection (ensure hammer has a Trigger Collider and Rigidbody)
    private void OnTriggerEnter(Collider other)
    {
        if (!isSlamming) return;

        if (((1 << other.gameObject.layer) & moleLayer) != 0)
        {
            Debug.Log("🎯 Mole hit via trigger: " + other.name);
            moleWasHit = true;
        }
    }
}
