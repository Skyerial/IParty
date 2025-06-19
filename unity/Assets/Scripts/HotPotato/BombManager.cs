using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEditorInternal;

public class BombManager : MonoBehaviour
{
    public MinigameHUDController hudController; 
    public GameObject bombPrefab;
    [SerializeField] private GameObject nameboardPrefab;
    public SwitchScene switchScene;
    public GameObject prefab;
    private GameObject currentBomb;
    public List<GameObject> players = new List<GameObject>();

    [System.Obsolete]
    void Start()
    {
        AudioManager.Instance.PlayRandomMiniGameTrack();

        JoinAllPlayers();
        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.GetComponent<PlayerInput>() != null && players.Count < 4)
            {
                players.Add(obj);
            }
        }

        SendPlayerStatsToAllClients();

        if (hudController != null)
        {
            hudController.OnCountdownFinished += StartHotPotato;
            hudController.ShowCountdown();
        }
    }


    void SendPlayerStatsToAllClients()
    {
        var availableButtons = new[] { "B", "D", "A", "C" }; // Gamepad: East, North, South, West

        for (int i = 0; i < players.Count; i++)
        {
            var sender = players[i]; // current player

            // Prepare opponent list
            var opponents = players
                .Where(p => p != sender)
                .ToList();

            var perClientStats = new List<PlayerConfig>();

            for (int j = 0; j < opponents.Count; j++)
            {
                var opp = opponents[j];
                var input = opp.GetComponent<PlayerInput>();
                if (input == null || input.devices.Count == 0) continue;

                InputDevice device = input.devices[0];
                if (!PlayerManager.playerStats.TryGetValue(device, out var stats)) continue;

                perClientStats.Add(new PlayerConfig
                {
                    name = stats.name,
                    color = stats.color,
                    button = availableButtons[j]
                });
            }

            var message = new HotPotatoMessage
            {
                type = "controller",
                controller = "hotpotato",
                playerstats = perClientStats
            };

            string json = JsonUtility.ToJson(message);

            var senderDevice = sender.GetComponent<PlayerInput>()?.devices.FirstOrDefault();
            if (senderDevice != null &&
                PlayerManager.playerStats.TryGetValue(senderDevice, out var senderStats))
            {
                var vc = PlayerManager.playerStats.FirstOrDefault(kv => kv.Value == senderStats && kv.Key is VirtualController).Key as VirtualController;
                if (vc != null)
                {
                    ServerManager.SendMessageToClient(vc.remoteId, json);
                    
                    var mover = sender.GetComponent<MovementHotpotato>();
                    if (mover != null)
                    {
                        mover.ConfigureThrowTargets(perClientStats);
                    }
                }
            }

        }
    }


    void JoinAllPlayers()
    {
        if (ServerManager.allControllers != null)
        {
            foreach (var device in ServerManager.allControllers.Values.ToArray())
            {
                Debug.Log("Spawning...");
                PlayerInputManager.instance.playerPrefab = prefab;
                PlayerInput playerInput = PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);
                if (playerInput != null)
                {
                    initPlayer(playerInput);
                    GameManager.RegisterPlayerGame(playerInput);
                    MovementHotpotato mover = playerInput.GetComponent<MovementHotpotato>();
                    mover.bombManager = this;
                }
                else
                {
                    Debug.LogError("PlayerInput == NULL");
                }
            }
        }
    }

    void initPlayer(PlayerInput playerInput)
    {
        InputDevice device = playerInput.devices[0];
        Transform body = playerInput.transform.Find("Body");
        var renderer = body.GetComponent<SkinnedMeshRenderer>();
        renderer.material = PlayerManager.findColor(device);

        if (nameboardPrefab == null) return;

        GameObject nameboard = Instantiate(nameboardPrefab, playerInput.transform);
        nameboard.transform.localPosition = new Vector3(0, 2f, 0);

        var text = nameboard.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text != null && PlayerManager.playerStats.TryGetValue(device, out var stats))
        {
            text.text = stats.name;
        }
    }

    void StartHotPotato()
    {
        hudController.StartGameTimer();
        SpawnBombOnRandomPlayer();
    }

    void Update()
    {
        players.RemoveAll(p => p == null);

        if (players.Count == 1)
        {
            Debug.Log("Gameover: winner " + players[0].name);
            PlayerManager.instance.tempRankAdd(players[0].GetComponent<PlayerInput>().devices[0]);
            if (hudController != null)
            {
                hudController.ShowFinishText();
                StartCoroutine(DelayedSceneSwitch());
            }
            return;
        }

        if (currentBomb == null && players.Count > 1)
        {
            SpawnBombOnRandomPlayer();
        }

    }
    private System.Collections.IEnumerator DelayedSceneSwitch()
    {
        yield return new WaitForSeconds(3f);
        switchScene.LoadNewScene("Winscreen");
    }


    void SpawnBombOnRandomPlayer()
    {
        int index = Random.Range(0, players.Count);
        GameObject selectedPlayer = players[index];

        currentBomb = Instantiate(bombPrefab);
        currentBomb.transform.SetParent(selectedPlayer.transform);
        currentBomb.transform.localPosition = new Vector3(0, 2f, 0f);
        currentBomb.transform.localRotation = Quaternion.identity;
    }

    public GameObject GetCurrentBomb()
    {
        return currentBomb;
    }


    [System.Serializable]
    public class PlayerConfig
    {
        public string name;
        public string color;
        public string button;
    }

    [System.Serializable]
    public class HotPotatoMessage
    {
        public string type;
        public string controller;
        public List<PlayerConfig> playerstats;
    }
}
