using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using Unity.VisualScripting;
using UnityEngine.Video;

public class TMGameManager : MonoBehaviour
{
    public static TMGameManager Instance { get; private set; }

    public int wordsPerPlayer = 10;

    public List<PlayerTypingController> players; // list of row1, ro2, etc

    private bool gameStarted = false;

    public int finishCount = 0;

    public Dictionary<string, PlayerTypingController> playerControllers;
    public Dictionary<PlayerTypingController, VirtualController> playerVirtualControllers;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        gameStarted = true;
        Debug.Log("Game started!");

        TM_MusicController.Instance.FadeInBGM(2f); // fade in over 2 seconds

        AttachMobilePlayer();
    }

    private void AttachMobilePlayer()
    {
        int i = 0;

        playerControllers = new Dictionary<string, PlayerTypingController>();
        playerVirtualControllers = new Dictionary<PlayerTypingController, VirtualController>();

        foreach (var mobilePlayer in PlayerManager.playerStats)
        {
            if (i >= players.Count) break;

            var controller = mobilePlayer.Key as VirtualController;
            if (controller == null)
            {
                Debug.LogWarning("Controller is not a VirtualController. Skipping.");
                continue;
            }
            var typingController = players[i];

            playerControllers[controller.remoteId] = typingController;
            playerVirtualControllers[typingController] = controller;

            typingController.textSpawner.words = wordsPerPlayer;
            typingController.textSpawner.SpawnWords();
            typingController.raceController.InitializeRace(wordsPerPlayer);
            typingController.Initialize();
            typingController.inputField.interactable = true;
            typingController.inputField.text = "";

            TextMeshProUGUI name = players[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            name.text = mobilePlayer.Value.name;

            i++;
        }

        while (i < players.Count)
        {
            players[i].gameObject.SetActive(false);
            i++;
        }
    }

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

    public void OnPlayerFinished(PlayerTypingController player)
    {
        finishCount++;
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
    }

}
