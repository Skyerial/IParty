using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SquatManager : MonoBehaviour
{
    public static bool inputEnabled = false;

    public List<GameObject> playerList = new List<GameObject>();
    public List<GameObject> rankingList = new List<GameObject>();


    [SerializeField] private float floatStartDelay = 2f;
    [SerializeField] private MinigameHUDController hudController;


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

        // Abonneer op HUD events
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
                rankings.Add((player, mash.GetshCounter()));
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
}
