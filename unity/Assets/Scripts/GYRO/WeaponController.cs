using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Collider), typeof(Rigidbody))]
public class WeaponController : MonoBehaviour
{
    [Header("Slam Settings")]

    [Tooltip("Total slam duration (down + up) in seconds")]
    public float slamDuration = 0.3f;

    [Tooltip("Multiplier to slow down the return after a miss (1 = normal speed, 2 = twice as slow)")]
    public float missReturnMultiplier = 2f;

    [Tooltip("Slam angle X (downward)")]
    public float slamAngleX = 60f;

    [Tooltip("Slam angle Z (sideways)")]
    public float slamAngleZ = -25f;

    [Tooltip("for the dizzy effect")]
    public Transform playerCamera;
    public float dizzyDuration = 2f;
    public float dizzyIntensity = 1f;
    PlayerInput playerInput;
    InputAction slamAction;

    bool isSlamming = false;
    bool hasHit = false;
    Quaternion startRot, downRot;

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

    public void Slam()
    {
        if (!isSlamming)
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
