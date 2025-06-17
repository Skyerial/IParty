using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private static int currentPlayers = 0;
    private static PlayerManager instance; 
    public static List<PlayerStats> tempRanking = new();
    public class PlayerStats
    {
        public int playerID;
        public int position;
        public string color;
        public bool winner;
        public string name;
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

    public static void RegisterPlayer(InputDevice device, string color, string name)
    {
        playerStats[device] = new PlayerStats
        {
            playerID = currentPlayers,
            position = 0,
            color = color,
            name = name
        };

        currentPlayers++;
        tempRanking.Add(playerStats[device]);
    }

    public void tempRankAdd(InputDevice device)
    {
        if (!tempRanking.Contains(playerStats[device]))
        {
            tempRanking.Add(playerStats[device]);
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

    public static void AddPosition(InputDevice device, int position)
    {
        playerStats[device].position += position;

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
}
