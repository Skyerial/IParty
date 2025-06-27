using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/**
 * @brief Joining Script for the tank game scene.
 * This script is responsible for joining all players in the tank game by spawning tanks
 * and assigning colors based on their input devices.
 */
public class JoinAllTank : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject prefab;
    public Material targetMaterial;
    void Start()
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
                    colorTank(playerInput);
                    GameManager.RegisterPlayerGame(playerInput);
                }
                else
                {
                    Debug.LogError("PlayerInput == NULL");
                }
            }
        }
    }

    /**
     * @brief Colors the tank based on the player's input device.
     * This method iterates through all renderers and their materials to find the target material
     * and applies the player's color to it.
     * @param playerInput The PlayerInput instance of the player whose tank is being colored.
     */
    void colorTank(PlayerInput playerInput)
    {
        Renderer[] renderers = playerInput.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            Debug.Log(renderer.name);
            // Check each material in the renderer
            foreach (Material mat in renderer.sharedMaterials)
            {
                Debug.Log(mat.name);
                if (mat == targetMaterial)
                {
                    InputDevice device = playerInput.devices[0];
                    renderer.material = PlayerManager.findColor(device);
                }
            }
        }
    }
}
