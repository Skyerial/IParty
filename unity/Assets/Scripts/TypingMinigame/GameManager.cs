using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public List<PlayerTypingController> players;

    private bool gameStarted = false;

    public int finishCount = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        gameStarted = true;
        Debug.Log("Game started!");

        foreach (var player in players)
        {
            player.inputField.interactable = true;
            player.inputField.text = "";
        }
    }

    public void OnPlayerFinished(PlayerTypingController player)
    {
        // Debug.Log($"{player.name} has completed all words!");
        finishCount++;
        Debug.Log($"{player.name} finished in position {finishCount}");

        // Optional: disable input
        player.inputField.interactable = false;

        // Optional: check if everyone is done
        if (AllPlayersFinished())
        {
            Debug.Log("Game over!");
            // Add win screen or restart logic
        }
    }

    private bool AllPlayersFinished()
    {
        foreach (var player in players)
        {
            if (player.spawner.transform.childCount > 0)
                return false;
        }
        return true;
    }
}
