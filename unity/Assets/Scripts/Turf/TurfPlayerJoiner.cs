// TurfPlayerJoiner.cs
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class TurfPlayerJoiner : MonoBehaviour
{
    [Tooltip("Assign your player prefab here")]
    public GameObject prefab;

    void Start()
    {
        if (ServerManager.allControllers == null) return;

        // Tell the PlayerInputManager which prefab to spawn
        PlayerInputManager.instance.playerPrefab = prefab;

        // Tank-style join loop: spawn & hand off to GameManager
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
