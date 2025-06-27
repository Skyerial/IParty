using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * @brief Handles scene changes and sending preset messages through the ServerManager.
 */
public class ChangeScene : MonoBehaviour
{
    /**
     * @brief Reloads a level based on the value selected in a TMP_Dropdown.
     * The dropdown must contain 12 options corresponding to specific scene names.
     * Loads the selected scene using UnityEngine.SceneManagement.
     */
    public void ReloadLevel()
    {
        TMP_Dropdown levels = FindAnyObjectByType<TMP_Dropdown>();

        string level = "";
        switch (levels.value)
        {
            case 0:
                level = "Spleef";
                break;
            case 1:
                level = "Turf";
                break;
            case 2:
                level = "TankGame";
                break;
            case 3:
                level = "TypingMinigame";
                break;
            case 4:
                level = "Game_Board";
                break;
            case 5:
                level = "HotPotato";
                break;
            case 6:
                level = "Winscreen";
                break;
            case 7:
                level = "GYRO";
                break;
            case 8:
                level = "SkyGlutes";
                break;
            case 9:
                level = "NewBoard";
                break;
            case 10:
                level = "Winscreen3D";
                break;
            case 11:
                level = "SetGame";
                break;
        }
        Debug.Log(level);
        SceneManager.LoadScene(level);
    }

    /**
     * @brief Sends a "dpad-preset" message to all connected sockets.
     * Used for testing d-pad related input functionality.
     */
    public void SendDpad()
    {
        Debug.Log("Test data send.");
        ServerManager.SendtoAllSockets("dpad-preset");
    }

    /**
     * @brief Sends a "joystick-preset" message to all connected sockets.
     * Used for testing joystick-related input functionality.
     */
    public void SendJoystick()
    {
        Debug.Log("Test data send.");
        ServerManager.SendtoAllSockets("joystick-preset");
    }

    /**
     * @brief Sends a "text-preset" message to all connected sockets.
     * Used for testing textbox input or commands.
     */
    public void SendTextBox()
    {
        ServerManager.SendtoAllSockets("text-preset");
    }

    /**
     * @brief Sends a "one-button" message to all connected sockets.
     * Used for testing a single button input event.
     */
    public void SendSingleButton()
    {
        Debug.Log("Test data send. SINGLE BUTTON");
        ServerManager.SendtoAllSockets("one-button");
    }
}
