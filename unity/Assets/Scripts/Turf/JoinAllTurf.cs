using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class JoinAllTurf : MonoBehaviour
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
                    Color col = colorPlayer(playerInput);
                }
                else
                {
                    Debug.LogError("PlayerInput == NULL");
                }
            }
        }
    }

    Color colorPlayer(PlayerInput playerInput)
    {
        SkinnedMeshRenderer[] renderers = playerInput.GetComponentsInChildren<SkinnedMeshRenderer>();
        foreach (SkinnedMeshRenderer renderer in renderers)
        {
            Debug.Log(renderer.name);
            if (renderer.name == "Body.008")
            {
                InputDevice device = playerInput.devices[0];
                Material mat = PlayerManager.findColor(device);
                renderer.material = mat;
                return mat.color;
            }
        }

        return Color.pink;
    }
}
