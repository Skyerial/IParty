using UnityEngine;
using UnityEngine.UI;

/**
 * @brief Enables or disables the start button based on the number of joined players.
 */
public class StartButtonManager : MonoBehaviour
{
    /**
     * @brief Reference to the UI Button that starts the game.
     */
    public Button startButton;
    /**
     * @brief Minimum number of players required to enable the start button.
     */
    public int minPlayers = 2;

    /**
     * @brief Unity event called once per frame; updates the start button's interactable state.
     */
    void Update()
    {
        if (startButton != null)
        {
            // Check if the number of players is greater than or equal to the minimum required
            startButton.interactable = PlayerManager.playerStats.Count >= minPlayers;
        }
    }
}
