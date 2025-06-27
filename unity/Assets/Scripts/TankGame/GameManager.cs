using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public float labelDisplayTime  = 5f;
    private static GameManager instance;
    public static bool gameActive = true;
    public static List<PlayerInput> gamePlayers = new List<PlayerInput>();
    private static List<InputDevice> deathOrder = new();
    private SwitchScene sceneSwitcher;
    public int countDownStartNumber;
    public TMP_Text countDownText;
    public int countDownCount;
    public Canvas countDownCanvas;


    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        sceneSwitcher = GetComponent<SwitchScene>();
        AudioManager audioHandler = FindAnyObjectByType<AudioManager>();
        audioHandler.PlayRandomMiniGameTrack();
        ServerManager.SendtoAllSockets("tank");
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

        instance.StartCoroutine(instance.ShowPlayerLabels());
    }

    private IEnumerator ShowPlayerLabels()
    {
        foreach (var pi in gamePlayers)
        {
            var labelGO = pi.transform.Find("PlayerLabelCanvas").gameObject;
            labelGO.SetActive(true);

            var img = labelGO.GetComponentInChildren<Image>();
            img.color = PlayerManager.findColor(pi.devices[0]).color;

            var txt = labelGO.GetComponentInChildren<TMP_Text>();
            txt.text = PlayerManager.playerStats[pi.devices[0]].name;
        }

        yield return new WaitForSecondsRealtime(labelDisplayTime);

        foreach (var pi in gamePlayers)
        {
            var labelGO = pi.transform.Find("PlayerLabelCanvas").gameObject;
            labelGO.SetActive(false);
        }
    }

    public static void PlayerDied(PlayerInput player)
    {
        Debug.Log(player + " died!");
        gamePlayers.Remove(player);
        if (!deathOrder.Contains(player.devices[0]))
        {
            deathOrder.Insert(0, player.devices[0]);
        }
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
            deathOrder.Reverse();
            foreach (var dev in deathOrder)
            {
                PlayerManager.instance.tempRankAdd(dev);
            }
            if (gamePlayers.Count == 1)
            {
                InputDevice winnerDevice = gamePlayers[0].devices[0];
                PlayerManager.instance.tempRankAdd(winnerDevice);
            }

            instance.EndGameAndLoadScene();

        }
    }

    public void EndGameAndLoadScene()
    {
        if (sceneSwitcher != null)
            sceneSwitcher.LoadNewScene("WinScreen");
        else
            SceneManager.LoadScene("WinScreen");
    }

}