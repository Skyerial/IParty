using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SquatManager : MonoBehaviour
{
    public static bool inputEnabled = false;

    public List<GameObject> playerList = new List<GameObject>();
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

        int highestPressCount = 1;
        highestPlayer = null;

        foreach (GameObject player in playerList)
        {
            PlayerMash mash = player.GetComponent<PlayerMash>();
            if (mash != null)
            {
                int count = mash.GetMashCounter();
                if (count > highestPressCount)
                {
                    highestPressCount = count;
                    highestPlayer = player;
                }
            }
        }

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
    }
}
