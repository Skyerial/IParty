using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    PlayerInput playerInput;
    InputAction moveAction;

    public bool MovementEnabled = true;


    public float moveSpeed = 0.5f;

    void Start(){
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
    }

    void Update(){
        if (!MovementEnabled) return;
        MovePlayer();
    }

    void MovePlayer(){
        Vector2 direction = moveAction.ReadValue<Vector2>();
        transform.position += new Vector3(direction.x, 0, direction.y) * moveSpeed * Time.deltaTime;
    }
}
