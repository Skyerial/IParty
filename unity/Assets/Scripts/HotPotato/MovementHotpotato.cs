using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MovementHotpotato : MonoBehaviour
{
    public BombManager bombManager;
    public float throwCooldown = 0.8f;
    private float cooldownTimer = 0f;
    private bool canThrow = true;

    private void Awake()
    {
        cooldownTimer = throwCooldown;
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

    public void OnThrowTo1(InputAction.CallbackContext context)
    {
        if (context.performed)
            TryThrowToIndex(0);
    }

    public void OnThrowTo2(InputAction.CallbackContext context)
    {
        if (context.performed)
            TryThrowToIndex(1);
    }

    public void OnThrowTo3(InputAction.CallbackContext context)
    {
        if (context.performed)
            TryThrowToIndex(2);
    }

    public void OnThrowTo4(InputAction.CallbackContext context)
    {
        if (context.performed)
            TryThrowToIndex(3);
    }
}

