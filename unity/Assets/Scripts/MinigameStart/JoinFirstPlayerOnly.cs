using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class JoinPartyLeader : MonoBehaviour
{
    public GameObject prefab;

    private void Start()
    {
        ServerManager.SendtoAllSockets("spleef");

        var controllers = ServerManager.allControllers?.Values.ToList();
        if (controllers != null && controllers.Count > 0)
        {
            var partyLeaderDevice = controllers[0];

            PlayerInputManager.instance.playerPrefab = prefab;
            PlayerInputManager.instance.JoinPlayer(-1, -1, null, partyLeaderDevice);
        }
        else
        {
            Debug.LogWarning("No controllers found in ServerManager.");
        }
    }
}
