using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class JoinAllTyping : MonoBehaviour
{
    public GameObject prefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
            Debug.Log($"null check - {pi} - {device}");
            TMGameManager.Instance?.RegisterPlayer(pi, device);
        }   
    }
}
