using UnityEngine;
using UnityEngine.InputSystem;

public class MovementHotpotato : MonoBehaviour
{
    PlayerInput playerInput;
    Animator animator;
    InputAction throwAction;
    public BombManager bombManager;
    public float throwCooldown = 0.8f;
    private float cooldownTimer = 0f;


    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        throwAction = playerInput.actions.FindAction("Throw");
        animator = GetComponent<Animator>();
        throwAction.Enable();
    }

    void Update()
    {

        cooldownTimer += Time.deltaTime;
        if (IsHoldingBomb() && throwAction.triggered && cooldownTimer >= throwCooldown)
        {
            ThrowBomb();
            cooldownTimer = 0f;
        }
    }

    bool IsHoldingBomb()
    {
        if (bombManager == null || bombManager.GetCurrentBomb() == null)
        {
            animator.SetBool("IsThrown", false);
            return false;
        }

        animator.SetBool("IsThrown", true);

        return bombManager.GetCurrentBomb().transform.parent == transform;
    }

    void ThrowBomb()
    {
        var others = bombManager.players.FindAll(p => p != null && p != gameObject);
        if (others.Count == 0) return;

        GameObject target = others[Random.Range(0, others.Count)];
        GameObject bomb = bombManager.GetCurrentBomb();
        Bomb bombScript = bomb.GetComponent<Bomb>();
        bombScript.isBeingThrown = true;

        bomb.transform.SetParent(null);

        Throw throwScript = bomb.GetComponent<Throw>();
        throwScript.ThrowToTarget(target.transform, bombScript);

        Debug.Log($"{gameObject.name} threw the bomb to {target.name}");
    }


}
