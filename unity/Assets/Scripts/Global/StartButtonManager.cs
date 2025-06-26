using UnityEngine;
using UnityEngine.UI;

public class StartButtonManager : MonoBehaviour
{
    public Button startButton;
    public int minPlayers = 2;

    void Update()
    {
        if (startButton != null)
        {
            // Enable only if there are at least 2 players
            startButton.interactable = PlayerManager.playerStats.Count >= 2;
        }
    }
}
