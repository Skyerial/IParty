using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class JoinAllGyro : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject prefab;
    public Material targetMaterial;

    private int playerIndex = 0;
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
                // if (playerInput != null)
                // {
                //     // Color col = colorPlayer(playerInput);
                // }
                // else
                // {
                //     Debug.LogError("PlayerInput == NULL");
                // }
            }
            GameManagerGyro.Instance.StartGame();
        }
    }

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
