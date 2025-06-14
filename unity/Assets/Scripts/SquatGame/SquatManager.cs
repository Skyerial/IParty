using UnityEngine;
using System.Collections.Generic;

public class SquatManager : MonoBehaviour
{
    public List<GameObject> playerList = new List<GameObject>();
    private bool gameEnded = false;
    private float gameDuration = 15f;
    private float timer = 0f;

    void Start()
    {
        StartNewRound();
    }

    void Update()
    {
        if (!gameEnded)
        {
            timer += Time.deltaTime;
            if (timer >= gameDuration)
            {
                EndGame();
            }
        }
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

        int highestPressCount = 1;

        // First pass: find the highest mash count
        foreach (GameObject player in playerList)
        {
            PlayerMash mash = player.GetComponent<PlayerMash>();
            if (mash != null)
            {
                int count = mash.GetMashCounter();
                if (count > highestPressCount)
                {
                    highestPressCount = count;
                }
            }
        }

        foreach (GameObject player in playerList)
        {
            PlayerMash mash = player.GetComponent<PlayerMash>();
            if (mash != null)
            {
                int mashCount = mash.GetMashCounter();
                float floatHeight = mashCount * 1f;

                mash.TriggerFloatAnimation(floatHeight);
            }
        }
    }
}
