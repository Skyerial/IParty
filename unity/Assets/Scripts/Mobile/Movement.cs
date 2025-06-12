using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    PlayerInput playerInput;
    InputAction throwAction;
    public float moveSpeed = 0.5f;
    public BombManager bombManager;
    
    private Animator animator;

    public float throwCooldown = 0.8f;
    private float cooldownTimer = 0f;


    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        throwAction = playerInput.actions.FindAction("Throw");
        throwAction.Enable();
        animator = GetComponent<Animator>();
    }

    void Update()
    {

        cooldownTimer += Time.deltaTime;
        if (IsHoldingBomb() && throwAction.triggered && cooldownTimer >= throwCooldown)
        {
            animator.SetBool("IsThrown", true);
            ThrowBomb();
            cooldownTimer = 0f;
        }
    }

    bool IsHoldingBomb()
    {
        if (bombManager == null || bombManager.GetCurrentBomb() == null) return false;

        return bombManager.GetCurrentBomb().transform.parent == transform;
    }

    // void ThrowBomb()
    // {
        // var others = bombManager.players.FindAll(p => p != null && p != gameObject);
        // if (others.Count == 0) return;

        // GameObject target = others[Random.Range(0, others.Count)];

        // GameObject bomb = bombManager.GetCurrentBomb();
        // bomb.transform.SetParent(target.transform);
        // bomb.transform.localPosition = new Vector3(0, 2f, 0);
        // bomb.transform.localRotation = Quaternion.identity;

        // Debug.Log($"{gameObject.name} threw the bomb to {target.name}");
    // }
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
