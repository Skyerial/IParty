using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private static int currentPlayers = 0;
    public static PlayerManager instance; 
    public static List<PlayerStats> tempRanking = new();
    public class PlayerStats
    {
        public int playerID;
        public int position;
        public string color;
        public int winner;
        public string name;
        public byte[] face;
    }

    public static Dictionary<InputDevice, PlayerStats> playerStats = new();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject); // Persist through scenes
        }
        else
        {
            Destroy(gameObject); // Prevent duplicates
        }
    }

    public static void RegisterPlayer(InputDevice device, string color, string name, byte[] face)
    {
        playerStats[device] = new PlayerStats
        {
            playerID = currentPlayers,
            position = 0,
            color = color,
            winner = 0,
            name = name,
            face = face
        };

        currentPlayers++;
        // tempRanking.Add(playerStats[device]);
    }

    public void tempRankAdd(InputDevice device)
    {
        Debug.Log($"device: {device}");
        if (!tempRanking.Contains(playerStats[device]))
        {
            tempRanking.Insert(0, playerStats[device]);
        }
    }

    public void tempRankClear()
    {
        tempRanking.Clear();
    }


    public static void RemovePlayer(InputDevice device)
    {
        playerStats.Remove(device);
        currentPlayers--;
    }

    public static void AddPosition(InputDevice device, int increment)
    {
        playerStats[device].position += increment;

    }

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
}
