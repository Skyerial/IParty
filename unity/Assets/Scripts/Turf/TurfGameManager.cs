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

    [Header("Player Labels")]
    public float labelDisplayTime  = 5f;

    [Header("UI Setup")]
    public GameObject playerUIPrefab;
    public RectTransform uiParent;
    public Vector2 uiOffset = new Vector2(10f, -10f);

    [Header("Pre-Game Countdown")]
    public Canvas countdownCanvas;
    public TMP_Text countdownText;
    public int countdownStart = 3;
    public AudioClip preGameAmbient;
    public AudioClip fullCountdownClip;

    [Header("Match Settings")]
    public float matchDuration = 60f;
    public string nextSceneName;
    public Canvas matchTimerCanvas;
    public TMP_Text matchTimerText;
    public AudioClip mainMusic;

    [Header("Finish Settings")]
    public Canvas finishCanvas;
    public TMP_Text finishText;
    public float finishDisplayTime = 3f;
    public AudioClip finishClip;

    class PlayerEntry
    {
        public InputDevice Device;
        public Color TurfColor;
        public Image Swatch;
        public TMP_Text PercentText;
        public float Percentage;
    }
    private readonly Dictionary<PlayerInput, PlayerEntry> players = new Dictionary<PlayerInput, PlayerEntry>();

    private SwitchScene sceneSwitcher;
    private AudioManager globalAudioManager;
    private AudioSource audioSource;

    private TurfPaintableSurface[] allTiles;
    private int totalTiles = 0;

    void Awake()
    {
        globalAudioManager = FindAnyObjectByType<AudioManager>();
        sceneSwitcher = GetComponent<SwitchScene>();

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        globalAudioManager?.StopMusic();

        allTiles = Object.FindObjectsByType<TurfPaintableSurface>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None
        );
        totalTiles = allTiles.Length;

        countdownCanvas?.gameObject.SetActive(false);
        matchTimerCanvas?.gameObject.SetActive(false);
        finishCanvas?.gameObject.SetActive(false);

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (preGameAmbient != null)
        {
            audioSource.clip = preGameAmbient;
            audioSource.loop = true;
            StartCoroutine(FadeIn(3f));
        }
        ServerManager.SendtoAllSockets("turf");
    }

    public static void RegisterPlayerGame(PlayerInput pi)
    {
        var dev = pi.devices[0];
        var mat = PlayerManager.findColor(dev);
        var matFace = PlayerManager.findFace(dev);

        var body = pi.GetComponentsInChildren<SkinnedMeshRenderer>()
                     .First(r => r.name == "Body.008");
        body.material = mat;

        var face = pi.GetComponentsInChildren<SkinnedMeshRenderer>()
                     .First(r => r.name == "Body.001");

        Material newMat = new Material(face.material);
        newMat.mainTexture = matFace;
        face.material = newMat;

        pi.DeactivateInput();
        Instance.AddPlayer(pi, dev, mat.color);
    }

    public void StartGame()
    {
        StartCoroutine(ShowPlayerLabels());
        StartCoroutine(PreGameCountdown());
    }

    private IEnumerator ShowPlayerLabels()
    {
        // show
        foreach (var kv in players)
        {
            var pi    = kv.Key;
            var entry = kv.Value;

            // find the pre-placed label under this player
            var labelGO = pi.transform.Find("PlayerLabelCanvas").gameObject;
            labelGO.SetActive(true);

            // tint the background
            var img = labelGO.GetComponentInChildren<Image>();
            img.color = entry.TurfColor;

            // set the name
            var txt = labelGO.GetComponentInChildren<TMP_Text>();
            txt.text = PlayerManager.playerStats[entry.Device].name;
        }

        // wait unscaled so it stays up during the countdown
        yield return new WaitForSecondsRealtime(labelDisplayTime);

        // hide
        foreach (var kv in players)
        {
            var labelGO = kv.Key.transform.Find("PlayerLabelCanvas").gameObject;
            labelGO.SetActive(false);
        }
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

    IEnumerator PreGameCountdown()
    {
        if (preGameAmbient != null)
            yield return StartCoroutine(FadeOut(1f));

        if (mainMusic != null)
        {
            audioSource.clip = mainMusic;
            audioSource.loop = true;
            StartCoroutine(FadeIn(10f));
        }

        if (fullCountdownClip != null)
            audioSource.PlayOneShot(fullCountdownClip);

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

        if (finishClip != null)
            audioSource.PlayOneShot(finishClip);

        finishCanvas?.gameObject.SetActive(true);
        finishText.text = "Finish";

        if (mainMusic != null)
            StartCoroutine(FadeOut(2f));

        FinalizeRanking();
        yield return new WaitForSecondsRealtime(finishDisplayTime);

        globalAudioManager?.PlayMusic();
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

    private IEnumerator FadeOut(float duration)
    {
        float startVol = audioSource.volume;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(startVol, 0f, elapsed / duration);
            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVol;
    }

    private IEnumerator FadeIn(float duration)
    {
        float targetVol = audioSource.volume;

        audioSource.volume = 0f;
        audioSource.Play();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            audioSource.volume = Mathf.Lerp(0f, targetVol, elapsed / duration);
            yield return null;
        }

        audioSource.volume = targetVol;
    }
}
