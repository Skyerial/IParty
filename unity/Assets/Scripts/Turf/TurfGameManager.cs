using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class TurfGameManager : MonoBehaviour
{
    public static TurfGameManager Instance { get; private set; }

    [Header("UI Setup")]
    public GameObject playerUIPrefab;
    public RectTransform uiParent;
    public Vector2 uiOffset = new Vector2(10f, -10f);

    [Header("Pre-Game Countdown")]
    public Canvas countdownCanvas;
    public TMP_Text countdownText;
    public int countdownStart = 3;

    [Header("Match Settings")]
    public float matchDuration = 60f;
    public string nextSceneName;
    public Canvas matchTimerCanvas;
    public TMP_Text matchTimerText;

    [Header("Finish Settings")]
    public Canvas finishCanvas;
    public TMP_Text finishText;
    public float finishDisplayTime = 3f;

    class PlayerEntry
    {
        public InputDevice Device;
        public Color TurfColor;
        public Image Swatch;
        public TMP_Text PercentText;
        public float Percentage;
    }
    private readonly Dictionary<PlayerInput, PlayerEntry> players = new Dictionary<PlayerInput, PlayerEntry>();

    private TurfPaintableSurface[] allTiles;
    private int totalTiles = 0;
    private SwitchScene sceneSwitcher;

    void Awake()
    {
        sceneSwitcher = GetComponent<SwitchScene>();
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        allTiles = Object.FindObjectsByType<TurfPaintableSurface>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );
        totalTiles = allTiles.Length;

        countdownCanvas?.gameObject.SetActive(false);
        matchTimerCanvas?.gameObject.SetActive(false);
        finishCanvas?.gameObject.SetActive(false);
        ServerManager.SendtoAllSockets("turf");
    }

    public static void RegisterPlayerGame(PlayerInput pi)
    {
        var dev = pi.devices[0];
        var mat = PlayerManager.findColor(dev);

        var body = pi.GetComponentsInChildren<SkinnedMeshRenderer>()
                     .First(r => r.name == "Body.008");
        body.material = mat;

        pi.DeactivateInput();
        Instance.AddPlayer(pi, dev, mat.color);
    }

    private void AddPlayer(PlayerInput pi, InputDevice dev, Color turfColor)
    {
        // Instantiate UI
        var uiObj = Instantiate(playerUIPrefab, uiParent);
        var rect = uiObj.GetComponent<RectTransform>();
        int idx = players.Count;
        SetAnchor(rect, idx);

        Vector2 pos = uiOffset;
        if ((idx & 1) != 0) pos.x = -pos.x;
        if ((idx & 2) != 0) pos.y = -pos.y;
        rect.anchoredPosition = pos;

        var sw = uiObj.transform.Find("Swatch").GetComponent<Image>();
        var txt = uiObj.transform.Find("PercentText").GetComponent<TMP_Text>();
        sw.color = turfColor;
        txt.text = "0.0%";

        // Store entry
        players[pi] = new PlayerEntry
        {
            Device = dev,
            TurfColor = turfColor,
            Swatch = sw,
            PercentText = txt,
            Percentage = 0f
        };
    }

    void Update()
    {
        foreach (var entry in players.Values)
        {
            int owned = allTiles.Count(t => t.CurrentColor == entry.TurfColor);
            float pct = totalTiles > 0 ? (owned / (float)totalTiles) * 100f : 0f;
            entry.Percentage = pct;
            entry.PercentText.text = $"{pct:0.0}%";
        }
    }

    void SetAnchor(RectTransform rt, int idx)
    {
        switch (idx)
        {
            case 0:
                rt.anchorMin = rt.anchorMax = new Vector2(0, 1);
                rt.pivot = new Vector2(0, 1);
                break;
            case 1:
                rt.anchorMin = rt.anchorMax = new Vector2(1, 1);
                rt.pivot = new Vector2(1, 1);
                break;
            case 2:
                rt.anchorMin = rt.anchorMax = new Vector2(0, 0);
                rt.pivot = new Vector2(0, 0);
                break;
            default:
                rt.anchorMin = rt.anchorMax = new Vector2(1, 0);
                rt.pivot = new Vector2(1, 0);
                break;
        }
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

        foreach (var pi in players.Keys)
            pi.ActivateInput();

        StartCoroutine(MatchTimer());
    }

    IEnumerator MatchTimer()
    {
        matchTimerCanvas?.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < matchDuration)
        {
            elapsed += Time.deltaTime;
            UpdateMatchTimerText(matchDuration - elapsed);
            yield return null;
        }

        matchTimerCanvas?.gameObject.SetActive(false);

        foreach (var pi in players.Keys)
            pi.DeactivateInput();

        finishCanvas?.gameObject.SetActive(true);
        finishText.text = "Finish";

        FinalizeRanking();
        yield return new WaitForSecondsRealtime(finishDisplayTime);

        sceneSwitcher.LoadNewScene(nextSceneName);
    }

    void UpdateMatchTimerText(float remaining)
    {
        if (matchTimerText == null) return;
        int seconds = Mathf.Max(0, Mathf.CeilToInt(remaining));
        int mins = seconds / 60;
        int secs = seconds % 60;
        matchTimerText.text = $"{mins}:{secs:00}";
    }

    public void FinalizeRanking()
    {
        var pm = Object.FindFirstObjectByType<PlayerManager>();
        pm.tempRankClear();

        var sortedDevices = players
            .OrderBy(kvp => kvp.Value.Percentage)
            .Select(kvp => kvp.Value.Device);

        foreach (var device in sortedDevices)
            pm.tempRankAdd(device);
    }
    
    public Color GetPlayerColor(PlayerInput pi)
    {
        if (players.TryGetValue(pi, out var entry))
            return entry.TurfColor;
        Debug.LogWarning($"No entry found for {pi}; defaulting to white.");
        return Color.white;
    }
}
