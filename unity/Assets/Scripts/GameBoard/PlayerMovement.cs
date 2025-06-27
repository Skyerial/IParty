using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    /**
    * @brief ID of you as a player
    */
    public int player_id = 0;

    /**
    * @brief current position of you as a player
    */
    public int current_pos = 0;

    /**
    * @brief integer telling if jumping is still happening
    */
    public int increment = 0;

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
    * @brief Capsule collider of the player
    */
    private CapsuleCollider cap;

    /**
    * @brief gamemaster class of the gameboard
    */
    private GameMaster gameMaster;

    /**
    * @brief playerinput object handling player input (component of player game object)
    */
    private PlayerInput playerInput;

    /**
    * @brief Specific inputaction to move player
    */
    private InputAction moveAction;

    /**
    * @brief Function ran at start of the class, to initiate components
    */
    private void Start()
    {
        // Newly added
        cap = GetComponent<CapsuleCollider>();

        // Get the Animator component attached to the same GameObject
        animator = GetComponent<Animator>();


        // Mapping the input
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Jump");
        moveAction.performed += ctx => HandleInput();

        // Finding the GameMaster
        gameMaster = FindAnyObjectByType<GameMaster>();

        // Assigning the correct Player ID
        player_id = PlayerManager.playerStats[playerInput.devices[0]].playerID;

        // Assigning players to the correct position NEEDS TO CHANGE
        current_pos = PlayerManager.playerStats[playerInput.devices[0]].position;
        Debug.Log("The spawning position is: " + current_pos);
        Transform tile = gameMaster.tileGroup.GetChild(current_pos);
        Transform spawnPoint = tile.GetChild(player_id);
        transform.position = spawnPoint.transform.position;
    }

    /**
    * @brief handles who is able to give input to the game
    */
    private void HandleInput()
    {
        Debug.Log($"Button pressed by {player_id}, the current player is {gameMaster.current_player}.");
        if (player_id == gameMaster.current_player && !gameMaster.numberShown)
        {
            gameMaster.press_random = 1;
        }
    }

    /**
    * @brief Checks for the player to be grounded during the board game
    * so that it can be animated accordingly
    */
    void Update()
    {
        animator.SetBool("IsGrounded", IsGrounded());
    }

    /**
    * @brief Couroutine for rotating as well as jumping to the next tile
    */
    public IEnumerator rotate_and_jump(Vector3 start, Vector3 end)
    {
        yield return StartCoroutine(rotate(start, end));
        yield return StartCoroutine(jump(start, end));
    }

    /**
    * @brief Couroutine for rotating to the next tile
    */
    public IEnumerator rotate(Vector3 start, Vector3 end)
    {
        Vector3 direction = end - start;
        direction.y = 0f; // eliminate vertical angle
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            if (transform == null)
            {
                Debug.Log("Player image found not");
            }
            yield return StartCoroutine(RotateToTarget(targetRotation, 0.2f));
        }
    }

    /**
    * @brief Function for handling jump from start to end
    */
    public IEnumerator jump(Vector3 start, Vector3 end)
    {
        animator.SetTrigger("Jump");
        yield return StartCoroutine(JumpArc(transform, start, end, 0.4f, 1f));
    }

    /**
    * @brief Couroutine for rotating target along targetRotation
    */
    public IEnumerator RotateToTarget(Quaternion targetRotation, float duration)
    {
        Quaternion startRotation = transform.rotation;
        float time = 0f;

        while (time < duration)
        {
            transform.rotation = Quaternion.Lerp(startRotation, targetRotation, time / duration);
            time += Time.deltaTime;
            yield return null; // Wait until next frame
        }

        transform.rotation = targetRotation; // Snap to final rotation to avoid overshoot
    }

    /**
    * @brief Function that checks if the player is grounded and returns a
    * boolean accordingly. So a grounded player is a player standing on
    * objects that unity considers as ground.
    */
    private bool IsGrounded()
    {
        // compute sphere‐check at bottom of capsule
        Vector3 worldCenter = transform.TransformPoint(cap.center);
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

        if (!IsGrounded())
        {
            // Preventing a double jump
            yield return new WaitForSeconds(0.5f);
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
