// TurfPlayerJoiner.cs
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/**
 * @brief Joins all connected input devices as players at game start for Turf mode using the PlayerInputManager.
 */
public class TurfPlayerJoiner : MonoBehaviour
{
    /**
     * @brief Prefab used to instantiate new PlayerInput instances.
     */
    [Tooltip("Assign your player prefab here")]
    public GameObject prefab;

    /**
     * @brief Unity event called on Start; iterates connected controllers, joins them as players, and registers them.
     */
    void Start()
    {
        if (ServerManager.allControllers == null) return;

        PlayerInputManager.instance.playerPrefab = prefab;

        foreach (var device in ServerManager.allControllers.Values.ToArray())
        {
            var pi = PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);
            if (pi == null)
            {
                Debug.LogError($"JoinPlayer failed for {device}");
                continue;
            }
            TurfGameManager.RegisterPlayerGame(pi);
        }
    }
}
