using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using System.Linq;

/**
 * @brief Handles scene transitions, including fade effects and optional auto-transition on start.
 */
public class SwitchScene : MonoBehaviour
{
    [Header("Fader Settings")]
    [SerializeField] private RectTransform fader; /**< UI element used to fade in/out during transitions. */
    [SerializeField] private float fadeDuration = 0.5f; /**< Duration of the fade animation. */

    [Header("Auto-Transition Settings")]
    [SerializeField] private bool autoTransitionOnStart = false; /**< If true, automatically transitions to another scene on start. */
    [SerializeField] private float delayBeforeTransition = 3f; /**< Delay before auto-transition begins. */
    [SerializeField] private string autoTransitionScene = "MobileSandbox"; /**< Name of the scene to auto-transition to. */

    private static bool hasLoadedMainMenuBefore = false; /**< Tracks whether the MainMenu has been loaded previously. */
    public bool skipNextStart = false; /**< If true, skips transition effects on next start. */

    /**
     * @brief Unity callback invoked on object initialization.
     *        Handles fade-in effects and optional auto-transition.
     * @return void
     */
    private void Start()
    {
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

    /**
     * @brief Loads a scene after a delay if auto-transition is enabled.
     * @return void
     */
    private void AutoTransition()
    {
        LoadNewScene(autoTransitionScene);
    }

    /**
     * @brief Loads a new scene with fade transition and performs cleanup if transitioning to MainMenu.
     * @param sceneName The name of the scene to load.
     * @return void
     */
    public void LoadNewScene(string sceneName)
    {
        Debug.Log("Loading scene: " + sceneName);

        if (sceneName == "MainMenu")
        {
            // Disconnect networking
            var serverManager = Object.FindFirstObjectByType<ServerManager>();
            if (serverManager != null)
            {
                serverManager.CloseConnections();
                Destroy(serverManager.gameObject);
            }

            // Clean up players
            var inputManager = Object.FindFirstObjectByType<PlayerInputManager>();
            if (inputManager != null)
            {
                inputManager.DisableJoining();
            }

            foreach (var player in PlayerInput.all.ToList())
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

        // Do the scene transition
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

    /**
     * @brief Loads a new scene additively with a fade transition.
     * @param sceneName The name of the additive scene to load.
     * @return void
     */
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
