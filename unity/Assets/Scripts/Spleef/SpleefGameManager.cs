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

    private AudioSource audioSource;
    public AudioClip preGameAmbient;

    [Header("Pre-Game Countdown")]
    public Canvas countdownCanvas;
    public TMP_Text countdownText;
    public int countdownStart = 3;
    public AudioClip fullCountdownClip;

    [Header("Tile Drop Settings")]
    public Canvas tileDropCanvas;
    public TMP_Text tileDropText;
    public float tileDropDelay = 5f;
    private bool tilesDroppingEnabled = false;
    public bool TilesDroppingEnabled => tilesDroppingEnabled;
    public AudioClip mainMusic;

    [Header("Finish Settings")]
    public Canvas finishCanvas;
    public TMP_Text finishText;
    public float finishDisplayTime = 3f;
    public AudioClip finishClip;

    class PlayerEntry
    {
        public InputDevice Device;
        public Color Color;
    }

    private readonly Dictionary<PlayerInput, PlayerEntry> players = new Dictionary<PlayerInput, PlayerEntry>();
    private readonly List<PlayerInput> eliminationOrder = new List<PlayerInput>();

    private SwitchScene sceneSwitcher;
    public string nextSceneName;

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

    public static void RegisterPlayerGame(PlayerInput pi)
    {
        var dev = pi.devices[0];
        var mat = PlayerManager.findColor(dev);

        var body = pi.GetComponentsInChildren<SkinnedMeshRenderer>()
                     .First(r => r.name == "Body.008");
        body.material = mat;

        Instance.players[pi] = new PlayerEntry { Device = dev, Color = mat.color };

        pi.DeactivateInput();
    }

    public void StartGame()
    {
        StartCoroutine(PreGameCountdown());
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

        StartCoroutine(EnableTileDropsAfterDelay());
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

    void UpdateMatchTimerText(float remaining)
    {
        if (tileDropText == null) return;
        int seconds = Mathf.Max(0, Mathf.CeilToInt(remaining));
        int mins = seconds / 60;
        int secs = seconds % 60;
        tileDropText.text = $"{mins}:{secs:00} Before Tiles Drop";
    }

    public void OnPlayerEliminated(PlayerInput pi)
    {
        if (!eliminationOrder.Contains(pi))
        {
            eliminationOrder.Add(pi);
            pi.DeactivateInput();
            pi.gameObject.SetActive(false);

            int aliveCount = players.Count - eliminationOrder.Count;
            if (aliveCount <= 1)
                StartCoroutine(OnPlayerEliminatedRoutine());
        }
    }

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
        sceneSwitcher.LoadNewScene(nextSceneName);
    }

    public void FinalizeRanking()
    {
        var pm = Object.FindFirstObjectByType<PlayerManager>();
        pm.tempRankClear();

        var sortedDevices = players
            .OrderBy(kvp => eliminationOrder.IndexOf(kvp.Key))
            .Select(kvp => kvp.Value.Device);

        foreach (var device in sortedDevices)
            pm.tempRankAdd(device);
    }
    
    public Color GetPlayerColor(PlayerInput pi) 
    {
        if (players.TryGetValue(pi, out var entry))
            return entry.Color;
        Debug.LogWarning($"No entry found for {pi}; defaulting to white.");
        return Color.white;
    }
}
