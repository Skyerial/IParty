using TMPro;
using UnityEngine;

public class ScoreDisplay : MonoBehaviour
{
    public TMP_Text scoreText;
    private int hits = 0;

    public void AddMoleHit()
    {
        // hits++;
        // UpdateDisplay();
    }

    public void RemoveMoleHit()
    {
        hits = Mathf.Max(0, hits - 1);
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        scoreText.text = $"Hits: {hits}";
    }
}
