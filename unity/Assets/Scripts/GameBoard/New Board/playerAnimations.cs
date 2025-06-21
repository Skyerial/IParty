using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class playerAnimations : MonoBehaviour
{
    private Animator animator;
    public LayerMask groundLayerMask;
    public float groundCheckOffset = 0.1f;
    private Transform playerImageTransform;
    private CapsuleCollider cap;
    private Camera playerCamera;

    void Awake()
    {
        // Find the child named "Camera" and get its Camera component
        Transform cameraTransform = transform.Find("Camera");
        if (cameraTransform != null)
        {
            playerCamera = cameraTransform.GetComponent<Camera>();
        }
        else
        {
            Debug.LogWarning("No child named 'Camera' found on player " + gameObject.name);
        }

        cap = GetComponent<CapsuleCollider>();

        // Get the Animator component attached to the same GameObject
        animator = GetComponent<Animator>();

        playerImageTransform = transform.Find("Player - Image");

        if (animator == null)
        {
            Debug.LogWarning("Animator component not found on this GameObject!");
        }
    }
    void Update()
    {
        animator.SetBool("IsGrounded", IsGrounded());
    }
    public IEnumerator rotate_and_jump(Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        direction.y = 0f; // eliminate vertical angle
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            if (playerImageTransform == null)
            {
                Debug.Log("Player image found not");
            }
            playerImageTransform.rotation = targetRotation;
        }
        animator.SetTrigger("Jump");
        yield return StartCoroutine(JumpArc(transform, start, end, 1.0f, 3.0f));
    }

    private bool IsGrounded()
    {
        // compute sphere‐check at bottom of capsule
        Vector3 worldCenter = playerImageTransform.TransformPoint(cap.center);
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

    private IEnumerator JumpArc(Transform target, Vector3 start, Vector3 end, float duration, float height)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = Mathf.Clamp01(elapsed / duration);

            float arc = height * 4f * (t - t * t); // Parabolic arc
            Vector3 currentPos = Vector3.Lerp(start, end, t);
            currentPos.y += arc;

            target.position = currentPos;

            elapsed += Time.deltaTime;
            yield return null;
        }

        if (Vector3.Distance(target.position, end) > 2f)
        {
            target.position = end; // only correct if it's noticeably off
            Debug.Log("Repositioning");
        }

    }
    public void ActivateCamera(bool activate)
    {
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(activate);
        }
    }
}

