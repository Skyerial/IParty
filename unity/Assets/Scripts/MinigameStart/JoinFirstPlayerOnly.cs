using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/* * @brief Handles controller for party leader to let them start the game.
 * This script is responsible for sending messages to the leader and other players,
 * and joining the first player with a specified prefab.
 */
public class JoinPartyLeader : MonoBehaviour
{
    public GameObject prefab;

    /**
     * @brief Start is called once before the first execution of Update after the MonoBehaviour is created.
     * This method initializes the game by sending messages to the leader and other players,
     * and joining the first player with the specified prefab.
     */
    private void Start()
    {
        var controllers = ServerManager.allControllers.Keys.ToList();
        if (controllers.Count == 0)
        {
            Debug.LogWarning("No controllers found in ServerManager.");
            return;
        }

        var leader = controllers[0];
        var leaderMsg = new ServerManager.MessagePlayers
        {
            type = "controller",
            controller = "description"
        };

        ServerManager.SendMessageToClient(leader, JsonUtility.ToJson(leaderMsg));

        for (int i = 1; i < controllers.Count; i++)
        {
            var dev = controllers[i];
            var waitMsg = new ServerManager.MessagePlayers
            {
                type = "controller",
                controller = "waitingpage"
            };

            ServerManager.SendMessageToClient(dev, JsonUtility.ToJson(waitMsg));
        }

        PlayerInputManager.instance.playerPrefab = prefab;
        PlayerInputManager.instance.JoinPlayer(-1, -1, null, ServerManager.allControllers[leader]);
    }
}
