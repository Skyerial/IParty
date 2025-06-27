using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/**
 * @brief Manages the 3D win screen by spawning and registering players in ranking order.
 */
public class Winscreen3DManager : MonoBehaviour
{
    /**
     * @brief Prefab used to instantiate each player's representation on the win screen.
     */
    public GameObject prefab;

    /**
     * @brief Unity event called on Start; initiates joining of all players for win display.
     */
    void Start()
    {
        JoinAllPlayers();
    }

    /**
     * @brief Joins all connected players based on game ranking, spawns their prefabs, and registers them.
     */
    void JoinAllPlayers()
    {
        if (ServerManager.allControllers != null && PlayerManager.instance != null)
        {
            List<int> ranking = PlayerManager.instance.rankGameboard;

            foreach (int rankedPlayerID in ranking)
            {
                // Find the InputDevice for the given player ID
                InputDevice device = ServerManager.allControllers
                    .FirstOrDefault(pair => 
                        PlayerManager.playerStats.ContainsKey(pair.Value) &&
                        PlayerManager.playerStats[pair.Value].playerID == rankedPlayerID
                    ).Value;

                if (device == null)
                {
                    Debug.LogWarning($"No device found for player ID: {rankedPlayerID}");
                    continue;
                }

                Debug.Log($"Spawning ranked player {rankedPlayerID}...");
                PlayerInputManager.instance.playerPrefab = prefab;

                PlayerInput playerInput = PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);
                if (playerInput != null)
                {
                    GameManager.RegisterPlayerGame(playerInput);
                }
                else
                {
                    Debug.LogError("PlayerInput == NULL");
                }
            }
        }
    }
}
