using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System.Linq;
using TMPro;

/**
 * @brief Handles player spawning logic, especially in the Lobby scene.
 */
public class PlayerSpawn : MonoBehaviour
{
    public GameObject nameboardPrefab;
    int i = 0;
    public GameObject crown;
    /**
     * @brief Called at the start of the scene. Clears controller list if in Lobby.
     * @return void
     */
    void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Lobby") ServerManager.allControllers?.Clear();
    }

    /**
     * @brief Called when a new player joins. Places them in the correct spawn position depending on context.
     *        In the Lobby, it also assigns color, face texture, nameboard and disables input.
     * @param playerInput PlayerInput component of the joined player.
     * @return void
     */
    public void OnPlayerJoined(PlayerInput playerInput)
    {
        GameObject spawnParent = GameObject.Find("Spawn");
        Transform[] spawnPoints = spawnParent.transform.Cast<Transform>()
                                            .Where(t => t.parent == spawnParent.transform)
                                            .ToArray();

        int spawnIndex = i % spawnPoints.Length;
        playerInput.transform.position = spawnPoints[spawnIndex].position;
        playerInput.transform.rotation = spawnPoints[spawnIndex].rotation;

        Debug.Log($"Spawning player at: {spawnPoints[spawnIndex].name}");

        string currentScene = SceneManager.GetActiveScene().name;
        if (currentScene == "Lobby" || currentScene == "WinScreen3D")
        {
            InitPlayer(playerInput);
            playerInput.DeactivateInput();
            if (currentScene == "WinScreen3D")
            {
                playerInput.transform.localScale = Vector3.one * 1.5f;
            }
            else
            {
                spawnPoints[spawnIndex].GetChild(0).GetChild(0).gameObject.SetActive(false);
                playerInput.transform.localScale = Vector3.one * 2f;

                if (spawnPoints[spawnIndex].name == "L1")
                {
                    crown.SetActive(true);
                }
            }
        }
        i++;
    }
    /**
     * @brief Initializes the player when they join the game. Assigns the appropriate body material, face texture,
     *        and instantiates a nameboard displaying the player's name. Customizes appearance based on the player's input device.
     * @param playerInput The PlayerInput component associated with the joined player.
     * @return void
     */
    private void InitPlayer(PlayerInput playerInput)
    {
        InputDevice device = playerInput.devices.Count > 0 ? playerInput.devices[0] : null;

        // Assign body material
        Transform body = playerInput.transform.Find("Body");
        if (body != null && body.TryGetComponent(out SkinnedMeshRenderer renderer))
        {
            renderer.material = PlayerManager.findColor(device);
        }

        // Assign face texture
        Transform face = playerInput.transform.Find("Face");
        if (face != null && face.TryGetComponent(out SkinnedMeshRenderer renderer_face))
        {
            Texture2D faceTexture = (device != null) ? PlayerManager.findFace(device) : null;

            if (faceTexture != null && faceTexture.width > 2)
            {
                // Only replace material if a valid custom face texture is found
                Material newMat = new Material(renderer_face.sharedMaterial);
                newMat.mainTexture = faceTexture;
                renderer_face.material = newMat;
            }
            else
            {
                // Keep existing material (don't overwrite)
                Debug.Log("[Face Debug] No valid face texture found; keeping default material.");
            }
        }

        // Instantiate nameboard with player name
        if (nameboardPrefab != null && device != null && PlayerManager.playerStats.ContainsKey(device))
        {
            GameObject nameboard = Instantiate(nameboardPrefab, playerInput.transform);
            nameboard.transform.localPosition = new Vector3(0, 2f, 0);
            nameboard.transform.localRotation = Quaternion.identity;

            TextMeshProUGUI text = nameboard.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = PlayerManager.playerStats[device].name;
            }

        }
    }

}
