using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;


public class GameManagerGyro : MonoBehaviour
{
    public static GameManagerGyro Instance { get; private set; }

    [Header("Countdown UI")]
    public Canvas countdownCanvas;
    public TMP_Text countdownText;
    public int countdownStart = 3;

    [Header("Game Timer")]
    public float matchDuration = 60f;
    public Canvas timerCanvas;
    public TMP_Text timerText;

    [Header("Finish UI")]
    public Canvas finishCanvas;
    public TMP_Text finishText;
    public float finishDisplayTime = 3f;

    private Dictionary<PlayerInput, int> moleHits = new();
    private List<PlayerInput> allPlayers = new();
    private SwitchScene sceneSwitcher;


    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        sceneSwitcher = GetComponent<SwitchScene>();
        ServerManager.SendtoAllSockets("gyro");
    }

    public void RegisterPlayer(PlayerInput player)
    {
        if (!allPlayers.Contains(player))
        {
            allPlayers.Add(player);
            moleHits[player] = 0;
            player.DeactivateInput();

            InputDevice dev = player.devices[0];

            // Baton kleuren
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
        }
    }


    public void AddMoleHit(PlayerInput player)
    {
        if (moleHits.ContainsKey(player))
            moleHits[player]++;
    }

    public void RemoveMoleHit(PlayerInput player)
    {
        if (moleHits.ContainsKey(player) && moleHits[player] > 0)
            moleHits[player]--;
    }

    public int GetMoleHits(PlayerInput player)
    {
        return moleHits.ContainsKey(player) ? moleHits[player] : 0;
    }

    public void StartGame()
    {
        StartCoroutine(GameFlow());
    }

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

    IEnumerator TimerRoutine()
    {
        timerCanvas?.gameObject.SetActive(true);
        float elapsed = 0f;
        while (elapsed < matchDuration)
        {
            elapsed += Time.deltaTime;
            float remaining = matchDuration - elapsed;
            int sec = Mathf.CeilToInt(remaining);
            timerText.text = sec.ToString();
            yield return null;
        }
        timerCanvas?.gameObject.SetActive(false);
    }
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
