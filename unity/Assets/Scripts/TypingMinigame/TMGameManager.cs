using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using Unity.VisualScripting;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using UnityEditor.Build.Player;
using System.Linq;
using UnityEditor.SearchService;

public class TMGameManager : MonoBehaviour
{
    public static TMGameManager Instance { get; private set; }

    public int wordsPerPlayer = 10;

    public List<PlayerTypingController> players; // list of row1, ro2, etc

    public List<GameObject> spawns; // spawn1, spawn2

    private bool gameStarted = false;

    public int finishCount = 0;

    public Dictionary<string, PlayerTypingController> playerControllers;

    public Dictionary<PlayerTypingController, VirtualController> playerVirtualControllers;

    public List<PlayerInput> playerInputs;
    public GameObject spawn;
    public string WinScreen;
    private SwitchScene sceneSwitcher;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        sceneSwitcher = GetComponent<SwitchScene>();
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        gameStarted = true;
        Debug.Log("Game started!");

        // playerControllers = new Dictionary<string, PlayerTypingController>();
        // playerVirtualControllers = new Dictionary<PlayerTypingController, VirtualController>();
        // playerInputs = new List<PlayerInput>();

        TM_MusicController.Instance.FadeInBGM(2f); // fade in over 2 seconds

        // AttachMobilePlayer();
    }

    public void RegisterPlayer(PlayerInput pi, VirtualController controller)
    {
        if (playerControllers == null)
            playerControllers = new Dictionary<string, PlayerTypingController>();
        if (playerVirtualControllers == null)
            playerVirtualControllers = new Dictionary<PlayerTypingController, VirtualController>();
        if (playerInputs == null)
            playerInputs = new List<PlayerInput>();

        playerInputs.Add(pi);
        if (controller == null)
        {
            Debug.LogWarning("Controller is not a VirtualController. Skipping.");
            return;
        }
        int playerIndex = playerInputs.FindIndex(p => p == pi);
        Debug.Log($"playerIndex: {playerIndex}");
        PlayerTypingController typingController = players[playerIndex];
        typingController.playerInputIndex = playerIndex;

        // place the player on its spawn
        pi.transform.position = spawn.transform.GetChild(playerIndex).transform.position;

        playerControllers[controller.remoteId] = typingController;
        playerVirtualControllers[typingController] = controller;

        ColorPlayer(pi);
        AddFacePlayer(pi);

        typingController.raceController = pi.GetComponent<PlayerRaceController>();
        typingController.textSpawner.words = wordsPerPlayer;
        typingController.textSpawner.SpawnWords();
        typingController.raceController.InitializeRace(wordsPerPlayer);
        typingController.Initialize();
        typingController.inputField.interactable = true;
        typingController.inputField.text = "";

        TextMeshProUGUI name = players[playerIndex].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        name.text = PlayerManager.playerStats.Values.FirstOrDefault(p => p.playerID == playerIndex).name;
    }

    private void ColorPlayer(PlayerInput pi)
    {
        InputDevice dev = pi.devices[0];
        Material mat = PlayerManager.findColor(dev);

        SkinnedMeshRenderer body = pi.transform.Find("Body").GetComponent<SkinnedMeshRenderer>();
        body.material = mat;
    }

    private void AddFacePlayer(PlayerInput pi)
    {
        Transform face = pi.transform.Find("Face");
        SkinnedMeshRenderer renderer_face = face.GetComponent<SkinnedMeshRenderer>();
        Texture2D faceTexture = new Texture2D(2, 2);
        faceTexture.LoadImage(PlayerManager.playerStats[pi.devices[0]].face);
        renderer_face.material = new Material(renderer_face.material);
        renderer_face.material.mainTexture = faceTexture;
    }

    // private void AttachMobilePlayer()
    // {
    //     int i = 0;

    //     foreach (var mobilePlayer in PlayerManager.playerStats)
    //     {
    //         if (i >= players.Count) break;

    //         var controller = mobilePlayer.Key as VirtualController;
    //         if (controller == null)
    //         {
    //             Debug.LogWarning("Controller is not a VirtualController. Skipping.");
    //             continue;
    //         }
    //         var typingController = players[i];

    //         playerControllers[controller.remoteId] = typingController;
    //         playerVirtualControllers[typingController] = controller;

    //         typingController.textSpawner.words = wordsPerPlayer;
    //         typingController.textSpawner.SpawnWords();
    //         typingController.raceController.InitializeRace(wordsPerPlayer);
    //         typingController.Initialize();
    //         typingController.inputField.interactable = true;
    //         typingController.inputField.text = "";

    //         TextMeshProUGUI name = players[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
    //         name.text = mobilePlayer.Value.name;

    //         i++;
    //     }

    //     while (i < players.Count)
    //     {
    //         players[i].gameObject.SetActive(false);
    //         i++;
    //     }
    // }

    public void HandleMobileInput(VirtualController player, string input)
    {
        if (playerControllers.TryGetValue(player.remoteId, out var controller))
        {
            controller.HandleInput(input);
        }
        else
        {
            Debug.LogWarning($"Input from unknown player: {player.remoteId}");
        }
    }

    public void SendClearCommandtoClient(PlayerTypingController player)
    {
        Debug.Log("recieved clear command");
        var controller = playerVirtualControllers[player];
        ServerManager.SendtoSocket(controller);
    }

    private string FinishPositionIntToString(int position)
    {
        switch (position)
        {
            case 1:
                return "1st";
            case 2:
                return "2nd";
            case 3:
                return "3rd";
            case 4:
                return "4th";
        }

        return "";
    }

    public void OnPlayerFinished(PlayerTypingController player)
    {
        finishCount++;
        player.finishPostion = finishCount;
        player.wordsLeftText.text = FinishPositionIntToString(finishCount);
        Debug.Log($"{player.name} finished in position {finishCount}");
        player.inputField.interactable = false;

        TM_MusicController.Instance.PlayFinishSFX();

        int activePlayerCount = playerControllers.Count;
        if (finishCount == activePlayerCount)
        {
            Debug.Log("Game over!");
            StartCoroutine(HandleEndGameSequence());
        }
    }

    private IEnumerator HandleEndGameSequence()
    {
        TM_MusicController.Instance.FadeOutBGM(1f);
        yield return new WaitForSeconds(1f); // Wait for fade out to finish

        TM_MusicController.Instance.PlayEndGameSFX();

        // TODO: Add win screen or restart logic
        PlayerManager pm = FindFirstObjectByType<PlayerManager>();
        List<PlayerTypingController> sortedControllers = playerControllers
            .Values
            .OrderBy(controller => controller.finishPostion)
            .ToList();

        foreach (var controller in sortedControllers)
        {
            InputDevice id = playerInputs[controller.playerInputIndex].devices[0];
            pm.tempRankAdd(id);
        }

        sceneSwitcher.LoadNewScene(WinScreen);
    }

}
