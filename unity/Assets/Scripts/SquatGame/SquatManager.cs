using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SquatManager : MonoBehaviour
{
    public static bool inputEnabled = false;

    public List<GameObject> playerList = new List<GameObject>();
    public List<GameObject> rankingList = new List<GameObject>();

    private bool gameEnded = false;
    private float gameDuration = 15f;
    private GameObject highestPlayer = null;

    [SerializeField] private float floatStartDelay = 2f;

    private float timer = 0f;

    void Start()
    {
        StartCoroutine(CountdownThenStart());
    }

    void Update()
    {
        if (!gameEnded && inputEnabled)
        {
            timer += Time.deltaTime;
            if (timer >= gameDuration)
            {
                EndGame();
            }
        }
    }

    IEnumerator CountdownThenStart()
    {
        inputEnabled = false;
        yield return new WaitForSeconds(4f);

        StartNewRound();
        inputEnabled = true;
    }

    void StartNewRound()
    {
        gameEnded = false;
        timer = 0f;

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
        inputEnabled = false;

        // Step 1: collect all mash counts
        List<(GameObject player, int mashCount)> rankings = new List<(GameObject, int)>();

        foreach (GameObject player in playerList)
        {
            PlayerMash mash = player.GetComponent<PlayerMash>();
            if (mash != null)
            {
                int count = mash.GetMashCounter();
                rankings.Add((player, count));
            }
        }

        // Step 2: sort by mash count descending
        rankings.Sort((a, b) => b.mashCount.CompareTo(a.mashCount));

        // Step 3: save to rankingList
        rankingList.Clear();
        foreach (var entry in rankings)
        {
            rankingList.Add(entry.player);
        }

        // Step 4: set highest player for camera
        if (rankingList.Count > 0)
        {
            highestPlayer = rankingList[0];
        }

        // Step 5: start float animation after delay
        StartCoroutine(DelayedFloatAnimation(floatStartDelay));
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

        // Optional: log the ranking
        for (int i = 0; i < rankingList.Count; i++)
        {
            Debug.Log($"{i + 1} place: {rankingList[i].name}");
        }
    }
}
