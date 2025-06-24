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
        if (currentScene == "Lobby")
        {
            spawnPoints[spawnIndex].GetChild(0).GetChild(0).gameObject.SetActive(false);

            // Assign body material
            Transform body = playerInput.transform.Find("Body");
            if (body != null && body.TryGetComponent(out SkinnedMeshRenderer renderer))
            {
                renderer.material = PlayerManager.findColor(playerInput.devices[0]);
            }

            // Assign face texture
            Transform face = playerInput.transform.Find("Face");
            if (face != null && face.TryGetComponent(out SkinnedMeshRenderer renderer_face))
            {
                Material newMat = new Material(renderer_face.material);
                InputDevice device = playerInput.devices.Count > 0 ? playerInput.devices[0] : null;
                Texture2D faceTexture = (device != null) ? PlayerManager.findFace(device) : null;

                if (faceTexture != null && faceTexture.width > 2)
                {
                    newMat.mainTexture = faceTexture;
                }
                else
                {
                    newMat.mainTexture = Texture2D.blackTexture;
                }

                renderer_face.material = newMat;
            }

            // Instantiate nameboard with player name
            if (nameboardPrefab != null)
            {
                GameObject nameboard = Instantiate(nameboardPrefab, playerInput.transform);
                nameboard.transform.localPosition = new Vector3(0, 2f, 0);
                nameboard.transform.localRotation = Quaternion.identity;

                TextMeshProUGUI text = nameboard.GetComponentInChildren<TextMeshProUGUI>();
                if (text != null)
                {
                    text.text = PlayerManager.playerStats[playerInput.devices[0]].name;
                }
            }

            // Resize and deactivate input
            playerInput.transform.localScale = Vector3.one * 2f;
            playerInput.DeactivateInput();
        }

        i++;
    }
}
