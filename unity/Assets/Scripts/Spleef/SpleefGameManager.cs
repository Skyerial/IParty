using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

/**
 * @brief Manages overall Spleef game flow: player registration, countdown, tile drops, and finish sequence.
 */
public class SpleefGameManager : MonoBehaviour
{
    public static SpleefGameManager Instance { get; private set; }

    [Header("Player Labels")]
    /**
     * @brief Duration (in seconds) to display player labels at game start.
     */
    public float labelDisplayTime  = 5f;

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
     * @brief Starting count value for the pre-game countdown.
     */
    public int countdownStart = 3;
    /**
     * @brief Ambient audio clip played before the countdown.
     */
    public AudioClip preGameAmbient;
    /**
     * @brief Audio clip played during the full countdown.
     */
    public AudioClip fullCountdownClip;

    [Header("Tile Drop Settings")]
    /**
     * @brief Canvas used for the tile-drop notification.
     */
    public Canvas tileDropCanvas;
    /**
     * @brief Text component displaying the time until tiles drop.
     */
    public TMP_Text tileDropText;
    /**
     * @brief Delay (in seconds) before tiles begin dropping.
     */
    public float tileDropDelay = 5f;
    /**
     * @brief Tracks whether tile dropping is enabled.
     */
    private bool tilesDroppingEnabled = false;
    /**
     * @brief Indicates whether tiles dropping is enabled.
     */
    public bool TilesDroppingEnabled => tilesDroppingEnabled;
    /**
     * @brief Name of the scene to load after the round finishes.
     */
    public string nextSceneName;
    /**
     * @brief Main background music audio clip.
     */
    public AudioClip mainMusic;

    [Header("Finish Settings")]
    /**
     * @brief Canvas shown at the finish.
     */
    public Canvas finishCanvas;
    /**
     * @brief Text component shown at game end.
     */
    public TMP_Text finishText;
    /**
     * @brief Duration (in seconds) to display the finish screen.
     */
    public float finishDisplayTime = 3f;
    /**
     * @brief Audio clip played when the game finishes.
     */
    public AudioClip finishClip;

    /**
     * @brief Internal structure for storing player input devices and assigned colors.
     */
    class PlayerEntry
    {
        public InputDevice Device;
        public Color Color;
    }

    private readonly Dictionary<PlayerInput, PlayerEntry> players = new Dictionary<PlayerInput, PlayerEntry>();
    private readonly List<PlayerInput> eliminationOrder = new List<PlayerInput>();

    private SwitchScene sceneSwitcher;
    private AudioManager globalAudioManager;
    private AudioSource audioSource;

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
     * @brief Unity event called on the first frame; stops music, hides UI, configures audio source, and signals server.
     */
    void Start()
    {
        globalAudioManager.StopMusic();

        tileDropCanvas?.gameObject.SetActive(false);
        countdownCanvas?.gameObject.SetActive(false);
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
        ServerManager.SendtoAllSockets("spleef");
    }

    /**
     * @brief Registers a new player: applies materials, deactivates input, and adds to the game.
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
     * @brief Adds a PlayerInput to the manager with its device and color, then shows player labels.
     * @param pi The PlayerInput instance.
     * @param dev The InputDevice associated with the player.
     * @param color The color assigned to the player.
     */
    private void AddPlayer(PlayerInput pi, InputDevice dev, Color color)
    {
        Instance.players[pi] = new PlayerEntry { Device = dev, Color = color };

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
            img.color = entry.Color;

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
     * @brief Coroutine handling the countdown before gameplay begins, with audio fades and time scale control.
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

        StartCoroutine(EnableTileDropsAfterDelay());
    }

    /**
     * @brief Coroutine that waits for the tile drop delay, updates the UI timer, and enables tile dropping.
     * @return IEnumerator for coroutine control.
     */
    IEnumerator EnableTileDropsAfterDelay()
    {
        tileDropCanvas?.gameObject.SetActive(true);

        float elapsed = 0f;
        while (elapsed < tileDropDelay)
        {
            if (elapsed > 2f)
                tileDropText.color = Color.red;

            elapsed += Time.deltaTime;
            UpdateMatchTimerText(tileDropDelay - elapsed);
            yield return null;
        }

        tileDropText.text = "Tiles Dropping!";
        yield return new WaitForSecondsRealtime(0.5f);
        tileDropCanvas?.gameObject.SetActive(false);
        tilesDroppingEnabled = true;
    }

    /**
     * @brief Updates the tile-drop countdown text to show remaining minutes and seconds.
     * @param remaining Time remaining before tile drop.
     */
    void UpdateMatchTimerText(float remaining)
    {
        if (tileDropText == null) return;
        int seconds = Mathf.Max(0, Mathf.CeilToInt(remaining));
        int mins = seconds / 60;
        int secs = seconds % 60;
        tileDropText.text = $"{mins}:{secs:00} Before Tiles Drop";
    }

    /**
     * @brief Handles logic when a player is eliminated, disables their input and checks for round end.
     * @param pi The PlayerInput instance of the eliminated player.
     */
    public void OnPlayerEliminated(PlayerInput pi)
    {
        if (!eliminationOrder.Contains(pi))
        {
            eliminationOrder.Add(pi);
            pi.DeactivateInput();
            pi.gameObject.SetActive(false);

            int aliveCount = players.Count - eliminationOrder.Count;
            if (aliveCount <= 1)
            {
                tilesDroppingEnabled = false;
                StartCoroutine(OnPlayerEliminatedRoutine());
            }

        }
    }

    /**
     * @brief Coroutine executed when only one player remains; shows finish UI, plays audio, finalizes ranking, and loads next scene.
     * @return IEnumerator for coroutine control.
     */
    private IEnumerator OnPlayerEliminatedRoutine()
    {
        foreach (var pi in players.Keys)
            pi.DeactivateInput();

        if (finishClip != null)
            audioSource.PlayOneShot(finishClip);

        finishCanvas?.gameObject.SetActive(true);
        finishText.text = "Finish";

        FinalizeRanking();

        if (mainMusic != null)
            StartCoroutine(FadeOut(2f));

        yield return new WaitForSecondsRealtime(finishDisplayTime);
        globalAudioManager.PlayMusic();
        sceneSwitcher.LoadNewScene(nextSceneName);
    }

    /**
     * @brief Orders players by elimination and updates the PlayerManager’s ranking.
     */
    public void FinalizeRanking()
    {
        eliminationOrder.Reverse();
        var pm = Object.FindFirstObjectByType<PlayerManager>();
        pm.tempRankClear();

        var sortedDevices = players
            .OrderBy(kvp => eliminationOrder.IndexOf(kvp.Key))
            .Select(kvp => kvp.Value.Device);

        foreach (var device in sortedDevices)
            pm.tempRankAdd(device);
    }
    
    /**
     * @brief Retrieves the assigned color for the given player, defaulting to white if not found.
     * @param pi The PlayerInput instance.
     * @return The player's assigned color.
     */
    public Color GetPlayerColor(PlayerInput pi) 
    {
        if (players.TryGetValue(pi, out var entry))
            return entry.Color;
        Debug.LogWarning($"No entry found for {pi}; defaulting to white.");
        return Color.white;
    }

    /**
     * @brief Coroutine that fades out the audio source over a specified duration.
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
