using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/**
 * @brief Auto-joins all connected virtual controllers at the start of the mobile testing scene.
 */
public class JoinAll : MonoBehaviour
{
    /**
     * @brief Unity event called on Start; iterates over all virtual controllers and joins each as a player.
     */
    void Start()
    {
        if (ServerManager.allControllers != null)
        {
            foreach (var device in ServerManager.allControllers.Values.ToArray())
                PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);
        }
    }
}
