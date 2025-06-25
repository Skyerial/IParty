using UnityEngine;
using UnityEngine.SceneManagement;

/**
 * @brief Controls frame rate settings based on the active scene.
 * Applies a 30 FPS cap in menu scenes and vSync elsewhere for efficiency.
 */
public class FrameRateManager : MonoBehaviour
{
    /**
     * @brief Singleton instance for global access.
     */
    public static FrameRateManager Instance;

    /**
     * @brief Called when the object is initialized.
     * Sets up the singleton and listens for scene changes.
     */
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /**
     * @brief Called when a new scene is loaded.
     * Applies different frame settings depending on scene name.
     * @param scene The loaded scene.
     * @param mode The scene load mode.
     */
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string name = scene.name;

        if (name == "MainMenu" || name == "WinScreen")
        {
            SetFrameRate(30);
        }
        else if (name == "Lobby" || name == "Game_Board" || name == "WinScreen3D")
        {
            SetFrameRate(60);
        }
        else
        {
            EnableVSync();
        }
    }

    /**
     * @brief Disables vSync and applies a fixed frame rate cap.
     * @param fps The target frames per second.
     */
    private void SetFrameRate(int fps)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
        Debug.Log($"[FrameRateManager] vSync OFF, FPS capped at {fps}");
    }

    /**
     * @brief Enables vSync and removes any frame rate cap.
     */
    private void EnableVSync()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = -1;
        Debug.Log("[FrameRateManager] vSync ON, uncapped FPS");
    }
}
