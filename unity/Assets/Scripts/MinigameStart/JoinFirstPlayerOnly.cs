using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class JoinPartyLeader : MonoBehaviour
{
    public GameObject prefab;

    private void Start()
    {
        var controllers = ServerManager.allControllers.Values.ToList();
        if (controllers.Count == 0)
        {
            Debug.LogWarning("No controllers found in ServerManager.");
            return;
        }

        var leader = controllers[0];
        var leaderMsg = new ServerManager.MessagePlayers {
            type       = "controller",
            controller = "description"
        };

        ServerManager.SendMessageToClient(leader.remoteId, JsonUtility.ToJson(leaderMsg));

        for (int i = 1; i < controllers.Count; i++)
        {
            var dev = controllers[i];
            var waitMsg = new ServerManager.MessagePlayers {
                type       = "controller",
                controller = "waitingpage"
            };

            ServerManager.SendMessageToClient(dev.remoteId, JsonUtility.ToJson(waitMsg));
        }

        PlayerInputManager.instance.playerPrefab = prefab;
        PlayerInputManager.instance.JoinPlayer(-1, -1, null, leader);
    }
}
