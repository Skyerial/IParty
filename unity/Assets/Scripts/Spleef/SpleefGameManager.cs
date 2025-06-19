// SpleefGameManager.cs
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SpleefGameManager : MonoBehaviour
{
    public static SpleefGameManager Instance { get; private set; }

    [Header("Pre-Game Countdown")]
    public Canvas   countdownCanvas;
    public TMP_Text countdownText;
    public int      countdownStart = 3;

    [Header("Finish Settings")]
    public Canvas   finishCanvas;
    public TMP_Text finishText;
    public float    finishDisplayTime = 3f;

    class PlayerEntry
    {
        public InputDevice device;
        public Color       color;
    }

    readonly List<PlayerEntry> entries = new();
    readonly List<PlayerInput>   gamePlayers     = new();
    readonly List<PlayerInput>   eliminationOrder= new();
    private SwitchScene          sceneSwitcher;

    void Awake()
    {
        sceneSwitcher = GetComponent<SwitchScene>();

        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        countdownCanvas?.gameObject.SetActive(false);
        finishCanvas?.gameObject.SetActive(false);
    }

    public static void RegisterPlayerGame(PlayerInput pi)
    {
        var dev = pi.devices[0];
        var mat = PlayerManager.findColor(dev);

        var body = pi.GetComponentsInChildren<SkinnedMeshRenderer>()
                     .First(r => r.name == "Body.008");
        body.material = mat;

        Instance.gamePlayers.Add(pi);
        pi.DeactivateInput();

        Instance.OnPlayerJoined(dev, mat.color);
    }

    void OnPlayerJoined(InputDevice dev, Color playercolor)
    {
        entries.Add(new PlayerEntry
        {
            device = dev,
            color = playercolor
        });
    }

    public void StartGame()
    {
        StartCoroutine(PreGameCountdown());
    }

    IEnumerator PreGameCountdown()
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

        countdownCanvas?.gameObject.SetActive(false);
        Time.timeScale = 1f;

        foreach (var pi in gamePlayers)
            pi.ActivateInput();
    }

    public void OnPlayerEliminated(PlayerInput pi)
    {
        if (!eliminationOrder.Contains(pi))
        {
            eliminationOrder.Add(pi);

            pi.DeactivateInput();
            pi.gameObject.SetActive(false);

            int aliveCount = gamePlayers.Count - eliminationOrder.Count;
            if (aliveCount <= 1)
            {
                EndGame();
                FinalizeRanking();
            }
        }
    }

    public void EndGame()
    {
        foreach (var pi in gamePlayers)
            pi.DeactivateInput();

        finishCanvas?.gameObject.SetActive(true);
        finishText.text = "Finish";
    }

    public void FinalizeRanking()
    {
        var pm = Object.FindFirstObjectByType<PlayerManager>();
        pm.tempRankClear();

        var eliminatedDevices = eliminationOrder
            .Select(pi => pi.devices[0])
            .ToList();

        var sorted = entries
            .OrderByDescending(e => eliminatedDevices.IndexOf(e.device))
            .Select(e => e.device);

        foreach (var dev in sorted)
            pm.tempRankAdd(dev);
    }

}
