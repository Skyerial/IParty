using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class Winscreen3DManager : MonoBehaviour
{

    public GameObject prefab;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        if (ServerManager.allControllers != null)
        {
            foreach (var device in ServerManager.allControllers.Values.ToArray())
            {
                Debug.Log("Spawning...");
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
