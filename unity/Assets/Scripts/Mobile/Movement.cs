using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    PlayerInput playerInput;
    InputAction moveAction;
    Animator animator;


    public float moveSpeed = 0.5f;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        animator = GetComponent<Animator>();
        moveAction = playerInput.actions.FindAction("Move");
    }

    void Update(){
        MovePlayer();
    }

    void MovePlayer() {
        Vector2 direction = moveAction.ReadValue<Vector2>();
        if (direction != Vector2.zero)
        {
            animator.SetBool("isMoving", true);
            Vector3 movementDirection = new Vector3(direction.x, 0, direction.y);
            Quaternion toRotation = Quaternion.LookRotation(movementDirection, Vector3.up);
            transform.position += movementDirection * moveSpeed * Time.deltaTime;
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, 720 * Time.deltaTime);
        }
        else
        {
            animator.SetBool("isMoving", false);
        }
    }
}
