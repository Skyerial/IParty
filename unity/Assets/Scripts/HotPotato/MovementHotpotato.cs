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
    private Dictionary<string, GameObject> buttonToTarget = new();

    void Start()
    {
        cooldownTimer = throwCooldown;
        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            var buttons = new[] { "ButtonA", "ButtonB", "ButtonC", "ButtonD" };

            foreach (string btn in buttons)
            {
                InputAction action = playerInput.actions[btn];
                if (action != null)
                {
                    string buttonName = btn.Replace("Button", ""); 
                    action.performed += ctx => OnThrowPressed(buttonName);
                    action.Enable();
                }
            }
        }
    }

    void Update()
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

    private void OnDestroy()
    {
        if (playerInput == null) return;

        var buttons = new[] { "ButtonA", "ButtonB", "ButtonC", "ButtonD" };

        foreach (string btn in buttons)
        {
            InputAction action = playerInput.actions[btn];
            if (action != null)
            {
                action.performed -= ctx => OnThrowPressed(btn.Replace("Button", ""));
            }
        }
    }

    private bool IsHoldingBomb()
    {
        return bombManager != null &&
               bombManager.GetCurrentBomb() != null &&
               bombManager.GetCurrentBomb().transform.parent == transform;
    }

    private void OnThrowPressed(string button)
    {
        if (!IsHoldingBomb() || !canThrow) return;

        if (buttonToTarget.TryGetValue(button, out GameObject target) && target != null)
        {
            ThrowBombTo(target);
        }
        else
        {
            Debug.LogWarning($"No target mapped to button {button}");
        }
    }

    private void ThrowBombTo(GameObject target)
    {
        if (target == null || target == gameObject) return;

        GameObject bomb = bombManager.GetCurrentBomb();
        if (bomb == null) return;

        Bomb bombScript = bomb.GetComponent<Bomb>();
        bombScript.isBeingThrown = true;

        bomb.transform.SetParent(null);

        Throw throwScript = bomb.GetComponent<Throw>();
        throwScript.ThrowToTarget(target.transform, bombScript);

        Debug.Log($"{gameObject.name} threw the bomb to {target.name}");

        canThrow = false;
        cooldownTimer = 0f;
    }

    public void ConfigureThrowTargets(List<BombManager.PlayerConfig> playerConfigs)
    {
        buttonToTarget.Clear();

        foreach (var config in playerConfigs)
        {
            GameObject target = bombManager.players.Find(p =>
                p.GetComponent<PlayerInput>()?.devices[0] != null &&
                PlayerManager.playerStats.TryGetValue(p.GetComponent<PlayerInput>().devices[0], out var stats) &&
                stats.name == config.name
            );

            if (target != null)
            {
                buttonToTarget[config.button] = target;
            }
        }

        Debug.Log($"Configured throw targets for {gameObject.name}");
    }
}
