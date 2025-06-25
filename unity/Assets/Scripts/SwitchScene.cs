using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;

public class SwitchScene : MonoBehaviour
{
    [Header("Fader Settings")]
    [SerializeField] private RectTransform fader;
    [SerializeField] private float fadeDuration = 0.5f;

    [Header("Auto-Transition Settings")]
    [SerializeField] private bool autoTransitionOnStart = false;
    [SerializeField] private float delayBeforeTransition = 3f;
    [SerializeField] private string autoTransitionScene = "MobileSandbox";

    private static bool hasLoadedMainMenuBefore = false;
    public bool skipNextStart = false;

    private void Start()
    {
        autoTransitionOnStart = false;
        if (fader != null)
        {
            string currentScene = SceneManager.GetActiveScene().name;
            Debug.Log("Current scene: " + currentScene);

            if (currentScene == "MainMenu")
            {
                var serverManager = Object.FindFirstObjectByType<ServerManager>();
                if (serverManager != null)
                {
                    ServerManager.takenColors.Clear();
                    Destroy(serverManager.gameObject);
                }

                if (!hasLoadedMainMenuBefore)
                {
                    hasLoadedMainMenuBefore = true;
                    fader.gameObject.SetActive(false);
                    return;
                }
            }

            fader.gameObject.SetActive(true);
            LeanTween.scale(fader, Vector3.one, 0f);
            LeanTween.scale(fader, Vector3.zero, fadeDuration)
                .setEase(LeanTweenType.easeInOutQuint)
                .setOnComplete(() => fader.gameObject.SetActive(false));
        }

        if (autoTransitionOnStart)
        {
            Invoke(nameof(AutoTransition), delayBeforeTransition);
        }
    }


    private void AutoTransition()
    {
        LoadNewScene(autoTransitionScene);
    }

    public void LoadNewScene(string sceneName)
    {
        Debug.Log("Loading scene: " + sceneName);

        if (sceneName == "MainMenu")
        {
            // --- Disconnect networking ---
            var serverManager = Object.FindFirstObjectByType<ServerManager>();
            if (serverManager != null)
            {
                serverManager.CloseConnections();
                Destroy(serverManager.gameObject);
            }

            // --- Clean up players ---
            var inputManager = Object.FindFirstObjectByType<PlayerInputManager>();
            if (inputManager != null)
            {
                inputManager.DisableJoining();
            }

            foreach (var player in PlayerInput.all.ToList()) // Make a copy to safely remove
            {
                Destroy(player.gameObject);
            }

            // Remove the reconnect canvas
            var reconnectCanvas = FindFirstObjectByType<Reconnect>();
            if (reconnectCanvas != null)
            {
                inputManager.DisableJoining();
            }
        }

        // --- Do the scene transition ---
        if (fader != null)
        {
            fader.gameObject.SetActive(true);
            LeanTween.scale(fader, Vector3.zero, 0f);
            LeanTween.scale(fader, Vector3.one, fadeDuration)
                .setEase(LeanTweenType.easeInOutQuint)
                .setOnComplete(() => SceneManager.LoadScene(sceneName));
        }
        else
        {
            SceneManager.LoadScene(sceneName);
        }
    }


    public void LoadSceneAdditive(string sceneName)
    {
        if (fader != null)
        {
            fader.gameObject.SetActive(true);
            LeanTween.scale(fader, Vector3.zero, 0f);
            LeanTween.scale(fader, Vector3.one, fadeDuration)
                .setEase(LeanTweenType.easeInOutQuint)
                .setOnComplete(() => SceneManager.LoadScene(sceneName, LoadSceneMode.Additive));
        }
        else
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
        }
    }
}

