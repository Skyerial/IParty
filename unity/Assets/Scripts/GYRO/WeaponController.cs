// WeaponController.cs
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

    PlayerInput playerInput;
    InputAction slamAction;

    bool    isSlamming = false;
    bool    hasHit     = false;
    Quaternion startRot, downRot;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        slamAction  = playerInput.actions.FindAction("Attack");

        var rb = GetComponent<Rigidbody>();
        rb.useGravity  = false;
        rb.isKinematic = false;
        rb.constraints = RigidbodyConstraints
                         .FreezePositionX | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionZ
                       | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;
    }

    void OnEnable()
    {
        slamAction.Enable();
    }

    void OnDisable()
    {
        slamAction.Disable();
    }

    void Update()
    {
        if (slamAction.triggered && !isSlamming)
            StartCoroutine(SlamRoutine());
    }

    IEnumerator SlamRoutine()
    {
        isSlamming = true;
        hasHit     = false;

        startRot = transform.localRotation;
        downRot  = startRot * Quaternion.Euler(slamAngle, 0f, 0f);
        float halfTime = slamDuration * 0.5f;
        float t = 0f;

        while (t < halfTime && !hasHit)
        {
            transform.localRotation = Quaternion.Slerp(startRot, downRot, t / halfTime);
            t += Time.deltaTime;
            yield return null;
        }
        if (!hasHit)
            transform.localRotation = downRot;

        t = 0f;
        Quaternion fromRot = transform.localRotation;
        while (t < halfTime)
        {
            transform.localRotation = Quaternion.Slerp(fromRot, startRot, t / halfTime);
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
    }
}
