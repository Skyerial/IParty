using UnityEngine;
using UnityEngine.SceneManagement;

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
