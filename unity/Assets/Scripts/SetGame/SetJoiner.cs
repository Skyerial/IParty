using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class SetJoiner : MonoBehaviour
{
    [Tooltip("Assign your player prefab here")]
    public GameObject prefab;

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
            SetGameManager.setGameManager.RegisterPlayerGame(pi);
        }
    }
}