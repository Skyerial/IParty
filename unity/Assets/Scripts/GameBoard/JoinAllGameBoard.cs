using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class JoinAllGameBoard : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject prefab;
    public GameMaster gameMaster;
    // public Material targetMaterial;
    void Start()
    {
        gameMaster = FindAnyObjectByType<GameMaster>();
        if (ServerManager.allControllers != null)
        {
            foreach (var device in ServerManager.allControllers.Values.ToArray())
            {
                Debug.Log("Spawning...");
                PlayerInputManager.instance.playerPrefab = prefab;
                PlayerInput playerInput = PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);
                if (playerInput != null)
                {
                    gameMaster.RegisterPlayer(playerInput);
                    colorPlayer(playerInput);
                }
                else
                {
                    Debug.LogError("PlayerInput == NULL");
                }
            }
        }
    }

    void colorPlayer(PlayerInput playerInput)
    {
        var body = playerInput.GetComponentsInChildren<SkinnedMeshRenderer>()
                     .First(r => r.name == "Body");
        
        var face = playerInput.GetComponentsInChildren<SkinnedMeshRenderer>()
                     .First(r => r.name == "Face");
        
        body.material = PlayerManager.findColor(playerInput.devices[0]);

        Material newMat = new Material(face.material);
        newMat.mainTexture = PlayerManager.findFace(playerInput.devices[0]);
        face.material = newMat;
    }

}
