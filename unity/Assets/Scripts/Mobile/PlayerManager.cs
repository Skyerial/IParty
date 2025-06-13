using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerManager : MonoBehaviour
{
    private static PlayerManager instance;
    public class PlayerStats
    {
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
            position = 0,
            color = color,
            name = name
        };
    }

    public static void RemovePlayer(InputDevice device)
    {
        playerStats.Remove(device);
    }

    public static void AddPosition(InputDevice device, int position)
    {
        playerStats[device].position += position;
    }
}
