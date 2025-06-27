using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/**
 * @brief Joining Script for the mobile testing scene.
*/
public class JoinAll : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (ServerManager.allControllers != null)
        {
            foreach (var device in ServerManager.allControllers.Values.ToArray())
                PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);
        }
    }
}
