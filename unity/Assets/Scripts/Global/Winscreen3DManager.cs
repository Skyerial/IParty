using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Winscreen3DManager : MonoBehaviour
{

    public GameObject prefab;

    void Start()
    {
        JoinAllPlayers();
    }

    /**
     * @brief Joins all connected players and initializes their setup.
     * @return void
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
