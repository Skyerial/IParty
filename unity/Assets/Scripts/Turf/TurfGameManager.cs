using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/**
 * @brief Manages overall Turf game flow: player registration, UI setup, countdown, match timer, and finish sequence.
 */
public class TurfGameManager : MonoBehaviour
{
    public static TurfGameManager Instance { get; private set; }

    [Header("Player Labels")]
    /**
     * @brief Duration (in seconds) to display player labels at game start.
     */
    public float labelDisplayTime  = 5f;

    [Header("UI Setup")]
    /**
     * @brief Prefab for instantiating each player's UI entry.
     */
    public GameObject playerUIPrefab;
    /**
     * @brief Parent RectTransform under which player UIs are placed.
     */
    public RectTransform uiParent;
    /**
     * @brief Offset from the anchor for positioning the player UI.
     */
    public Vector2 uiOffset = new Vector2(10f, -10f);

    [Header("Pre-Game Countdown")]
    /**
     * @brief Canvas used for the pre-game countdown.
     */
    public Canvas countdownCanvas;
    /**
     * @brief Text component displaying the countdown number.
     */
    public TMP_Text countdownText;
    /**
     * @brief Starting value for the pre-game countdown.
     */
    public int countdownStart = 3;
    /**
     * @brief Ambient audio clip played before the countdown.
     */
    public AudioClip preGameAmbient;
    /**
     * @brief Audio clip played during the countdown.
     */
    public AudioClip fullCountdownClip;

    [Header("Match Settings")]
    /**
     * @brief Total match duration in seconds.
     */
    public float matchDuration = 60f;
    /**
     * @brief Name of the scene to load after the match ends.
     */
    public string nextSceneName;
    /**
     * @brief Canvas showing the match timer.
     */
    public Canvas matchTimerCanvas;
    /**
     * @brief Text component displaying the remaining match time.
     */
    public TMP_Text matchTimerText;
    /**
     * @brief Main background music clip for the match.
     */
    public AudioClip mainMusic;

    [Header("Finish Settings")]
    /**
     * @brief Canvas displayed at match end.
     */
    public Canvas finishCanvas;
    /**
     * @brief Text component shown on the finish canvas.
     */
    public TMP_Text finishText;
    /**
     * @brief Duration (in seconds) to display the finish screen.
     */
    public float finishDisplayTime = 3f;
    /**
     * @brief Audio clip played when the match finishes.
     */
    public AudioClip finishClip;

    /**
     * @brief Internal structure for storing player device, turf color, UI swatch, percentage text, and coverage percentage.
     */
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

    /**
     * @brief Unity event called when the script instance is loaded; initializes singletons and references.
     */
    void Awake()
    {
        globalAudioManager = FindAnyObjectByType<AudioManager>();
        sceneSwitcher = GetComponent<SwitchScene>();

        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    /**
     * @brief Unity event called on the first frame; stops music, gathers tiles, hides UI, configures audio, and notifies server.
     */
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

    /**
     * @brief Registers a new player: applies materials, deactivates input, and adds player entry.
     * @param pi The PlayerInput instance for the joining player.
     */
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

    /**
     * @brief Instantiates and positions UI for a player, stores their entry, and shows labels.
     * @param pi The PlayerInput instance.
     * @param dev The InputDevice associated with the player.
     * @param turfColor The color assigned to the player’s turf.
     */
    private void AddPlayer(PlayerInput pi, InputDevice dev, Color turfColor)
    {
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

        players[pi] = new PlayerEntry
        {
            Device = dev,
            TurfColor = turfColor,
            Swatch = sw,
            PercentText = txt,
            Percentage = 0f
        };

        StartCoroutine(ShowPlayerLabels());
    }

    /**
     * @brief Starts the game by launching the pre-game countdown coroutine.
     */
    public void StartGame()
    {
        StartCoroutine(PreGameCountdown());
    }

    /**
     * @brief Coroutine to display player name labels above characters for a set duration.
     * @return IEnumerator for coroutine control.
     */
    private IEnumerator ShowPlayerLabels()
    {
        foreach (var kv in players)
        {
            var pi    = kv.Key;
            var entry = kv.Value;

            var labelGO = pi.transform.Find("PlayerLabelCanvas").gameObject;
            labelGO.SetActive(true);

            var img = labelGO.GetComponentInChildren<Image>();
            img.color = entry.TurfColor;

            var txt = labelGO.GetComponentInChildren<TMP_Text>();
            txt.text = PlayerManager.playerStats[entry.Device].name;
        }

        yield return new WaitForSecondsRealtime(labelDisplayTime);

        foreach (var kv in players)
        {
            var labelGO = kv.Key.transform.Find("PlayerLabelCanvas").gameObject;
            labelGO.SetActive(false);
        }
    }

    /**
     * @brief Unity event called every frame; updates each player's coverage percentage in the UI.
     */
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

    /**
     * @brief Sets the anchor and pivot of a RectTransform based on player index.
     * @param rt The RectTransform to configure.
     * @param idx The player index determining corner placement.
     */
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

    /**
     * @brief Coroutine handling the countdown before match start, with audio fades and time scale control.
     * @return IEnumerator for coroutine control.
     */
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

    /**
     * @brief Coroutine that runs the match timer, updates UI, disables input, shows finish UI, and transitions scene.
     * @return IEnumerator for coroutine control.
     */
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

    /**
     * @brief Updates the match timer text to show remaining minutes and seconds.
     * @param remaining Time remaining in seconds.
     */
    void UpdateMatchTimerText(float remaining)
    {
        if (matchTimerText == null) return;
        int seconds = Mathf.Max(0, Mathf.CeilToInt(remaining));
        int mins = seconds / 60;
        int secs = seconds % 60;
        matchTimerText.text = $"{mins}:{secs:00}";
    }

    /**
     * @brief Orders players by coverage percentage and updates the PlayerManager’s ranking.
     */
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

    /**
     * @brief Retrieves the assigned turf color for the given player, defaulting to white if not found.
     * @param pi The PlayerInput instance.
     * @return The player's assigned turf color.
     */
    public Color GetPlayerColor(PlayerInput pi)
    {
        if (players.TryGetValue(pi, out var entry))
            return entry.TurfColor;
        Debug.LogWarning($"No entry found for {pi}; defaulting to white.");
        return Color.white;
    }

    /**
     * @brief Coroutine that fades out the audio source over a specified duration and stops playback.
     * @param duration The duration of the fade-out in seconds.
     * @return IEnumerator for coroutine control.
     */
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

    /**
     * @brief Coroutine that fades in the audio source over a specified duration.
     * @param duration The duration of the fade-in in seconds.
     * @return IEnumerator for coroutine control.
     */
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
