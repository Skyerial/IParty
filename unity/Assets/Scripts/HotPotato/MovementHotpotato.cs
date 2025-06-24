using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/**
 * @brief Handles player input and bomb throwing logic for the Hot Potato game.
 */
public class MovementHotpotato : MonoBehaviour
{
    public BombManager bombManager;
    public float throwCooldown = 0.8f;
    private float cooldownTimer = 0f;
    private bool canThrow = true;
    private PlayerInput playerInput;
    // public Animator animator;
    private Dictionary<string, GameObject> buttonToTarget = new();
    private bool lastBombHoldingState = false;

    /**
     * @brief Initializes input actions for throw buttons and sets up event listeners.
     * @return void
     */
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

    /**
     * @brief Updates the throw cooldown timer to re-enable throwing after a delay.
     * @return void
     */
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

        bool isCurrentlyHolding = IsHoldingBomb();
        if (isCurrentlyHolding != lastBombHoldingState)
        {
            // SetHoldingBombState(isCurrentlyHolding);
            lastBombHoldingState = isCurrentlyHolding;
        }
    }

    /**
     * @brief Removes event listeners on input actions when this object is destroyed.
     * @return void
     */
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

    /**
     * @brief Checks if this player is currently holding the bomb.
     * @return bool True if holding bomb, false otherwise.
     */
    private bool IsHoldingBomb()
    {
        return bombManager != null &&
               bombManager.GetCurrentBomb() != null &&
               bombManager.GetCurrentBomb().transform.parent == transform;
    }

    /**
     * @brief Handles input when a throw button is pressed. Throws the bomb if possible.
     * @param button The button pressed ("A", "B", "C", "D").
     * @return void
     */
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

    /**
     * @brief Detaches the bomb and initiates the throw toward the target player.
     * @param target The GameObject to throw the bomb to.
     * @return void
     */
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
        // SetHoldingBombState(false);
    }
    /**
     * @brief Maps controller buttons to target players based on provided player config data.
     * @param playerConfigs List of opponent player configurations.
     * @return void
     */
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
    
    // public void SetHoldingBombState(bool isHolding)
    // {
    //     animator.SetBool("Throw", isHolding);
    // }
}

