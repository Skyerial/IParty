using TMPro;
using UnityEngine;

/**
 * @brief Displays and updates the player's mole hit score on the UI.
 */
public class ScoreDisplay : MonoBehaviour
{
    /**
     * @brief Reference to the TMP_Text component used to show the score.
     */
    public TMP_Text scoreText;

    /**
     * @brief Internal counter for how many moles the player has hit.
     */
    private int hits = 0;

    /**
     * @brief Increments the hit counter and updates the UI display.
     */
    public void AddMoleHit()
    {
        hits++;
        UpdateDisplay();
    }

    /**
     * @brief Decrements the hit counter (minimum 0) and updates the UI display.
     */
    public void RemoveMoleHit()
    {
        hits = Mathf.Max(0, hits - 1);
        UpdateDisplay();
    }

    /**
     * @brief Updates the score text with the current hit count.
     */
    private void UpdateDisplay()
    {
        scoreText.text = $"Hits: {hits}";
    }
}
