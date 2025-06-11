using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static bool gameActive = true;
    public static List<GameObject> gamePlayers = new List<GameObject>();

    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public static void RegisterPlayerGame(GameObject player)
    {
        Debug.Log(player);
        gamePlayers.Add(player);
    }

    public static void PlayerDied(GameObject player)
    {
        Debug.Log(player);
        gamePlayers.Remove(player);
        CheckForGameEnd();
    }

    public static void CheckForGameEnd()
    {
        if (gamePlayers.Count <= 1 && gameActive)
        {
            gameActive = false;
            Debug.Log("Game Over!");
            Debug.Log("The winner is: " + gamePlayers[0]);

        }
    }
}