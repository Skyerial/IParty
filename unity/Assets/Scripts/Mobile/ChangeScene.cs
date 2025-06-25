using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
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
                SendTextBox();
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
        }
        Debug.Log(level);
        SceneManager.LoadScene(level);
    }

    public void SendDpad()
    {
        Debug.Log("Test data send.");
        ServerManager.SendtoAllSockets("dpad-preset");
    }

    public void SendJoystick()
    {
        Debug.Log("Test data send.");
        ServerManager.SendtoAllSockets("joystick-preset");
    }

    public void SendTextBox()
    {
        ServerManager.SendtoAllSockets("text-preset");
    }

    public void SendSingleButton()
    {
        Debug.Log("Test data send. SINGLE BUTTON");
        ServerManager.SendtoAllSockets("one-button");
    }
    
}