using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TMGameManager : MonoBehaviour
{
    public static TMGameManager Instance { get; private set; }

    public List<PlayerTypingController> players;

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

        foreach (var player in players)
        {
            player.inputField.interactable = true;
            player.inputField.text = "";
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
