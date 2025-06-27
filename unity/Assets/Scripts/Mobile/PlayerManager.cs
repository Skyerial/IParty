using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/**
 * @brief Manages player data, colors, faces, rankings, and persistence between scenes.
 */
public class PlayerManager : MonoBehaviour
{
    private const int MaxPlayers = 4;
    /**
     * @brief Tracks which player IDs have already been assigned.
     */
    private static HashSet<int> currentPlayers = new HashSet<int>();
    /**
     * @brief Singleton instance reference.
     */
    public static PlayerManager instance;

    /**
     * @brief Temporary ranking list used to track player order during or after minigames.
     */
    public static List<PlayerStats> tempRanking = new();

    /**
     * @brief List representing final rankings on the gameboard.
     */
    public List<int> rankGameboard = new();

    /**
     * @brief Stores player-specific information.
     */
    public class PlayerStats
    {
        public int playerID;
        public int position;
        public string color;
        public int winner;
        public string name;
        public byte[] face;
    }

    /**
     * @brief Maps input devices to their corresponding PlayerStats.
     */
    public static Dictionary<InputDevice, PlayerStats> playerStats = new();

    /**
     * @brief Name of the current minigame in play.
     */
    public static string currentMinigame;
    /**
     * @brief Flag indicating whether the gameboard has been initialized before.
     */
    public static bool firstTimeBoard = true;

    /**
     * @brief Unity Awake event; sets up singleton instance and persists across scenes.
     */
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /**
     * @brief Registers a new player with their device, color, name, and face image.
     * @param device The player's InputDevice.
     * @param color The player's chosen color as a string.
     * @param name The player's display name.
     * @param face Byte array representing the player's face texture.
     */
    public static void RegisterPlayer(InputDevice device, string color, string name, byte[] face)
    {
        playerStats[device] = new PlayerStats
        {
            playerID = AssignID(),
            position = 0,
            color = color,
            winner = 0,
            name = name,
            face = face
        };
    }

    /**
     * @brief Assigns the next available player ID up to MaxPlayers.
     * @return The assigned player ID, or 5 if none available.
     */
    public static int AssignID()
    {
        for (int id = 0; id < MaxPlayers; id++)
        {
            if (!currentPlayers.Contains(id))
            {
                currentPlayers.Add(id);
                return id;
            }
        }

        return 5;
    }

    /**
     * @brief Adds a player's stats to the temporary ranking list if not already present.
     * @param device The player's InputDevice whose stats to add.
     */
    public void tempRankAdd(InputDevice device)
    {
        Debug.Log($"device: {device}");
        if (!tempRanking.Contains(playerStats[device]))
        {
            tempRanking.Insert(0, playerStats[device]);
        }
    }

    /**
     * @brief Clears the temporary ranking list.
     */
    public void tempRankClear()
    {
        tempRanking.Clear();
    }

    /**
     * @brief Removes a player from tracking when they leave the game.
     * @param device The player's InputDevice to remove.
     */
    public static void RemovePlayer(InputDevice device)
    {
        currentPlayers.Remove(playerStats[device].playerID);
        playerStats.Remove(device);
    }

    /**
     * @brief Increments the stored position value for a specific player.
     * @param device The player's InputDevice whose position to increment.
     * @param increment The amount to add to the player's position.
     */
    public static void AddPosition(InputDevice device, int increment)
    {
        playerStats[device].position += increment;
    }

    /**
     * @brief Retrieves the Material associated with the player's selected color.
     * @param device The player's InputDevice whose color to find.
     * @return A Material corresponding to the player's chosen color.
     */
    public static Material findColor(InputDevice device)
    {
        Material mat = Resources.Load<Material>("Materials/Default");
        switch (playerStats[device].color)
        {
            case "Yellow":
                mat = Resources.Load<Material>("Materials/Global/Yellow");
                break;
            case "Red":
                mat = Resources.Load<Material>("Materials/Global/Red");
                break;
            case "Green":
                mat = Resources.Load<Material>("Materials/Global/Green");
                break;
            case "Blue":
                mat = Resources.Load<Material>("Materials/Global/Blue");
                break;
        }
        return mat;
    }

    /**
     * @brief Converts a player's face byte array into a Texture2D object.
     * @param device The player's InputDevice whose face to load.
     * @return A Texture2D of the player's face, or a blank texture if unavailable.
     */
    public static Texture2D findFace(InputDevice device)
    {
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(playerStats[device].face);
        if (texture == null)
        {
            Debug.Log("There is no face data for " + playerStats[device].name);
            return new Texture2D(2, 2);
        }

        return texture;
    }

    /**
     * @brief Updates the current minigame name.
     * @param name The name of the new current minigame.
     */
    public static void changeCurrentMinigame(string name)
    {
        currentMinigame = name;
    }
}
