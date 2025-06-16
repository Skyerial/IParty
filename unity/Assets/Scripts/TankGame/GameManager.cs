using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private static GameManager instance;
    public static bool gameActive = true;
    public static List<PlayerInput> gamePlayers = new List<PlayerInput>();
    public int countDownStartNumber;
    public TMP_Text countDownText;
    public int countDownCount;
    public Canvas countDownCanvas;


    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    public void StartCountDown()
    {
        countDownCount = countDownStartNumber;
        countDownCanvas.gameObject.SetActive(true);

        Time.timeScale = 0;
        StartCoroutine(CountDownCo());
    }

    private IEnumerator CountDownCo()
    {
        if (countDownCount > 0)
        {
            countDownText.text = countDownCount.ToString();
        }
        else
        {
            countDownText.text = "Start!";
        }


        yield return new WaitForSecondsRealtime(1f);
        countDownCount--;
        if (countDownCount >= 0)
        {
            StartCoroutine(CountDownCo());
        }
        else
        {
            Debug.Log("Done!");
            countDownCanvas.gameObject.SetActive(false);
            Time.timeScale = 1;
        }
    }

    void ActivateAllInput()
    {
        foreach (var player in gamePlayers)
        {
            player.ActivateInput();
        }
    }
    public static void RegisterPlayerGame(PlayerInput player)
    {
        Debug.Log(player);
        gamePlayers.Add(player);
        player.DeactivateInput();
    }

    public static void PlayerDied(PlayerInput player)
    {
        Debug.Log(player + " died!");
        gamePlayers.Remove(player);
        CheckForGameEnd();
    }

    public void StartGame()
    {
        ActivateAllInput();
        StartCountDown();
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