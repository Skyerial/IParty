using UnityEngine;
using UnityEngine.SceneManagement;

public class FrameRateManager : MonoBehaviour
{
    public static FrameRateManager Instance;

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

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string name = scene.name;

        if (name == "MainMenu" || name == "Lobby")
        {
            SetFrameRate(30);
        }
        else
        {
            EnableVSync();
        }
    }

    private void SetFrameRate(int fps)
    {
        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = fps;
        Debug.Log($"[FrameRateManager] vSync OFF, FPS capped at {fps}");
    }

    private void EnableVSync()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = -1;
        Debug.Log("[FrameRateManager] vSync ON, uncapped FPS");
    }
}
