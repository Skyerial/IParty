using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class JoinPartyLeader : MonoBehaviour
{
    public GameObject prefab;

    private void Start()
    {
        var controllers = ServerManager.allControllers?.Values.ToList();
        if (controllers != null && controllers.Count > 0)
        {
            // Assign first player as party leader
            var partyLeader = controllers[0];

            ServerManager.SendToSpecificSocket(partyLeader, "spleef");

            foreach (var device in controllers.Skip(1))
            {
                ServerManager.SendToSpecificSocket(device, "mainboard");
            }

            // Join party leader into the scene
            PlayerInputManager.instance.playerPrefab = prefab;
            PlayerInputManager.instance.JoinPlayer(-1, -1, null, partyLeader);
        }
        else
        {
            Debug.LogWarning("No controllers found in ServerManager.");
        }
    }
}
