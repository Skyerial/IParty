using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MovementHotpotato : MonoBehaviour
{
    public BombManager bombManager;
    public float throwCooldown = 0.8f;
    private float cooldownTimer = 0f;
    private bool canThrow = true;
    private PlayerInput playerInput;
    private InputAction throwTo1Action;
    private InputAction throwTo2Action;
    private InputAction throwTo3Action;
    private InputAction throwTo4Action;

    void Start()
    {
        cooldownTimer = throwCooldown;
        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            throwTo1Action = playerInput.actions["ThrowTo1"];
            throwTo2Action = playerInput.actions["ThrowTo2"];
            throwTo3Action = playerInput.actions["ThrowTo3"];
            throwTo4Action = playerInput.actions["ThrowTo4"];

            throwTo1Action.performed += OnThrowTo1Performed;
            throwTo2Action.performed += OnThrowTo2Performed;
            throwTo3Action.performed += OnThrowTo3Performed;
            throwTo4Action.performed += OnThrowTo4Performed;

            throwTo1Action.Enable();
            throwTo2Action.Enable();
            throwTo3Action.Enable();
            throwTo4Action.Enable();
        }
    }


    private void OnDestroy()
    {
        throwTo1Action.performed -= OnThrowTo1Performed;
        throwTo2Action.performed -= OnThrowTo2Performed;
        throwTo3Action.performed -= OnThrowTo3Performed;
        throwTo4Action.performed -= OnThrowTo4Performed;
    }

    private void Update()
    {
        if (!canThrow)
        {
            cooldownTimer += Time.deltaTime;
            if (cooldownTimer >= throwCooldown)
            {
                canThrow = true;
                cooldownTimer = 0f;
            }
        }
    }

    private bool IsHoldingBomb()
    {
        return bombManager != null &&
               bombManager.GetCurrentBomb() != null &&
               bombManager.GetCurrentBomb().transform.parent == transform;
    }

    private List<GameObject> GetOtherPlayers()
    {
        return bombManager.players.FindAll(p => p != null && p != gameObject);
    }

    private void TryThrowToIndex(int index)
    {
        if (!IsHoldingBomb() || !canThrow) return;

        var others = GetOtherPlayers();
        if (index >= others.Count) return;

        GameObject target = others[index];

        GameObject bomb = bombManager.GetCurrentBomb();
        Bomb bombScript = bomb.GetComponent<Bomb>();
        bombScript.isBeingThrown = true;

        bomb.transform.SetParent(null);

        Throw throwScript = bomb.GetComponent<Throw>();
        throwScript.ThrowToTarget(target.transform, bombScript);

        Debug.Log($"{gameObject.name} threw the bomb to {target.name}");

        canThrow = false;
        cooldownTimer = 0f;
    }

    private void OnThrowTo1Performed(InputAction.CallbackContext context)
    {
        Debug.Log("Throw to 1 performed");
        TryThrowToIndex(0);
    }

    private void OnThrowTo2Performed(InputAction.CallbackContext context)
    {
        Debug.Log("Throw to 2 performed");
        TryThrowToIndex(1);
    }

    private void OnThrowTo3Performed(InputAction.CallbackContext context)
    {
        Debug.Log("Throw to 3 performed");
        TryThrowToIndex(2);
    }

    private void OnThrowTo4Performed(InputAction.CallbackContext context)
    {
        Debug.Log("Throw to 4 performed");
        TryThrowToIndex(3);
    }
}

