using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

/**
 * @brief Handles player movement and idle animation logic based on input in testing scene.
 */
public class Movement : MonoBehaviour
{
    PlayerInput playerInput;
    InputAction moveAction;

    /**
     * @brief Enables or disables player movement.
     */
    public bool MovementEnabled = true;

    Animator animator;

    /**
     * @brief Movement speed multiplier.
     */
    public float moveSpeed = 0.5f;

    /**
     * @brief Initializes input actions and starts idle animation coroutine.
     */
    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        animator = GetComponent<Animator>();
        StartCoroutine(PlayIdleAnimationsRandomly());
    }

    /**
     * @brief Handles player movement each frame if enabled.
     */
    void Update()
    {
        if (!MovementEnabled) return;
        MovePlayer();
    }

    /**
     * @brief Moves the player based on input direction and moveSpeed.
     */
    void MovePlayer()
    {
        Vector2 direction = moveAction.ReadValue<Vector2>();
        transform.position += new Vector3(direction.x, 0, direction.y) * moveSpeed * Time.deltaTime;
    }

    /**
     * @brief Randomly plays idle animations with randomized wait times and mirroring.
     */
    IEnumerator PlayIdleAnimationsRandomly()
    {
        while (true)
        {
            float waitTime = Random.Range(3f, 25f);
            yield return new WaitForSeconds(waitTime);

            if (animator != null)
            {
                bool mirror = Random.value > 0.5f;
                animator.SetBool("Mirror", mirror);

                if (Random.value > 0.5f)
                {
                    animator.SetTrigger("Look");
                }
                else
                {
                    animator.SetTrigger("Bored");
                }
            }
        }
    }
}
