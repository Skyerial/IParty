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
}