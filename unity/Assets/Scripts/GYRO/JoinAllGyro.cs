using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class JoinAllGyro : MonoBehaviour
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
                // if (playerInput != null)
                // {
                //     // Color col = colorPlayer(playerInput);
                // }
                // else
                // {
                //     Debug.LogError("PlayerInput == NULL");
                // }
            }
        }
    }
}
