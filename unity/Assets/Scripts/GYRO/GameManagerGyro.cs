using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/**
 * @brief Manages the overall flow of the Whack-a-Mole-style minigame, including player registration, countdown, game timing, scoring, and finishing.
 */
public class GameManagerGyro : MonoBehaviour
{
    /**
     * @brief Singleton instance for global access.
     */
    public static GameManagerGyro Instance { get; private set; }

    [Header("Player Labels")]
    /**
     * @brief Time in seconds to show player labels at the start.
     */
    public float labelDisplayTime  = 5f;

    [Header("Countdown UI")]
    /**
     * @brief Canvas containing the countdown text.
     */
    public Canvas countdownCanvas;
    /**
     * @brief Text element used to display countdown numbers.
     */
    public TMP_Text countdownText;
    /**
     * @brief Starting value of the countdown.
     */
    public int countdownStart = 3;

    [Header("Game Timer")]
    /**
     * @brief Total duration of the game in seconds.
     */
    public float matchDuration = 60f;
    /**
     * @brief Canvas displaying the game timer.
     */
    public Canvas timerCanvas;
    /**
     * @brief Text element showing remaining game time.
     */
    public TMP_Text timerText;

    [Header("Finish UI")]
    /**
     * @brief Canvas shown at the end of the game.
     */
    public Canvas finishCanvas;
    /**
     * @brief Text shown when the game finishes.
     */
    public TMP_Text finishText;
    /**
     * @brief Duration to show the finish screen.
     */
    public float finishDisplayTime = 3f;

    /**
     * @brief Tracks how many moles each player hit.
     */
    private Dictionary<PlayerInput, int> moleHits = new();
    /**
     * @brief List of all players participating in the game.
     */
    private List<PlayerInput> allPlayers = new();
    /**
     * @brief Reference to the SwitchScene component for loading next scenes.
     */
    private SwitchScene sceneSwitcher;

    /**
     * @brief Initializes the singleton and other components, sends controller to all sockets, and plays music.
     */
    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        sceneSwitcher = GetComponent<SwitchScene>();
        ServerManager.SendtoAllSockets("whackamole");
        AudioManager audioHandler = FindAnyObjectByType<AudioManager>();
        audioHandler.PlayRandomMiniGameTrack();
    }

    /**
     * @brief Registers a player into the game and initializes their baton color and input state.
     * @param player The joining player's PlayerInput.
     */
    public void RegisterPlayer(PlayerInput player)
    {
        if (!allPlayers.Contains(player))
        {
            allPlayers.Add(player);
            moleHits[player] = 0;
            player.DeactivateInput();

            InputDevice dev = player.devices[0];

            var baton = player.GetComponentsInChildren<Renderer>()
                            .FirstOrDefault(r => r.name == "Cilindro.013");
            if (baton != null && PlayerManager.playerStats.ContainsKey(dev))
            {
                baton.material = PlayerManager.findColor(dev);
            }
            else
            {
                Debug.LogWarning($"Baton niet gevonden of speler niet geregistreerd: {dev}");
            }

            StartCoroutine(ShowPlayerLabels());
        }
    }

    /**
     * @brief Coroutine to display each player's label and hide them after a delay.
     * @return IEnumerator for coroutine control.
     */
    private IEnumerator ShowPlayerLabels()
    {
        foreach (var pi in allPlayers)
        {
            var labelGO = pi.transform.Find("PlayerLabelCanvas").gameObject;
            labelGO.SetActive(true);

            var img = labelGO.GetComponentInChildren<Image>();
            img.color = PlayerManager.findColor(pi.devices[0]).color;

            var txt = labelGO.GetComponentInChildren<TMP_Text>();
            txt.text = PlayerManager.playerStats[pi.devices[0]].name;
        }

        yield return new WaitForSecondsRealtime(labelDisplayTime);

        foreach (var pi in allPlayers)
        {
            var labelGO = pi.transform.Find("PlayerLabelCanvas").gameObject;
            labelGO.SetActive(false);
        }
    }

    /**
     * @brief Increments the mole hit counter for a given player.
     * @param player The PlayerInput of the player who hit a mole.
     */
    public void AddMoleHit(PlayerInput player)
    {
        if (moleHits.ContainsKey(player))
            moleHits[player]++;
    }

    /**
     * @brief Decrements the mole hit counter for a given player.
     * @param player The PlayerInput of the player to update.
     */
    public void RemoveMoleHit(PlayerInput player)
    {
        if (moleHits.ContainsKey(player) && moleHits[player] > 0)
            moleHits[player]--;
    }

    /**
     * @brief Gets the current mole hit count for a specific player.
     * @param player The PlayerInput of the player.
     * @return Number of moles hit by the player.
     */
    public int GetMoleHits(PlayerInput player)
    {
        return moleHits.ContainsKey(player) ? moleHits[player] : 0;
    }

    /**
     * @brief Starts the minigame flow: countdown, gameplay, and finish.
     */
    public void StartGame()
    {
        StartCoroutine(GameFlow());
    }

    /**
     * @brief Main coroutine managing game stages.
     * @return IEnumerator for coroutine control.
     */
    private IEnumerator GameFlow()
    {
        yield return StartCoroutine(CountdownRoutine());

        foreach (var p in allPlayers)
            p.ActivateInput();

        yield return StartCoroutine(TimerRoutine());

        foreach (var p in allPlayers)
            p.DeactivateInput();

        finishCanvas?.gameObject.SetActive(true);
        finishText.text = "Finish!";

        yield return new WaitForSecondsRealtime(finishDisplayTime);

        FinalizeRanking();
        if (sceneSwitcher != null)
        {
            sceneSwitcher.LoadNewScene("WinScreen");
        }
        else
        {
            SceneManager.LoadScene("WinScreen");
        }
    }

    /**
     * @brief Coroutine for displaying the countdown before the game starts.
     * @return IEnumerator for coroutine control.
     */
    IEnumerator CountdownRoutine()
    {
        countdownCanvas?.gameObject.SetActive(true);
        Time.timeScale = 0f;

        int counter = countdownStart;
        while (counter > 0)
        {
            countdownText.text = counter.ToString();
            yield return new WaitForSecondsRealtime(1f);
            counter--;
        }

        countdownText.text = "Start!";
        yield return new WaitForSecondsRealtime(1f);
        countdownText.text = "";
        countdownCanvas?.gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    /**
     * @brief Coroutine to track and update the game timer until match ends.
     * @return IEnumerator for coroutine control.
     */
    IEnumerator TimerRoutine()
    {
        timerCanvas?.gameObject.SetActive(true);
        float elapsed = 0f;
        while (elapsed < matchDuration)
        {
            elapsed += Time.deltaTime;
            float remaining = matchDuration - elapsed;
            int sec = Mathf.CeilToInt(remaining);
            if (sec <= 10)
            {
                timerText.color = Color.red;
            }
            timerText.text = sec.ToString();
            yield return null;
        }
        timerCanvas?.gameObject.SetActive(false);
    }

    /**
     * @brief Orders players by fewest mole hits and sends the ranking to PlayerManager.
     */
    private void FinalizeRanking()
    {
        var pm = Object.FindFirstObjectByType<PlayerManager>();
        if (pm == null) return;

        pm.tempRankClear();

        var sorted = moleHits
            .OrderBy(pair => pair.Value)
            .Select(pair => pair.Key.devices[0]);

        foreach (var dev in sorted)
        {
            pm.tempRankAdd(dev);
        }
    }
}
