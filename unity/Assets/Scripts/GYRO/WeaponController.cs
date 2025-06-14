using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider), typeof(Rigidbody), typeof(PlayerInput))]
public class WeaponController : MonoBehaviour
{
    [Header("Slam Settings")]
    [Tooltip("Degrees to swing down on slam")]
    public float slamAngle = 60f;

    [Tooltip("Total slam duration (down + up) in seconds")]
    public float slamDuration = 0.3f;

    [Tooltip("Multiplier to slow down the return after a miss (1 = normal speed, 2 = twice as slow)")]
    public float missReturnMultiplier = 2f;

    [Tooltip("Slam angle X (downward)")]
    public float slamAngleX = 60f;

    [Tooltip("Slam angle Z (sideways)")]
    public float slamAngleZ = -25f;


    public CanvasGroup dizzyOverlay;

    public float dizzyDuration = 2f;

    PlayerInput playerInput;
    InputAction slamAction;

    bool isSlamming = false;
    bool hasHit = false;
    Quaternion startRot, downRot;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        slamAction = playerInput.actions.FindAction("Jump");

        var rb = GetComponent<Rigidbody>();
        rb.useGravity = false;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints
                         .FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ
                         | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    void OnEnable() => slamAction.Enable();
    void OnDisable() => slamAction.Disable();

    void Update()
    {
        if (slamAction.triggered && !isSlamming)
            StartCoroutine(SlamRoutine());
    }

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

    void OnTriggerEnter(Collider other)
    {
        if (!isSlamming || hasHit) return;

        if (other.CompareTag("Mole"))
        {
            hasHit = true;
            var mole = other.GetComponent<Mole>();
            mole?.OnHit();
        }
        else if (other.CompareTag("Bomb"))
        {
            hasHit = true;
            Debug.Log("💣 Bomb hit! Player is dizzy!");
            // StartCoroutine(HandleBombHit());
        }
    }


    // IEnumerator HandleBombHit()
    // {
    //     // Disable input
    //     slamAction.Disable();

    //     // Show overlay
    //     if (dizzyOverlay != null)
    //     {
    //         dizzyOverlay.alpha = 1f;
    //         dizzyOverlay.blocksRaycasts = true;
    //     }

    //     yield return new WaitForSeconds(dizzyDuration);

    //     // Hide overlay
    //     if (dizzyOverlay != null)
    //     {
    //         dizzyOverlay.alpha = 0f;
    //         dizzyOverlay.blocksRaycasts = false;
    //     }

    //     // Re-enable input
    //     slamAction.Enable();
    // }


}
