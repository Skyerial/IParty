using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

/**
 * @brief Handles UI and game flow when a player disconnects and reconnects.
 */
public class Reconnect : MonoBehaviour
{
    /**
     * @brief Panel GameObject displayed to prompt reconnection.
     */
    public GameObject ReconnectPanel;

    /**
     * @brief Text component showing the disconnected player's name.
     */
    public TMP_Text Name;

    /**
     * @brief Image component displaying the disconnected player's face texture.
     */
    public RawImage face;

    /**
     * @brief Text component showing the reconnect code.
     */
    public TMP_Text reconnectCode;

    /**
     * @brief Flag indicating whether a disconnect event is currently active.
     */
    public bool disconnected;

    /**
     * @brief Unity event called when the script instance is loaded; initializes state and persists across scenes.
     */
    void Awake()
    {
        disconnected = false;
        DontDestroyOnLoad(gameObject);
    }

    /**
     * @brief Triggers the disconnect UI and pauses the game.
     * @param id The identifier of the disconnected player.
     * @param code The reconnect code to display.
     */
    public void DisconnectEvent(string id, string code)
    {
        disconnected = true;
        ReconnectPanel.SetActive(true);
        Time.timeScale = 0;
        MessageUpdate(id, code);
    }

    /**
     * @brief Hides the reconnect UI and resumes the game when reconnection is complete.
     */
    public void ReconnectEvent()
    {
        ReconnectPanel.SetActive(false);
        Time.timeScale = 1;
        disconnected = false;
    }

    /**
     * @brief Updates the reconnect panel with the player's name, face, and code.
     * @param id The identifier of the disconnected player.
     * @param code The reconnect code to display.
     */
    void MessageUpdate(string id, string code)
    {
        var device = ServerManager.allControllers[id];
        var name = PlayerManager.playerStats[device].name;
        Texture2D faceTexture = PlayerManager.findFace(device);
        Name.text = name;
        face.texture = faceTexture;
        reconnectCode.text = code;
    }
}
