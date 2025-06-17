using UnityEngine;
using System.Collections.Generic;

public class BombManager : MonoBehaviour
{
    public MinigameHUDController hudController; // drag this in the Inspector

    public GameObject bombPrefab;
    private GameObject currentBomb;
    public List<GameObject> players = new List<GameObject>();

    [System.Obsolete]
    void Start()
    {
        // if (ServerManager.allControllers != null)
        // {
        //     foreach (var device in ServerManager.allControllers.Values.ToArray())
        //     {
        //         Debug.Log("Spawning...");
        //         PlayerInputManager.instance.playerPrefab = prefab;
        //         PlayerInput playerInput = PlayerInputManager.instance.JoinPlayer(-1, -1, null, device);
        //         if (playerInput != null)
        //         {
        //             colorTank(playerInput);
        //             GameManager.RegisterPlayerGame(playerInput);
        //         }
        //         else
        //         {
        //             Debug.LogError("PlayerInput == NULL");
        //         }
        //     }
        // }

        GameObject[] allObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            if (obj.name.Contains("Player") && players.Count < 4)
            {
                players.Add(obj);
            }
        }
        if (hudController != null)
        {
            hudController.OnCountdownFinished += StartHotPotato;
            hudController.ShowCountdown();
        }
    }

    void StartHotPotato()
    {
        Debug.Log("Countdown done — starting hot potato game.");
        hudController.StartGameTimer();
        SpawnBombOnRandomPlayer();
    }

    void Update()
    {
        players.RemoveAll(p => p == null);

        if (players.Count == 1)
        {
            Debug.Log("Gameover: winner " + players[0].name);
            return;
        }

        if (currentBomb == null && players.Count > 1)
        {
            SpawnBombOnRandomPlayer();
        }

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
}
