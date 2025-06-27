using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * @brief Button to start a selected minigame.
 * This script is attached to a button that, when clicked,
 * loads the currently selected minigame scene.
 */
public class StartMinigameButton : MonoBehaviour
{
    public void LoadSelectedMinigame()
    {
        if (!string.IsNullOrEmpty(PlayerManager.currentMinigame))
        {
            Debug.Log("check check");
            SceneManager.LoadScene(PlayerManager.currentMinigame);
        }
        else
        {
            Debug.LogWarning("No minigame selected in PlayerManager.");
        }
    }
}
