using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/**
 * @brief Controls the weapon slamming mechanics and collision interactions with game objects (e.g., Mole, Bomb, OilBarrel).
 */
[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class WeaponController : MonoBehaviour
{
    [Header("Slam Settings")]

    /**
     * @brief Total duration for a slam action (down and up phases combined).
     */
    [Tooltip("Total slam duration (down + up) in seconds")]
    public float slamDuration = 0.3f;

    /**
     * @brief Factor to slow the return if the slam missed a target.
     */
    [Tooltip("Multiplier to slow down the return after a miss (1 = normal speed, 2 = twice as slow)")]
    public float missReturnMultiplier = 2f;

    /**
     * @brief Downward angle for the slam (X-axis).
     */
    [Tooltip("Slam angle X (downward)")]
    public float slamAngleX = 60f;

    /**
     * @brief Side angle for the slam (Z-axis).
     */
    [Tooltip("Slam angle Z (sideways)")]
    public float slamAngleZ = -25f;

    /**
     * @brief Reference to the player's camera for visual effects.
     */
    [Tooltip("for the dizzy effect")]
    public Transform playerCamera;

    /**
     * @brief Duration of the dizzy screen effect.
     */
    public float dizzyDuration = 2f;

    /**
     * @brief Intensity of the dizzy effect.
     */
    public float dizzyIntensity = 1f;

    PlayerInput playerInput;
    InputAction slamAction;

    bool isSlamming = false;
    bool hasHit = false;
    Quaternion startRot, downRot;

    /**
     * @brief Initializes rigidbody and constraints on awake.
     */
    void Awake()
    {
        // playerInput = GetComponent<PlayerInput>();
        // slamAction = playerInput.actions.FindAction("Jump");
        var rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints
                         .FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ
                         | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    // void OnEnable() => slamAction.Enable();
    // void OnDisable() => slamAction.Disable();

    // void Update()
    // {
    //     if (slamAction.triggered && !isSlamming)
    //         StartCoroutine(SlamRoutine());
    // }

    /**
     * @brief Public method to trigger the slam manually.
     */
    public void Slam()
    {
        if (!isSlamming)
            StartCoroutine(SlamRoutine());
    }

    /**
     * @brief Coroutine that animates the slam motion down and up.
     */
    IEnumerator SlamRoutine()
    {
        isSlamming = true;
        hasHit = false;

        startRot = transform.localRotation;
        downRot = startRot * Quaternion.Euler(slamAngleX, 0f, slamAngleZ);

        float halfTime = slamDuration * 0.5f;
        float t = 0f;

        // Slam down
        while (t < halfTime && !hasHit)
        {
            transform.localRotation = Quaternion.Slerp(startRot, downRot, t / halfTime);
            t += Time.deltaTime;
            yield return null;
        }

        if (!hasHit)
            transform.localRotation = downRot;

        // Return up, slower if miss
        t = 0f;
        Quaternion fromRot = transform.localRotation;
        float returnTime = hasHit ? halfTime : halfTime * missReturnMultiplier;

        while (t < returnTime)
        {
            transform.localRotation = Quaternion.Slerp(fromRot, startRot, t / returnTime);
            t += Time.deltaTime;
            yield return null;
        }

        transform.localRotation = startRot;
        isSlamming = false;
    }

    /**
     * @brief Trigger callback to detect collisions with game objects.
     * @param other The collider the weapon enters.
     */
    void OnTriggerEnter(Collider other)
    {
        if (!isSlamming || hasHit) return;

        PlayerInput player = GetComponentInParent<PlayerInput>();

        if (other.CompareTag("Mole"))
        {
            hasHit = true;
            var mole = other.GetComponent<Mole>();
            mole?.OnHit();
            if (player != null)
                GameManagerGyro.Instance.AddMoleHit(player);
        }
        else if (other.CompareTag("Bomb"))
        {
            hasHit = true;
            var Bomb = other.GetComponent<BombGyro>();
            Bomb?.OnHit();

            if (player != null)
                GameManagerGyro.Instance.RemoveMoleHit(player);
            StartCoroutine(BombEffect());
        }
        else if (other.CompareTag("OilBarrel"))
        {
            hasHit = true;
            var barrel = other.GetComponent<OilBarrel>();
            barrel?.OnHit();
        }
    }

    /**
     * @brief Coroutine for dizzy visual feedback after hitting a bomb.
     */
    IEnumerator BombEffect()
    {
        Vector3 originalPosition = playerCamera.localPosition;
        Quaternion originalRotation = playerCamera.localRotation;

        float elapsed = 0f;

        while (elapsed < dizzyDuration)
        {
            float fade = 1f - (elapsed / dizzyDuration);

            float x = Mathf.Sin(Time.time * 10f) * 0.05f * dizzyIntensity * fade;
            float y = Mathf.Cos(Time.time * 12f) * 0.05f * dizzyIntensity * fade;

            float zRot = Mathf.Sin(Time.time * 6f) * 5f * fade;
            playerCamera.localPosition = originalPosition + new Vector3(x, y, 0);
            playerCamera.localRotation = originalRotation * Quaternion.Euler(0, 0, zRot);

            elapsed += Time.deltaTime;
            yield return null;
        }

        float smoothTime = 0.2f;
        float t = 0f;
        while (t < smoothTime)
        {
            playerCamera.localPosition = Vector3.Lerp(playerCamera.localPosition, originalPosition, t / smoothTime);
            playerCamera.localRotation = Quaternion.Slerp(playerCamera.localRotation, originalRotation, t / smoothTime);
            t += Time.deltaTime;
            yield return null;
        }

        playerCamera.localPosition = originalPosition;
        playerCamera.localRotation = originalRotation;
    }
}
