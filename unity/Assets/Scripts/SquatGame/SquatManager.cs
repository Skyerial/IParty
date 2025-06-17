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

    [SerializeField] private float floatStartDelay = 2f;
    [SerializeField] private MinigameHUDController hudController;
    [SerializeField] private GameObject nameboardPrefab;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform spawnParent; // Parent object containing spawn points

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

        // ✅ SPAWN PLAYERS
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
        yield return new WaitForSeconds(delay);

        foreach (GameObject player in playerList)
        {
            PlayerMash mash = player.GetComponent<PlayerMash>();
            if (mash != null)
            {
                float floatHeight = mash.GetMashCounter() * 1f;
                mash.TriggerFloatAnimation(floatHeight);
            }
        }

        if (highestPlayer != null)
        {
            Camera.main.GetComponent<CameraFollow>()?.SetTarget(highestPlayer.transform);
        }

        for (int i = 0; i < rankingList.Count; i++)
        {
            Debug.Log($"{i + 1} place: {rankingList[i].name}");
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
