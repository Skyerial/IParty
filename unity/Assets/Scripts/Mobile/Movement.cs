using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;
public class Movement : MonoBehaviour
{
    PlayerInput playerInput;
    InputAction moveAction;

    public bool MovementEnabled = true;

    Animator animator;


    public float moveSpeed = 0.5f;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions.FindAction("Move");
        animator = GetComponent<Animator>();
        StartCoroutine(PlayIdleAnimationsRandomly());
    }

    void Update()
    {
        if (!MovementEnabled) return;
        MovePlayer();
    }

    void MovePlayer()
    {
        Vector2 direction = moveAction.ReadValue<Vector2>();
        transform.position += new Vector3(direction.x, 0, direction.y) * moveSpeed * Time.deltaTime;
    }
    
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
