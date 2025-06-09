using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    // public static GameManager Instance;


    // public bool gameActive = true;

    // void Awake()
    // {
    //     if (Instance == null) Instance = this;
    //     else Destroy(gameObject);
    // }

    // public void RegisterPlayer(PlayerController player)
    // {
    //     players.Add(player);
    // }

    // public void OnPlayerDied(PlayerController player)
    // {
    //     players.Remove(player);
    //     CheckForGameEnd();
    // }

    // void CheckForGameEnd()
    // {
    //     if (players.Count <= 1 && gameActive)
    //     {
    //         gameActive = false;
    //         Debug.Log("Game Over!");

    //         // Trigger win/lose, show UI, etc.
    //     }
    // }
}