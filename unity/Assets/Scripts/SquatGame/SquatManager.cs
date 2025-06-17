using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class SquatManager : MonoBehaviour
{
    public static bool inputEnabled = false;

    public List<GameObject> playerList = new List<GameObject>();
    public List<GameObject> rankingList = new List<GameObject>();
    [SerializeField] private SwitchScene switchScene;


    [SerializeField] private float floatStartDelay = 2f;
    [SerializeField] private MinigameHUDController hudController;
    [SerializeField] private GameObject nameboardPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnParent;

    private bool gameEnded = false;
    private GameObject highestPlayer = null;

    void Start()
    {
        inputEnabled = false;

        if (hudController == null)
        {
            Debug.LogError("hudController is not assigned in the inspector!", this);
            return;
        }

        SpawnPlayers();

        hudController.OnCountdownFinished += HandleCountdownFinished;
        hudController.OnGameTimerFinished += HandleGameTimerFinished;

        hudController.ShowCountdown();
    }

    private void HandleCountdownFinished()
    {
        inputEnabled = true;
        StartNewRound();
        hudController.StartGameTimer();
    }

    private void HandleGameTimerFinished()
    {
        inputEnabled = false;
        EndGame();
    }

    void StartNewRound()
    {
        gameEnded = false;

        foreach (GameObject player in playerList)
        {
            PlayerMash mash = player.GetComponent<PlayerMash>();
            if (mash != null)
            {
                mash.StartNewRound();
            }
        }
    }

    void EndGame()
    {
        gameEnded = true;

        List<(GameObject player, int mashCount)> rankings = new List<(GameObject, int)>();

        foreach (GameObject player in playerList)
        {
            PlayerMash mash = player.GetComponent<PlayerMash>();
            if (mash != null)
            {
                rankings.Add((player, mash.GetMashCounter()));
            }
        }

        rankings.Sort((a, b) => b.mashCount.CompareTo(a.mashCount));
        rankingList.Clear();

        foreach (var entry in rankings)
        {
            rankingList.Add(entry.player);
        }

        if (rankingList.Count > 0)
            highestPlayer = rankingList[0];

        StartCoroutine(DelayedFloatAnimation(floatStartDelay));

        for (int i = 0; i < rankingList.Count; i++)
        {
            GameObject player = rankingList[i];
            PlayerInput input = player.GetComponent<PlayerInput>();

            if (input != null && PlayerManager.playerStats.ContainsKey(input.devices[0]))
            {
                var stats = PlayerManager.playerStats[input.devices[0]];
                stats.position = i + 1;
                stats.winner = (i == 0);
                Debug.Log($"Updated PlayerManager: {stats.name} - Position {stats.position} - Winner: {stats.winner}");
            }
        }

    }

    public void UpdateHighestPlayer()
    {
        GameObject topPlayer = null;
        int topCount = -1;

        foreach (GameObject player in playerList)
        {
            PlayerMash mash = player.GetComponent<PlayerMash>();
            if (mash != null)
            {
                int count = mash.GetMashCounter();
                if (count > topCount)
                {
                    topCount = count;
                    topPlayer = player;
                }
            }
        }

        if (topPlayer != null)
        {
            CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
            cam.SetTarget(topPlayer.transform);
        }
    }

    private IEnumerator DelayedFloatAnimation(float delay)
    {
        Debug.Log("⚠ Coroutine started");

        yield return new WaitForSeconds(delay);

        if (highestPlayer != null)
        {
            CameraFollow cam = Camera.main.GetComponent<CameraFollow>();
            if (cam != null)
            {
                cam.SetTarget(highestPlayer.transform);
                Debug.Log($"Camera now following {highestPlayer.name}");
            }
            else
            {
                Debug.LogWarning("No CameraFollow component found");
            }
        }

        foreach (GameObject player in playerList)
        {
            PlayerMash mash = player.GetComponent<PlayerMash>();
            if (mash != null)
            {
                float floatHeight = mash.GetMashCounter() * 1f;
                mash.TriggerFloatAnimation(floatHeight);
            }
        }

        yield return new WaitUntil(() =>
            playerList.All(p => p.GetComponent<PlayerMash>()?.IsFloatDone == true)
        );

        Debug.Log("✅ All players finished floating");

        yield return new WaitForSeconds(1.5f);

        if (switchScene != null)
        {
            switchScene.LoadNewScene("WinScreen");
        }
        else
        {
            Debug.LogWarning("switchScene reference is null");
        }
    }

    void SpawnPlayers()
    {
        if (ServerManager.allControllers == null)
        {
            Debug.LogWarning("No controllers found.");
            return;
        }

        var devices = ServerManager.allControllers.Values.ToArray();
        Transform[] spawnPoints = GetSpawnPoints();

        PlayerInputManager.instance.playerPrefab = playerPrefab;

        for (int i = 0; i < devices.Length; i++)
        {
            InputDevice device = devices[i];
            GameObject player = SpawnPlayerForDevice(device, i, spawnPoints);
            if (player != null)
            {
                SetupPlayerAppearance(player, device);
                playerList.Add(player);
            }
        }
    }

    Transform[] GetSpawnPoints()
    {
        return spawnParent.GetComponentsInChildren<Transform>()
                        .Where(t => t != spawnParent)
                        .ToArray();
    }

    GameObject SpawnPlayerForDevice(InputDevice device, int index, Transform[] spawnPoints)
    {
        Debug.Log("Spawning player for device: " + device.displayName);

        PlayerInput playerInput = PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);
        if (playerInput == null)
        {
            Debug.LogError("Failed to join player.");
            return null;
        }

        GameObject player = playerInput.gameObject;

        if (index < spawnPoints.Length)
        {
            player.transform.position = spawnPoints[index].position;
        }
        else
        {
            Debug.LogWarning($"Not enough spawn points for player {index + 1}");
        }

        return player;
    }

    void SetupPlayerAppearance(GameObject player, InputDevice device)
    {
        Transform bodyTransform = player.transform.Find("Body");
        if (bodyTransform != null)
        {
            Renderer renderer = bodyTransform.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = PlayerManager.findColor(device);
                renderer.material = mat;
            }
            else
            {
                Debug.LogWarning("Renderer not found on 'Body' object.");
            }
        }
        else
        {
            Debug.LogWarning("Could not find 'Body' transform on player.");
        }

        if (nameboardPrefab != null)
        {
            GameObject nameboard = Instantiate(nameboardPrefab, player.transform);

            nameboard.transform.localPosition = new Vector3(0, 2.0f, 0);

            TextMeshProUGUI text = nameboard.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                text.text = PlayerManager.playerStats[device].name;
            }
        }
    }
}
