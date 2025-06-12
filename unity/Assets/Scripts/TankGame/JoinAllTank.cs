using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class JoinAllTank : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject prefab;
    void Start()
    {   if (ServerManager.allControllers != null)
        {
            foreach (var device in ServerManager.allControllers.Values.ToArray())
            {
                PlayerInputManager.instance.playerPrefab = prefab;
                PlayerInput playerInput = PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);
                if (playerInput != null)
                {
                    GameManager.RegisterPlayerGame(playerInput.gameObject);
                }
                else
                {
                    Debug.LogError("PlayerInput == NULL");
                }
            }
        }
    }
}
