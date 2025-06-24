using UnityEngine;
using UnityEngine.SceneManagement;

public class SwitchScene : MonoBehaviour
{

    [SerializeField] RectTransform fader;
    private static bool hasLoadedMainMenuBefore = false;

    private void Start()
    {
        string currentScene = SceneManager.GetActiveScene().name;
        Debug.Log(currentScene);

        if (currentScene == "MainMenu")
        {
            var serverManager = Object.FindFirstObjectByType<ServerManager>();
            if (serverManager != null)
            {
                Destroy(serverManager.gameObject);
            }
        }

        fader.gameObject.SetActive(true);
        LeanTween.scale(fader, new Vector3(1, 1, 1), 0f);
        LeanTween.scale(fader, Vector3.zero, 0.5f)
            .setEase(LeanTweenType.easeInOutQuint)
            .setOnComplete(() =>
            {
                fader.gameObject.SetActive(false);
            });
    }
    public void LoadNewScene(string sceneName)
    {
        Debug.Log("Loading scene: " + sceneName);
        fader.gameObject.SetActive(true);
        LeanTween.scale(fader, Vector3.zero, 0f);
        LeanTween.scale(fader, new Vector3(1, 1, 1), 0.5f).setEase(LeanTweenType.easeInOutQuint).setOnComplete(() =>
       {
           SceneManager.LoadScene(sceneName);
       });

    }
    public void LoadSceneAdditive(string sceneName)
    {
        fader.gameObject.SetActive(true);
        LeanTween.scale(fader, Vector3.zero, 0f);
        LeanTween.scale(fader, new Vector3(1, 1, 1), 0.5f).setEase(LeanTweenType.easeInOutQuint).setOnComplete(() =>
       {
           SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
       });
    }
}
