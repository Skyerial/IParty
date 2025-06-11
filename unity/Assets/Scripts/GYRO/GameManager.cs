using System.Collections;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [Header("Game Objects")]
    [Tooltip("Drag your Cup object from the Hierarchy into this slot.")]
    public GameObject cup;

    [Tooltip("Drag a UI Text - TextMeshPro object here for status updates.")]
    public TextMeshProUGUI statusText;

    [Header("Timing Settings")]
    [Tooltip("The minimum time the cup will stay covered.")]
    public float minCoveredTime = 2.0f;

    [Tooltip("The maximum time the cup will stay covered.")]
    public float maxCoveredTime = 5.0f;

    private bool isCupOpen = false;
    private bool roundOver = false;

    void Start()
    {
        StartCoroutine(GameLoop());
    }

    public void PlayerAttemptedHit(int playerID)
    {
        // Check 1: Is the cup open?
        // Check 2: Has someone else already won this round?
        if (isCupOpen && !roundOver)
        {
            roundOver = true; // Mark the round as over so no one else can win.
            Debug.Log($"Player {playerID} WINS THE ROUND!");
            statusText.text = $"Player {playerID} Wins!";
        }
        else if (!isCupOpen)
        {
            // --- TOO EARLY! ---
            Debug.Log($"Player {playerID} hit too early!");
            // You could add a penalty or message here if you want.
            // For now, we'll just ignore it.
        }
    }

    IEnumerator GameLoop()
    {
        while (true)
        {
            roundOver = false;
            isCupOpen = false;
            cup.SetActive(true);
            statusText.text = "Get Ready...";
            Debug.Log("Cup is ON. Waiting for random time...");


            float randomWait = Random.Range(minCoveredTime, maxCoveredTime);
            yield return new WaitForSeconds(randomWait);

            // --- 2. THE ACTION PHASE ---
            isCupOpen = true; // The window to hit is now open!
            cup.SetActive(false); // Reveal the mosquito
            statusText.text = "SMACK IT!";
            Debug.Log("Cup is OFF! Mosquito is revealed. Waiting for input...");

            // THIS IS THE KEY CHANGE:
            // Instead of waiting for a fixed time, we now wait UNTIL a player wins.
            // The 'roundOver' variable will be set to 'true' by the PlayerAttemptedHit() function.
            yield return new WaitUntil(() => roundOver == true);

            // --- 3. END OF ROUND ---
            Debug.Log("Round over. Resetting in 3 seconds...");
            cup.SetActive(true); // Cover the mosquito again for the next round.
            yield return new WaitForSeconds(3f); // A 3-second pause to see who won.
        }
    }
}