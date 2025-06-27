using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
 * @brief Manages the game state for the tank game.
 * Handles player input activation, displays player labels, and manages the game lifecycle.
 */
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

    /**
     * @brief Gets the singleton instance of GameManager and initializes the scene switcher.
     */
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);

        sceneSwitcher = GetComponent<SwitchScene>();
        AudioManager audioHandler = FindAnyObjectByType<AudioManager>();
        audioHandler.PlayRandomMiniGameTrack();
        ServerManager.SendtoAllSockets("tank");
    }

    /**
     * @brief Starts the countdown timer for the game.
     * Activates the countdown canvas and pauses the game time.
     */
    public void StartCountDown()
    {
        countDownCount = countDownStartNumber;
        countDownCanvas.gameObject.SetActive(true);

        Time.timeScale = 0;
        StartCoroutine(CountDownCo());
    }

    /**
     * @brief Coroutine that handles the countdown logic.
     * Updates the countdown text and manages the countdown timer.
     */
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

    /**
     * @brief Activates all player inputs.
     * Used to unpause the game or when players are not active.
     */
    void ActivateAllInput()
    {
        foreach (var player in gamePlayers)
        {
            player.ActivateInput();
        }
    }

    /**
     * @brief Registers a player input for the game.
     * Adds the player to the gamePlayers list and starts showing player labels.
     * @param player The PlayerInput instance of the player to register.
     */
    public static void RegisterPlayerGame(PlayerInput player)
    {
        Debug.Log(player);
        gamePlayers.Add(player);
        player.DeactivateInput();

        instance.StartCoroutine(instance.ShowPlayerLabels());
    }

    /**
     * @brief Shows player labels for all registered players.
     * Displays the player's name and color on their respective label canvas.
     */
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

    /**
     * @brief Handles player death.
     * Removes the player from the gamePlayers list and updates the death order.
     * Checks if the game has ended after a player dies.
     * @param player The PlayerInput instance of the player that died.
     */
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

    /**
     * @brief Starts the game.
     * Activates all player inputs and starts the countdown timer.
     */
    public void StartGame()
    {
        ActivateAllInput();
        StartCountDown();
    }

    /**
     * @brief Checks if the game has ended.
     * If only one player remains, the game is marked as inactive and the winner is announced.
     * Updates the death order and adds ranks to players based on their death order.
     */
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

    /** @brief Ends the game and loads the win screen scene.
     * Uses the scene switcher if available, otherwise uses SceneManager to load the scene.
     */
    public void EndGameAndLoadScene()
    {
        if (sceneSwitcher != null)
            sceneSwitcher.LoadNewScene("WinScreen");
        else
            SceneManager.LoadScene("WinScreen");
    }

}