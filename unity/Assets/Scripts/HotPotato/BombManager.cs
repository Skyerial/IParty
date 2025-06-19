using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEditorInternal;

public class BombManager : MonoBehaviour
{
    public MinigameHUDController hudController; // drag this in the Inspector
    public GameObject bombPrefab;
    public SwitchScene switchScene;
    public GameObject prefab;
    private GameObject currentBomb;
    public List<GameObject> players = new List<GameObject>();

    [System.Obsolete]
    void Start()
    {
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
        string[] buttonLetters = new[] { "A", "B", "C", "D" };

        // Build playerstats list in the same order as BombManager.players
        var orderedPlayerConfigs = new List<PlayerConfig>();

        for (int i = 0; i < players.Count && i < buttonLetters.Length; i++)
        {
            GameObject playerObj = players[i];
            var input = playerObj.GetComponent<PlayerInput>();
            if (input == null || input.devices.Count == 0)
                continue;

            InputDevice device = input.devices[0];
            if (!PlayerManager.playerStats.TryGetValue(device, out var stats))
                continue;

            orderedPlayerConfigs.Add(new PlayerConfig
            {
                name = stats.name,
                color = stats.color,
                button = buttonLetters[i]
            });
        }

        // Create message object
        var message = new HotPotatoMessage
        {
            type = "hotpotato",
            label = "playerstats",
            playerstats = orderedPlayerConfigs
        };

        string json = JsonUtility.ToJson(message);

        // Send to all clients
        foreach (var vc in PlayerManager.playerStats.Keys.OfType<VirtualController>())
        {
            ServerManager.SendMessageToClient(vc.remoteId, json);
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
                    colorPlayer(playerInput);
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

    void colorPlayer(PlayerInput playerInput)
    {
        Transform body = playerInput.transform.Find("Body");
        SkinnedMeshRenderer renderer = body.GetComponent<SkinnedMeshRenderer>();
        renderer.material = PlayerManager.findColor(playerInput.devices[0]);
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
        public string label;
        public List<PlayerConfig> playerstats;
    }
}
