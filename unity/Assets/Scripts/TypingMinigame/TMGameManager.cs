using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using Unity.VisualScripting;

public class TMGameManager : MonoBehaviour
{
    public static TMGameManager Instance { get; private set; }

    public int wordsPerPlayer = 10;

    public List<PlayerTypingController> players; // list of row1, ro2, etc

    private bool gameStarted = false;

    public int finishCount = 0;

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

        // foreach (var player in players)
        // {
        //     player.textSpawner.words = wordsPerPlayer;
        //     player.textSpawner.SpawnWords();

        //     player.raceController.InitializeRace(wordsPerPlayer);
        //     player.Initialize();

        //     player.inputField.interactable = true;
        //     player.inputField.text = "";
        // }
    }

    private void AttachMobilePlayer()
    {
        int i = 0;
        foreach (var mobilePlayer in PlayerManager.playerStats)
        {
            players[i].textSpawner.words = wordsPerPlayer;
            players[i].textSpawner.SpawnWords();
            players[i].raceController.InitializeRace(wordsPerPlayer);
            players[i].Initialize();
            players[i].inputField.interactable = true;
            players[i].inputField.text = "";


            TextMeshProUGUI name = players[i].transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            name.text = mobilePlayer.Value.name;

            i++;
        }

        while (i < 4)
        {
            players[i].gameObject.GetComponent<Renderer>().enabled = false;
            i++;
        }
    }

    public void OnPlayerFinished(PlayerTypingController player)
    {
        finishCount++;
        Debug.Log($"{player.name} finished in position {finishCount}");
        player.inputField.interactable = false;

        TM_MusicController.Instance.PlayFinishSFX();

        if (finishCount == 4)
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
