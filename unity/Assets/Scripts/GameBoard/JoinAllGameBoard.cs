using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class JoinAllGameBoard : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public GameObject prefab;
    public GameMaster gameMaster;
    // public Material targetMaterial;

    /**
    * @brief Function called on start of the gameboard (every time)
    */
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

                    // ✅ Restore board position
                    int savedPos = PlayerManager.playerStats[device].position;
                    int playerID = PlayerManager.playerStats[device].playerID;
                    playerInput.GetComponent<PlayerMovement>().current_pos = savedPos;

                    // ✅ Move GameObject to correct tile marker
                    if (savedPos < gameMaster.tileGroup.childCount)
                    {
                        var tile = gameMaster.tileGroup.GetChild(savedPos);
                        var tileScript = tile.GetComponent<tileHandler>();
                        if (tileScript != null && tileScript.markers.Length > playerID)
                        {
                            var marker = tileScript.markers[playerID];
                            playerInput.transform.position = marker.position + Vector3.up;
                        }
                        else
                        {
                            Debug.LogWarning($"Invalid tile marker for player {playerID} at tile {savedPos}");
                        }
                    }
                    else
                    {
                        Debug.LogWarning($"Saved position {savedPos} exceeds tile count");
                    }
                }
                else
                {
                    Debug.LogError("PlayerInput == NULL");
                }
            }
        }
    }

    /**
    * @brief adds color to a given player
    */
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
