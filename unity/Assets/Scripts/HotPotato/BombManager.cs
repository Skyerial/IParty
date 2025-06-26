using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;

/**
 * @brief Manages the Hot Potato game logic, including player setup, bomb spawning, HUD events, and game end.
 */
public class BombManager : MonoBehaviour
{
    public MinigameHUDController hudController; 
    public GameObject bombPrefab;
    [SerializeField] private GameObject nameboardPrefab;
    public SwitchScene switchScene;
    public GameObject prefab;
    private GameObject currentBomb;
    public List<GameObject> players = new List<GameObject>();

    /**
     * @brief Initializes the game, joins players, sets up HUD and starts countdown.
     * @return void
     */
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

    /**
     * @brief Sends each player's opponent stats to their client and configures throw targets.
     * @return void
     */
    void SendPlayerStatsToAllClients()
    {
        var availableButtons = new[] { "B", "D", "A", "C" }; 

        for (int i = 0; i < players.Count; i++)
        {
            var sender = players[i];

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

    /**
     * @brief Joins all connected players and initializes their setup.
     * @return void
     */
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

    /**
     * @brief Initializes visual appearance and nameplate for a single player.
     * @param playerInput The PlayerInput component of the player to initialize.
     * @return void
     */
    void initPlayer(PlayerInput playerInput)
    {
        InputDevice device = playerInput.devices[0];
        Transform body = playerInput.transform.Find("Body");
        var renderer = body.GetComponent<SkinnedMeshRenderer>();
        renderer.material = PlayerManager.findColor(device);

        var faceTransform = playerInput.transform.Find("Face");
        if (faceTransform != null)
        {
            var faceRenderer = faceTransform.GetComponent<Renderer>();
            if (faceRenderer != null)
            {
                var newMat = new Material(faceRenderer.material);
                Texture2D faceTexture = PlayerManager.findFace(device);

                if (faceTexture != null && faceTexture.width > 2)
                {
                    newMat.mainTexture = faceTexture;
                }

                faceRenderer.material = newMat; 
            }
        }
        
        if (nameboardPrefab == null) return;

        GameObject nameboard = Instantiate(nameboardPrefab);
        nameboard.transform.position = playerInput.transform.position + new Vector3(0, 2f, 0);
        nameboard.transform.localRotation = Quaternion.identity;

        var text = nameboard.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text != null && PlayerManager.playerStats.TryGetValue(device, out var stats))
        {
            text.text = stats.name;
        }
    }

    /**
     * @brief Called when countdown finishes. Starts game timer and spawns initial bomb.
     * @return void
     */
    void StartHotPotato()
    {
        hudController.StartGameTimer();
        SpawnBombOnRandomPlayer();
    }

    /**
     * @brief Updates the game state. Handles player elimination and bomb respawn logic.
     * @return void
     */
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

    /**
     * @brief Waits for a short delay, then loads the win screen scene.
     * @return IEnumerator
     */
    private System.Collections.IEnumerator DelayedSceneSwitch()
    {
        yield return new WaitForSeconds(3f);
        switchScene.LoadNewScene("Winscreen");
    }

    /**
     * @brief Spawns the bomb and attaches it to a random player.
     * @return void
     */
    void SpawnBombOnRandomPlayer()
    {
        int index = Random.Range(0, players.Count);
        GameObject selectedPlayer = players[index];

        currentBomb = Instantiate(bombPrefab);
        currentBomb.transform.SetParent(selectedPlayer.transform);
        // var mover = selectedPlayer.GetComponent<MovementHotpotato>();
        // if (mover != null)
        // {
        //     mover.SetHoldingBombState(true); 
        // }
        currentBomb.transform.localPosition = new Vector3(0, 2f, 0f);
        currentBomb.transform.localRotation = Quaternion.identity;
    }

    /**
     * @brief Returns the currently active bomb object.
     * @return GameObject The current bomb.
     */
    public GameObject GetCurrentBomb()
    {
        return currentBomb;
    }

    /**
     * @brief Holds player display data for clients (name, color, assigned button).
     */
    [System.Serializable]
    public class PlayerConfig
    {
        public string name;
        public string color;
        public string button;
    }

    /**
     * @brief Message structure sent to clients containing controller type and opponent stats.
     */
    [System.Serializable]
    public class HotPotatoMessage
    {
        public string type;
        public string controller;
        public List<PlayerConfig> playerstats;
    }
}
