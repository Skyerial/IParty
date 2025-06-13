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

        int highestPressCount = 0;
        PlayerMash winner = null;

        foreach (GameObject player in playerList)
        {
            PlayerMash mash = player.GetComponent<PlayerMash>();
            if (mash != null)
            {
                int currentMashCount = mash.GetMashCounter();
                if (currentMashCount > highestPressCount)
                {
                    highestPressCount = currentMashCount;
                    winner = mash;
                }
            }
        }

        foreach (GameObject player in playerList)
        {
            PlayerMash mash = player.GetComponent<PlayerMash>();
            if (mash != null)
            {
                int mashCount = mash.GetMashCounter();
                float floatHeight = Mathf.Clamp(mashCount * 1f, 0f, 10f);
                mash.TriggerFloatAnimation(floatHeight);
            }
        }

    }
}
