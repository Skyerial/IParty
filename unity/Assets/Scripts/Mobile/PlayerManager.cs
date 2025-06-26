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
    private static HashSet<int> currentPlayers = new HashSet<int>();
    public static PlayerManager instance;

    /**
     * @brief Temporary ranking list used to track player order during or after minigames.
     */
    public static List<PlayerStats> tempRanking = new();

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
     * @brief Maps input devices to their corresponding player stats.
     */
    public static Dictionary<InputDevice, PlayerStats> playerStats = new();

    public static string currentMinigame;

    /**
     * @brief Called when this object is initialized. Sets singleton instance and persists it across scenes.
     * @return void
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
     * @param device The player's input device.
     * @param color Player's color as a string.
     * @param name Player's display name.
     * @param face Byte array representing face texture image.
     * @return void
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

        // instance.rankGameboard.Add(playerStats[device].playerID);
        // currentPlayers++;
    }

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
     * @brief Adds a player to the temporary ranking list if not already present.
     * @param device The player's input device.
     * @return void
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
     * @return void
     */
    public void tempRankClear()
    {
        tempRanking.Clear();
    }

    /**
     * @brief Removes a player from the playerStats dictionary.
     * @param device The player's input device.
     * @return void
     */
    public static void RemovePlayer(InputDevice device)
    {   
        currentPlayers.Remove(playerStats[device].playerID);
        playerStats.Remove(device);
        // currentPlayers--;
    }

    /**
     * @brief Increments the position value for a player.
     * @param device The player's input device.
     * @param increment Value to add to the player's position.
     * @return void
     */
    public static void AddPosition(InputDevice device, int increment)
    {
        playerStats[device].position += increment;
    }

    /**
     * @brief Returns the color material associated with the player's selected color.
     * @param device The player's input device.
     * @return Material representing the player's color.
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
     * @param device The player's input device.
     * @return Texture2D of the player's face, or blank if not available.
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

    public static void changeCurrentMinigame(string name)
    {
        currentMinigame = name;
    }
}
