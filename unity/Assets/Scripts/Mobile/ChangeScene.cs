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
        }
        Debug.Log(level);
        SceneManager.LoadScene(level);
    }
}