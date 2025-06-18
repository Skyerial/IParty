using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SquatManager : MonoBehaviour
{
    public static bool inputEnabled = false;

    [SerializeField] private SwitchScene switchScene;
    [SerializeField] private float floatStartDelay = 2f;
    [SerializeField] private MinigameHUDController hudController;
    [SerializeField] private GameObject nameboardPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnParent;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip floatStartSFX;

    private List<GameObject> playerList = new();
    private List<GameObject> rankingList = new();
    private GameObject highestPlayer;

    void Start()
    {
        inputEnabled = false;

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

    void StartRound()
    {
        inputEnabled = true;
        ResetPlayers();
        hudController.StartGameTimer();
    }

    void EndRound()
    {
        inputEnabled = false;
        RankPlayers();
        UpdatePlayerStats();

        PlayerManager.instance.tempRankClear();
        foreach (var player in rankingList)
        {
            var input = player.GetComponent<PlayerInput>();
            if (input != null)
                PlayerManager.instance.tempRankAdd(input.devices[0]);
        }

        StartCoroutine(FloatAndLoadWin());
    }


    void ResetPlayers()
    {
        foreach (var player in playerList)
        {
            player.GetComponent<PlayerMash>()?.StartNewRound();
        }
    }

    void RankPlayers()
    {
        rankingList = playerList
            .OrderByDescending(p => p.GetComponent<PlayerMash>()?.GetMashCounter() ?? 0)
            .ToList();

        highestPlayer = rankingList.FirstOrDefault();
    }

    void UpdatePlayerStats()
    {
        for (int i = 0; i < rankingList.Count; i++)
        {
            var input = rankingList[i].GetComponent<PlayerInput>();
            if (input != null && PlayerManager.playerStats.TryGetValue(input.devices[0], out var stats))
            {
                stats.position = i + 1;
                stats.winner = (i == 0);
            }
        }
    }

    IEnumerator FloatAndLoadWin()
    {
        yield return new WaitForSeconds(floatStartDelay);

        if (audioSource != null && floatStartSFX != null)
        {
            audioSource.PlayOneShot(floatStartSFX);
        }

        AssignCamera();
        StartFloatAnimations();

        yield return new WaitUntil(() => playerList.All(p => p.GetComponent<PlayerMash>()?.IsFloatDone == true));
        yield return new WaitForSeconds(1f);

        switchScene?.LoadNewScene("WinScreen");
    }

    void AssignCamera()
    {
        if (highestPlayer == null) return;
        Camera.main?.GetComponent<CameraFollow>()?.SetTarget(highestPlayer.transform);
    }

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


    void SpawnPlayers()
    {
        var devices = ServerManager.allControllers?.Values.ToArray();
        if (devices == null) return;

        var spawnPoints = spawnParent.GetComponentsInChildren<Transform>()
            .Where(t => t != spawnParent)
            .ToArray();

        PlayerInputManager.instance.playerPrefab = playerPrefab;

        for (int i = 0; i < devices.Length; i++)
        {
            var device = devices[i];
            var input = PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);
            if (input == null) continue;

            var player = input.gameObject;
            if (i < spawnPoints.Length)
                player.transform.position = spawnPoints[i].position;

            SetPlayerVisuals(player, device);
            playerList.Add(player);
        }
    }

    void SetPlayerVisuals(GameObject player, InputDevice device)
    {
        var body = player.transform.Find("Body")?.GetComponent<Renderer>();
        if (body != null)
            body.material = PlayerManager.findColor(device);

        if (nameboardPrefab == null) return;

        var nameboard = Instantiate(nameboardPrefab, player.transform);
        nameboard.transform.localPosition = new Vector3(0, 2f, 0);

        var text = nameboard.GetComponentInChildren<TextMeshProUGUI>();
        if (text != null)
            text.text = PlayerManager.playerStats[device].name;
    }

    void PrintPlayerScores()
    {
        Debug.Log("=== Player Scores ===");

        foreach (var player in rankingList)
        {
            var mash = player.GetComponent<PlayerMash>();
            var input = player.GetComponent<PlayerInput>();

            if (mash != null && input != null && PlayerManager.playerStats.TryGetValue(input.devices[0], out var stats))
            {
                Debug.Log($"{stats.name}: {mash.GetMashCounter()} presses");
            }
        }

        Debug.Log("=====================");
    }
}