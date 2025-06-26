using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
/**
 * @brief Handling all player animations on the gameboard
 */

public class playerAnimations : MonoBehaviour
{
    /**
    * @brief Player animator used in the script
    */
    private Animator animator;

    /**
    * @brief Mask defining the ground layer
    */
    public LayerMask groundLayerMask;

    /**
    * @brief Float defining how far one can be from the ground to be
    * considered standing on it
    */
    public float groundCheckOffset = 0.1f;

    /**
    * @brief Transform of the actual player 3D model
    */
    private Transform playerImageTransform;

    /**
    * @brief Capsule collider of the player
    */
    private CapsuleCollider cap;

    /**
    * @brief Camera object attached to the player
    */
    private Camera playerCamera;

    /**
    * @brief Function on start up handling the camera and player
    */
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

    /**
    * @brief Checks for the player to be grounded during the board game
    * so that it can be animated accordingly
    */
    void Update()
    {
        animator.SetBool("IsGrounded", IsGrounded());

        if (IsGrounded())
        {
            animator.SetBool("Dash", false);
        }
    }

    /**
    * @brief Couroutine for rotating as well as jumping to the next tile
    */
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
        yield return StartCoroutine(JumpArc(transform, start, end, 0.5f, 3.0f));
    }
    /**
    * @brief Function that checks if the player is grounded and returns a
    * boolean accordingly. So a grounded player is a player standing on
    * objects that unity considers as ground.
    */
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

    /**
    * @brief Coroutine that causes the target to jump from start to end for
    * the duration with a given height.
    */
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

    /**
    * @brief Function activating the players camera or deactivating it dependant
    * on the activate boolean
    */
    public void ActivateCamera(bool activate)
    {
        if (playerCamera != null)
        {
            playerCamera.gameObject.SetActive(activate);
        }
    }

    /**
    * @brief Function that defines linear movement of the object from start
    * to end with duration.
    */
    public IEnumerator LinearMovement(Vector3 start, Vector3 end, float duration)
    {
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            transform.position = Vector3.Lerp(start, end, t);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    /**
    * @brief Function forcing the player to enter falling animation
    */
    public void makeFall()
    {
        animator.SetBool("Dash", true);
    }
}

