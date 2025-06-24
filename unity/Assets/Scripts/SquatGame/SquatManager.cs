using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/**
 * @brief Manages game flow for squat minigame: countdown, input, scoring, float animation, and scene transition.
 */
public class SquatManager : MonoBehaviour
{
    /**
     * @brief Controls whether mash input is allowed.
     */
    public static bool inputEnabled = false;

    [SerializeField]
    private ChangeScene changeScene;

    [SerializeField]
    private SwitchScene switchScene;

    [SerializeField]
    private float floatStartDelay = 2f;

    [SerializeField]
    private MinigameHUDController hudController;

    [SerializeField]
    private GameObject nameboardPrefab;

    [SerializeField]
    private GameObject playerPrefab;

    [SerializeField]
    private Transform spawnParent;

    [SerializeField]
    private AudioSource audioSource;

    [SerializeField]
    private AudioClip floatStartSFX;

    private List<GameObject> playerList = new();
    private List<GameObject> rankingList = new();
    private GameObject highestPlayer;

    /**
     * @brief Unity callback. Initializes game, shows countdown, and spawns players.
     */
    void Start()
    {
        inputEnabled = false;

        ServerManager.SendtoAllSockets("one-button");

        if (hudController == null)
        {
            Debug.LogError("hudController not assigned");
            return;
        }

        SpawnPlayers();
        hudController.OnCountdownFinished += StartRound;
        hudController.OnGameTimerFinished += EndRound;
        hudController.ShowCountdown();
    }

    /**
     * @brief Called when countdown ends. Enables input and starts timer.
     */
    void StartRound()
    {
        inputEnabled = true;
        ResetPlayers();
        hudController.StartGameTimer();
    }

    /**
     * @brief Called when game timer ends. Disables input, ranks players, updates stats, starts float animation.
     */
    void EndRound()
    {
        inputEnabled = false;
        RankPlayers();
        UpdatePlayerStats();

        PlayerManager.instance.tempRankClear();
        for (int i = rankingList.Count - 1; i >= 0; i--)
        {
            var input = rankingList[i].GetComponent<PlayerInput>();
            if (input != null)
                PlayerManager.instance.tempRankAdd(input.devices[0]);
        }

        StartCoroutine(FloatAndLoadWin());
    }

    /**
     * @brief Resets mash state for all players.
     */
    void ResetPlayers()
    {
        foreach (var player in playerList)
        {
            player.GetComponent<PlayerMash>()?.StartNewRound();
        }
    }

    /**
     * @brief Sorts players by mash count, highest first.
     */
    void RankPlayers()
    {
        rankingList = playerList
            .OrderByDescending(p => p.GetComponent<PlayerMash>()?.GetMashCounter() ?? 0)
            .ToList();

        highestPlayer = rankingList.FirstOrDefault();
    }

    /**
     * @brief Updates playerStats with final ranking positions.
     */
    void UpdatePlayerStats()
    {
        for (int i = 0; i < rankingList.Count; i++)
        {
            var input = rankingList[i].GetComponent<PlayerInput>();
            if (
                input != null
                && PlayerManager.playerStats.TryGetValue(input.devices[0], out var stats)
            )
            {
                stats.position = i + 1;
            }
        }
    }

    /**
     * @brief Plays float animation, waits for completion, then loads win screen.
     */
    IEnumerator FloatAndLoadWin()
    {
        yield return new WaitForSeconds(floatStartDelay);

        if (audioSource != null && floatStartSFX != null)
        {
            audioSource.PlayOneShot(floatStartSFX);
        }

        AssignCamera();
        StartFloatAnimations();

        yield return new WaitUntil(() =>
            playerList.All(p => p.GetComponent<PlayerMash>()?.IsFloatDone == true)
        );
        yield return new WaitForSeconds(1f);

        switchScene?.LoadNewScene("WinScreen");
    }

    /**
     * @brief Makes the camera follow the top-ranked player.
     */
    void AssignCamera()
    {
        if (highestPlayer == null)
            return;
        Camera.main?.GetComponent<CameraFollow>()?.SetTarget(highestPlayer.transform);
    }

    /**
     * @brief Triggers float animation for all players based on mash count.
     */
    void StartFloatAnimations()
    {
        float maxMash = playerList.Max(p => p.GetComponent<PlayerMash>()?.GetMashCounter() ?? 1);

        foreach (var player in playerList)
        {
            var mash = player.GetComponent<PlayerMash>();
            if (mash != null)
            {
                float mashCount = mash.GetMashCounter();
                float floatMultiplier = 1f;
                float scaledHeight = mashCount * floatMultiplier;

                mash.TriggerFloatAnimation(scaledHeight);
            }
        }
    }

    /**
     * @brief Spawns players and assigns their visuals.
     */
    void SpawnPlayers()
    {
        var devices = ServerManager.allControllers?.Values.ToArray();
        if (devices == null)
            return;

        var spawnPoints = spawnParent
            .GetComponentsInChildren<Transform>()
            .Where(t => t != spawnParent)
            .ToArray();

        PlayerInputManager.instance.playerPrefab = playerPrefab;

        for (int i = 0; i < devices.Length; i++)
        {
            var device = devices[i];
            var input = PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);
            if (input == null)
                continue;

            var player = input.gameObject;

            if (i < spawnPoints.Length)
            {
                var spawnPoint = spawnPoints[i];

                player.transform.position = spawnPoint.position;

                Vector3 targetPos = Camera.main.transform.position;
                Vector3 lookDirection = (targetPos - player.transform.position).normalized;
                lookDirection.y = 0f;

                if (lookDirection != Vector3.zero)
                    player.transform.rotation = Quaternion.LookRotation(lookDirection);
            }

            SetPlayerVisuals(player, device);
            playerList.Add(player);
        }
    }

    /**
     * @brief Sets body color, face texture, and nameboard for a player.
     */
    void SetPlayerVisuals(GameObject player, InputDevice device)
    {
        SetPlayerBodyColor(player, device);
        SetPlayerFaceTexture(player, device);
        SetPlayerNameboard(player, device);
    }

    /**
     * @brief Sets player body color based on device.
     */
    void SetPlayerBodyColor(GameObject player, InputDevice device)
    {
        var body = player.transform.Find("Body")?.GetComponent<Renderer>();
        if (body != null)
            body.material = PlayerManager.findColor(device);
    }

    /**
     * @brief Sets player face texture based on device.
     */
    void SetPlayerFaceTexture(GameObject player, InputDevice device)
    {
        var faceTransform = player.transform.Find("Face");
        if (faceTransform == null)
            return;

        var faceRenderer = faceTransform.GetComponent<Renderer>();
        if (faceRenderer == null)
            return;

        var newMat = new Material(faceRenderer.material);
        Texture2D faceTexture = PlayerManager.findFace(device);

        if (faceTexture != null && faceTexture.width > 2)
        {
            newMat.mainTexture = faceTexture;
        }

        faceRenderer.material = newMat;
    }

    /**
     * @brief Adds nameboard above player and sets player name.
     */
    void SetPlayerNameboard(GameObject player, InputDevice device)
    {
        if (nameboardPrefab == null)
            return;

        var nameboard = Instantiate(nameboardPrefab, player.transform);
        nameboard.transform.localPosition = new Vector3(0, 2f, 0);
        nameboard.transform.localRotation = Quaternion.identity;

        var text = nameboard.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = PlayerManager.playerStats[device].name;
    }
}
