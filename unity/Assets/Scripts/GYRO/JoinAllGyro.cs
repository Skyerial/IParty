using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/**
 * @brief Spawns all players using input devices from ServerManager and assigns split-screen camera views.
 */
public class JoinAllGyro : MonoBehaviour
{
    /**
     * @brief The player prefab to instantiate for each joined player.
     */
    public GameObject prefab;

    /**
     * @brief Optional material reference (not used directly in this script).
     */
    public Material targetMaterial;

    /**
     * @brief Tracks the index of each joined player for camera layout.
     */
    private int playerIndex = 0;

    /**
     * @brief Unity Start method; spawns one player for each input device in ServerManager and applies split screen.
     */
    void Start()
    {
        if (ServerManager.allControllers != null)
        {
            int totalPlayers = ServerManager.allControllers.Count;
            foreach (var device in ServerManager.allControllers.Values.ToArray())
            {
                Debug.Log("Spawning...");
                PlayerInputManager.instance.playerPrefab = prefab;
                PlayerInput playerInput = PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);

                if (playerInput != null)
                {
                    GameManagerGyro.Instance.RegisterPlayer(playerInput);
                    Camera cam = playerInput.GetComponentInChildren<Camera>();
                    if (cam != null)
                    {
                        ApplySplitScreenViewport(cam, playerIndex, totalPlayers);
                    }
                    playerIndex++;
                }
            }
            GameManagerGyro.Instance.StartGame();
        }
    }

    /**
     * @brief Configures the given camera to occupy the correct screen portion for split-screen play.
     * @param cam The camera to assign a viewport to.
     * @param index The player's index in the join order.
     * @param totalPlayers The total number of players joined.
     */
    void ApplySplitScreenViewport(Camera cam, int index, int totalPlayers)
    {
        if (totalPlayers == 1)
        {
            cam.rect = new Rect(0f, 0f, 1f, 1f);
        }
        else if (totalPlayers == 2)
        {
            if (index == 0)
                cam.rect = new Rect(0f, 0.5f, 1f, 0.5f); // Top half
            else if (index == 1)
                cam.rect = new Rect(0f, 0f, 1f, 0.5f);   // Bottom half
        }
        else if (totalPlayers <= 4)
        {
            switch (index)
            {
                case 0:
                    cam.rect = new Rect(0f, 0.5f, 0.5f, 0.5f); // Top-left
                    break;
                case 1:
                    cam.rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f); // Top-right
                    break;
                case 2:
                    cam.rect = new Rect(0f, 0f, 0.5f, 0.5f); // Bottom-left
                    break;
                case 3:
                    cam.rect = new Rect(0.5f, 0f, 0.5f, 0.5f); // Bottom-right
                    break;
            }
        }
        else
        {
            Debug.LogWarning("Only supports up to 4 players.");
        }
    }
}
