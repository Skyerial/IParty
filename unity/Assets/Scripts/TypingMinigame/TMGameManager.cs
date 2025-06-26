using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.Video;
using UnityEngine.InputSystem;
using System.Linq;


/**
 * @brief Handles the main logic of the game, registering players and the distributing of incoming input to the right players
 */
public class TMGameManager : MonoBehaviour
{
    public static TMGameManager Instance { get; private set; }

    /**
    * @brief The amount of words that need to be typed correctly to win the game
    */
    public int wordsPerPlayer = 10;

    /**
    * @brief A list containing all the rows that a player could be attached to
    */
    public List<PlayerTypingController> rows; // list of row1, ro2, etc

    /**
    * @brief A list of all the spawns
    */
    public List<GameObject> spawns; // spawn1, spawn2

    /**
    * @brief Amount of players that have finished
    */
    public int finishCount = 0;

    /**
    * @brief Dict that links remote ids to PlayerTypingControllers
    */
    public Dictionary<string, PlayerTypingController> playerControllers;

    /**
    * @brief Dict that links PlayerTypingController to their connected VirtualController
    */
    public Dictionary<PlayerTypingController, VirtualController> playerVirtualControllers;

    /**
    * @brief List of PlayerInputs coming in from the JoinAll
    */
    public List<PlayerInput> playerInputs;

    /**
    * @brief Spawn object that contains the spawns for all possible player spawns
    */
    public GameObject spawn;

    /**
    * @brief The WinScreen name to transition to after the game is finished
    */
    public string WinScreen;

    private SwitchScene sceneSwitcher;

    /**
    * @brief This class is the message that contains which controller needs to load on the client
    * and the list of all the words the player needs to type to win
    */
    [Serializable]
    private class AllWordsMessage
    {
        public string type = "controller";
        public string controller = "text-preset";
        public List<string> words;
    }

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
        AudioManager.Instance.PlayRandomMiniGameTrack();
    }

    /**
    * @brief Registers the player by giving it a row with the player name, setting player color and face.
    */
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
        PlayerTypingController typingController = rows[playerIndex];
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
        SendAllWordsToClient(typingController);

        TextMeshProUGUI name = rows[playerIndex].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        name.text = PlayerManager.playerStats.Values.FirstOrDefault(p => p.playerID == playerIndex).name;
    }

    /**
    * @brief Pass a completed word to the PlayerTypingController of the right player
    */
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

    /**
    * @brief Send all the words that the player needs to type to the client device
    */
    public void SendAllWordsToClient(PlayerTypingController player)
    {
        // lookup their VirtualController
        if (!playerVirtualControllers.TryGetValue(player, out var vc))
        {
            Debug.LogWarning($"[TMGameManager] No controller for {player.name}");
            return;
        }
        string clientId = vc.remoteId;

        var words = player.textSpawner.spawnedWords;
        var msgObj = new AllWordsMessage { words = words };

        string json = JsonUtility.ToJson(msgObj);
        ServerManager.SendMessageToClient(clientId, json);
    }

    /**
    * @brief Once all put one player is finished end the game and go to the winscreen
    */
    public void OnPlayerFinished(PlayerTypingController player)
    {
        finishCount++;
        player.finishPostion = finishCount;
        player.wordsLeftText.text = FinishPositionIntToString(finishCount);
        Debug.Log($"{player.name} finished in position {finishCount}");
        player.inputField.interactable = false;

        TM_MusicController.Instance.PlayFinishSFX();

        int activePlayerCount = playerControllers.Count;
        if (finishCount == activePlayerCount || finishCount == activePlayerCount - 1)
        {
            Debug.Log("Game over!");
            StartCoroutine(HandleEndGameSequence());
        }
    }

    private IEnumerator HandleEndGameSequence()
    {
        yield return new WaitForSeconds(1f); // Wait for fade out to finish

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
}
