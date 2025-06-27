using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;


/**
* @brief Join function that connects a remote player to the PlayerInputManager of unity, assigns a prefab for the player
            and then registers the player with the TMGameManager
*/
public class JoinAllTyping : MonoBehaviour
{
    public GameObject prefab;

    void Start()
    {
        if (ServerManager.allControllers == null) return;

        PlayerInputManager.instance.playerPrefab = prefab;

        foreach (var device in ServerManager.allControllers.Values.ToArray())
        {
            var pi = PlayerInputManager.instance?.JoinPlayer(-1, -1, null, device);
            if (pi == null)
            {
                Debug.LogError($"JoinPlayer failed for {device}");
                continue;
            }
            TMGameManager.Instance?.RegisterPlayer(pi, device);
        }
    }
}
