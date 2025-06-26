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
            // Check if the number of players is greater than or equal to the minimum required
            startButton.interactable = PlayerManager.playerStats.Count >= minPlayers;
        }
    }
}
