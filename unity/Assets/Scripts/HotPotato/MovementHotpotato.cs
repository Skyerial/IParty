using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class MovementHotpotato : MonoBehaviour
{
    PlayerInput playerInput;
    InputAction throw1Action;
    InputAction throw2Action;
    InputAction throw3Action;
    public BombManager bombManager;
    public float throwCooldown = 0.8f;
    private float cooldownTimer = 0f;

    void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        throw1Action = playerInput.actions["ThrowTo1"];
        throw2Action = playerInput.actions["ThrowTo2"];
        throw3Action = playerInput.actions["ThrowTo3"];

        throw1Action.Enable();
        throw2Action.Enable();
        throw3Action.Enable();
    }


    void Update()
    {
        cooldownTimer += Time.deltaTime;
        if (!IsHoldingBomb() || cooldownTimer < throwCooldown) return;

        var others = GetOtherPlayers();
        if (others.Count < 3) return;

        if (throw1Action.triggered)
        {
            ThrowBomb(others[0]);
        }
        else if (throw2Action.triggered)
        {
            ThrowBomb(others[1]);
        }
        else if (throw3Action.triggered)
        {
            ThrowBomb(others[2]);
        }
    }

    List<GameObject> GetOtherPlayers()
    {
        return bombManager.players.FindAll(p => p != null && p != gameObject);
    }

    bool IsHoldingBomb()
    {
        if (bombManager == null || bombManager.GetCurrentBomb() == null)
        {
            return false;
        }
        return bombManager.GetCurrentBomb().transform.parent == transform;
    }

    void ThrowBomb(GameObject target)
    {
        GameObject bomb = bombManager.GetCurrentBomb();
        Bomb bombScript = bomb.GetComponent<Bomb>();
        bombScript.isBeingThrown = true;

        bomb.transform.SetParent(null);

        Throw throwScript = bomb.GetComponent<Throw>();
        throwScript.ThrowToTarget(target.transform, bombScript);

        Debug.Log($"{gameObject.name} threw the bomb to {target.name}");
        cooldownTimer = 0f;
    }


}
